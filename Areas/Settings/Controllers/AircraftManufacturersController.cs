using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    public class AircraftManufacturersController : Controller
    {
        private readonly FRAContext _context;

        public AircraftManufacturersController(FRAContext context)
        {
            _context = context;
        }

        // GET: Settings/AircraftManufacturers
        public async Task<IActionResult> Index()
        {
            var list = await _context.AircraftManufacturers
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return View(list);
        }

        // GET: Settings/AircraftManufacturers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftManufacturers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return NotFound();

            return View(entity);
        }

        // GET: Settings/AircraftManufacturers/Create
        public IActionResult Create()
        {
            return View(new AircraftManufacturer());
        }

        // POST: Settings/AircraftManufacturers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftManufacturer entity)
        {
            var normalizedCode = entity.Code?.Trim().ToUpperInvariant() ?? string.Empty;

            if (await _context.AircraftManufacturers.AnyAsync(x => x.Code.Trim().ToUpper() == normalizedCode))
            {
                ModelState.AddModelError(nameof(entity.Code), "Code already exists.");
            }

            if (!ModelState.IsValid) return View(entity);

            entity.Code = normalizedCode;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.Description = string.IsNullOrWhiteSpace(entity.Description) ? null : entity.Description.Trim();

            _context.AircraftManufacturers.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Manufacturer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/AircraftManufacturers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftManufacturers.FindAsync(id);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST: Settings/AircraftManufacturers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftManufacturer entity)
        {
            if (id != entity.Id) return NotFound();

            var normalizedCode = entity.Code?.Trim().ToUpperInvariant() ?? string.Empty;

            if (await _context.AircraftManufacturers.AnyAsync(x => x.Id != entity.Id && x.Code.Trim().ToUpper() == normalizedCode))
            {
                ModelState.AddModelError(nameof(entity.Code), "Code already exists.");
            }

            if (!ModelState.IsValid) return View(entity);

            entity.Code = normalizedCode;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.Description = string.IsNullOrWhiteSpace(entity.Description) ? null : entity.Description.Trim();

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.AircraftManufacturers.AnyAsync(e => e.Id == entity.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = "Manufacturer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/AircraftManufacturers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftManufacturers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST: Settings/AircraftManufacturers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.AircraftManufacturers.FindAsync(id);
            if (entity != null)
            {
                entity.IsActive = false;
                _context.AircraftManufacturers.Update(entity);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Manufacturer deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}