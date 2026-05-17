using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class AcStatusTypeController : Controller
    {
        private readonly FRAContext _context;

        public AcStatusTypeController(FRAContext context)
        {
            _context = context;
        }

        // GET: AcStatusTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.AcStatusTypes.ToListAsync());
        }

        // GET: AcStatusTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var acStatusType = await _context.AcStatusTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (acStatusType == null) return NotFound();

            return View(acStatusType);
        }

        // GET: AcStatusTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AcStatusTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("StatusName,Description")] AcStatusType acStatusType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(acStatusType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(acStatusType);
        }

        // GET: AcStatusTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var acStatusType = await _context.AcStatusTypes.FindAsync(id);
            if (acStatusType == null) return NotFound();

            return View(acStatusType);
        }

        // POST: AcStatusTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcStatusType acStatusType)
        {
            if (id != acStatusType.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(acStatusType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.AcStatusTypes.Any(e => e.Id == acStatusType.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(acStatusType);
        }

        // GET: AcStatusTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var acStatusType = await _context.AcStatusTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (acStatusType == null) return NotFound();

            return View(acStatusType);
        }

        // POST: AcStatusTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acStatusType = await _context.AcStatusTypes.FindAsync(id);
            if (acStatusType != null)
            {
                _context.AcStatusTypes.Remove(acStatusType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

