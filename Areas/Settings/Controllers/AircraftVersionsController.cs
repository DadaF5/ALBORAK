using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    public class AircraftVersionsController : Controller
    {
        private readonly FRAContext _context;

        public AircraftVersionsController(FRAContext context)
        {
            _context = context;
        }

        // GET: Settings/AircraftVersions
        public async Task<IActionResult> Index()
        {
            var list = await _context.AircraftVersions
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return View(list);
        }

        // GET: Settings/AircraftVersions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftVersions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // GET: Settings/AircraftVersions/Create
        public IActionResult Create()
        {
            return View(new AircraftVersion());
        }

        // POST: Settings/AircraftVersions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftVersion entity)
        {
            if (await _context.AircraftVersions.AnyAsync(x => x.Code == entity.Code))
            {
                ModelState.AddModelError(nameof(entity.Code), "Code already exists.");
            }

            if (!ModelState.IsValid) return View(entity);

            _context.AircraftVersions.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/AircraftVersions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftVersions.FindAsync(id);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST: Settings/AircraftVersions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftVersion entity)
        {
            if (id != entity.Id) return NotFound();

            if (await _context.AircraftVersions.AnyAsync(x => x.Id != entity.Id && x.Code == entity.Code))
            {
                ModelState.AddModelError(nameof(entity.Code), "Code already exists.");
            }

            if (!ModelState.IsValid) return View(entity);

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.AircraftVersions.AnyAsync(e => e.Id == entity.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/AircraftVersions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AircraftVersions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST: Settings/AircraftVersions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.AircraftVersions.FindAsync(id);
            if (entity != null)
            {
                _context.AircraftVersions.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}