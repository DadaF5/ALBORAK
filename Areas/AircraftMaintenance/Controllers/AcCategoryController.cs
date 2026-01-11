using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class AcCategoryController : Controller
    {
        private readonly FRAContext _context;

        public AcCategoryController(FRAContext context)
        {
            _context = context;
        }

        // GET: AcCategory
        public async Task<IActionResult> Index()
        {
            var categories = await _context.AcCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        // GET: AcCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AcCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.AcCategories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: AcCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.AcCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: AcCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcCategory category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcCategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: AcCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.AcCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: AcCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.AcCategories.FindAsync(id);
            _context.AcCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AcCategoryExists(int id)
        {
            return _context.AcCategories.Any(e => e.Id == id);
        }
    }

}