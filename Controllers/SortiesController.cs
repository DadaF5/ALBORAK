using FRAProject.Data;
using FRAProject.Mapping;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class SortiesController : Controller
    {
        private readonly FRAContext _context;

        public SortiesController(FRAContext context)
        {
            _context = context;
        }

        // GET: Sorties/Create?odvId=123
        public async Task<IActionResult> Create(int odvId)
        {
            var odv = await _context.Odvs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null) return NotFound();

            var vm = new SortieVm();
            ViewBag.OdvId = odvId;
            ViewBag.OdvSummary = $"{odv.Id} - {odv.OdvDate:yyyy-MM-dd}";
            return View(vm);
        }

        // POST: Sorties/Create?odvId=123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int odvId, SortieVm vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OdvId = odvId;
                return View(vm);
            }

            var odv = await _context.Odvs
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null) return NotFound();

            // Pass the current user name (or null) to the mapper so audit "CreatedBy" is set server-side.
            var createdBy = User?.Identity?.Name;

            var sortie = SortieMapper.MapForCreate(vm, odvId, odv.BaseId, createdBy);

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Sorties.Add(sortie);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction("Details", "Odv", new { id = odvId });
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "An error occurred adding the sortie.");
                ViewBag.OdvId = odvId;
                return View(vm);
            }
        }

        // You can add Index/Details/Edit/Delete actions here as needed.
    }
}