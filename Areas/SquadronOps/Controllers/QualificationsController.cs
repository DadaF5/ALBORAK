using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using FRAProject.Areas.SquadronOps.Models;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all. Qualification is global
    // reference data (no Squadron/Base/AcMainGroup of its own, same shape
    // as Phase/Ata), so no data scoping is added — just the missing
    // module-level policy gate.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class QualificationsController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<QualificationsController> _logger;

        public QualificationsController(FRAContext context, ILogger<QualificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Qualifications
        // searchString - text to search in Name/Description
        // sortOrder - "name_asc" (default), "name_desc", "type_asc", "type_desc", "active_asc", "active_desc"
        // pageNumber & pageSize - simple paging
        public async Task<IActionResult> Index(string sortOrder, string? searchString, int pageNumber = 1, int pageSize = 25)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageSize"] = pageSize;

            IQueryable<Qualification> query = _context.Qualifications.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(q =>
                    EF.Functions.Like(q.Name, $"%{s}%") ||
                    EF.Functions.Like(q.Description ?? string.Empty, $"%{s}%") ||
                    EF.Functions.Like(q.QualificationType, $"%{s}%"));
            }

            // Sorting
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(q => q.Name),
                "type_asc" => query.OrderBy(q => q.QualificationType).ThenBy(q => q.Name),
                "type_desc" => query.OrderByDescending(q => q.QualificationType).ThenByDescending(q => q.Name),
                "active_asc" => query.OrderBy(q => q.Active).ThenBy(q => q.Name),
                "active_desc" => query.OrderByDescending(q => q.Active).ThenByDescending(q => q.Name),
                "name_asc" or _ => query.OrderBy(q => q.Name)
            };

            // Simple paging
            pageNumber = Math.Max(1, pageNumber);
            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["TotalItems"] = totalItems;
            ViewData["PageNumber"] = pageNumber;

            return View(items); // expects a view that takes IEnumerable<Qualification>
        }

        // GET: Qualifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var qualification = await _context.Qualifications
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (qualification == null) return NotFound();

            return View(qualification);
        }

        // GET: Qualifications/Create
        [Authorize(Policy = "SquadronOpsWrite")]
        public IActionResult Create() => View();

        // POST: Qualifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create([Bind("Name,Description,QualificationType,Active")] Qualification qualification)
        {
            if (!ModelState.IsValid) return View(qualification);

            // Prevent duplicates: same Name (case-insensitive) and QualificationType
            var name = qualification.Name.Trim();
            var type = (qualification.QualificationType ?? "Other").Trim();
            var exists = await _context.Qualifications
                .AnyAsync(q => q.Name.ToLower() == name.ToLower() && q.QualificationType == type);

            if (exists)
            {
                ModelState.AddModelError(nameof(Qualification.Name), "A qualification with the same name and type already exists.");
                return View(qualification);
            }

            qualification.Name = name;
            qualification.QualificationType = type;

            _context.Add(qualification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Qualifications/Edit/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var qualification = await _context.Qualifications.FindAsync(id);
            if (qualification == null) return NotFound();

            return View(qualification);
        }

        // POST: Qualifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,QualificationType,Active")] Qualification qualification)
        {
            if (id != qualification.Id) return BadRequest();

            if (!ModelState.IsValid) return View(qualification);

            // Prevent duplicates excluding current record
            var name = qualification.Name.Trim();
            var type = (qualification.QualificationType ?? "Other").Trim();
            var exists = await _context.Qualifications
                .AnyAsync(q => q.Id != qualification.Id && q.Name.ToLower() == name.ToLower() && q.QualificationType == type);

            if (exists)
            {
                ModelState.AddModelError(nameof(Qualification.Name), "A qualification with the same name and type already exists.");
                return View(qualification);
            }

            try
            {
                qualification.Name = name;
                qualification.QualificationType = type;
                _context.Update(qualification);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QualificationExists(qualification.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Qualifications/Delete/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var qualification = await _context.Qualifications
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (qualification == null) return NotFound();

            return View(qualification);
        }

        // POST: Qualifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var qualification = await _context.Qualifications.FindAsync(id);
            if (qualification != null)
            {
                _context.Qualifications.Remove(qualification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // API: GET: Qualifications/IsDuplicate?name=...&type=...
        [HttpGet]
        public async Task<IActionResult> IsDuplicate(string name, string type, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(false);

            name = name.Trim();
            type = (type ?? "Other").Trim();

            var exists = await _context.Qualifications
                .AnyAsync(q => q.Name.ToLower() == name.ToLower()
                               && q.QualificationType == type
                               && (!excludeId.HasValue || q.Id != excludeId.Value));

            return Json(exists);
        }

        private bool QualificationExists(int id)
            => _context.Qualifications.Any(e => e.Id == id);
    }
}
