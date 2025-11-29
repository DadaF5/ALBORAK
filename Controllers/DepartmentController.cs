using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly FRAContext _context;

        public DepartmentController(FRAContext context)
        {
            _context = context;
        }

        // ===========================
        // INDEX
        // ===========================
        public async Task<IActionResult> Index(int? filterBaseId, string? searchTerm)
        {
            var query = _context.Departments.Include(d => d.Base).AsQueryable();

            if (filterBaseId.HasValue)
                query = query.Where(d => d.BaseId == filterBaseId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(d => d.Name.Contains(searchTerm));

            var vm = new DepartmentViewModel
            {
                Departments = await query.OrderBy(d => d.Base.BaseName).ThenBy(d => d.Name).ToListAsync(),
                FilterBaseId = filterBaseId,
                SearchTerm = searchTerm,

                // 🔥 IMPORTANT LINE – populate dropdown inside ViewModel
                Bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.BaseName
                    })
                    .ToListAsync()


            };

            //ViewBag.Bases = await _context.Bases
            //    .OrderBy(b => b.BaseName)
            //    .Select(b => new SelectListItem
            //    {
            //        Value = b.Id.ToString(),
            //        Text = b.BaseName
            //    }).ToListAsync();

            return View(vm);
        }

        // ===========================
        // CREATE GET
        // ===========================
        public async Task<IActionResult> Create()
        {
            ViewBag.Bases = await GetBasesSelectListAsync();
            return View();
        }

        // ===========================
        // CREATE POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Bases = await GetBasesSelectListAsync();
                return View(createDto);
            }

            // Duplicate check
            bool exists = await _context.Departments
                .AnyAsync(d => d.BaseId == createDto.BaseId &&
                               d.Name.ToLower() == createDto.Name.Trim().ToLower());

            if (exists)
            {
                ModelState.AddModelError("Name", "This Department already exists for the selected Base.");
                ViewBag.Bases = await GetBasesSelectListAsync();
                return View(createDto);
            }

            var entity = new Department
            {
                Name = createDto.Name.Trim(),
                Description = createDto.Description,
                BaseId = createDto.BaseId
            };

            _context.Departments.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // EDIT GET
        // ===========================
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Departments.FindAsync(id);
            if (entity == null) return NotFound();

            var editDto = new DepartmentEditDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                BaseId = entity.BaseId
            };

            ViewBag.Bases = await GetBasesSelectListAsync();
            return View(editDto);
        }

        // ===========================
        // EDIT POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentEditDto editDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Bases = await GetBasesSelectListAsync();
                return View(editDto);
            }

            // Duplicate check
            bool exists = await _context.Departments
                .AnyAsync(d => d.BaseId == editDto.BaseId &&
                               d.Name.ToLower() == editDto.Name.Trim().ToLower() &&
                               d.Id != editDto.Id);

            if (exists)
            {
                ModelState.AddModelError("Name", "This Department already exists for the selected Base.");
                ViewBag.Bases = await GetBasesSelectListAsync();
                return View(editDto);
            }

            var entity = await _context.Departments.FindAsync(editDto.Id);
            if (entity == null) return NotFound();

            entity.Name = editDto.Name.Trim();
            entity.Description = editDto.Description;
            entity.BaseId = editDto.BaseId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // DELETE
        // ===========================
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Departments
                .Include(d => d.Base)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (entity == null) return NotFound();

            _context.Departments.Remove(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // HELPER: Get Bases dropdown
        // ===========================
        private async Task<List<SelectListItem>> GetBasesSelectListAsync()
        {
            return await _context.Bases
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BaseName
                })
                .ToListAsync();
        }
    }
}
