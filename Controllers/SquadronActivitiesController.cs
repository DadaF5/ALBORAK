using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class SquadronActivitiesController : Controller
    {
        private readonly FRAContext _context;
        private readonly SquadronActivityService _svc;

        public SquadronActivitiesController(FRAContext context, SquadronActivityService svc)
        {
            _context = context;
            _svc = svc;
        }

        // GET: SquadronActivities/CompleteSortie/5
        public async Task<IActionResult> CompleteSortie(int id)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Aircraft)
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SortieId == id);

            if (sortie == null) return NotFound();

            var vm = new CompleteSortieVm
            {
                SortieId = sortie.SortieId,
                OdvId = sortie.OdvID,
                TakeOffUtc = sortie.StartTime ?? System.DateTime.UtcNow,
                LandingUtc = sortie.LandingTime ?? System.DateTime.UtcNow,
                HobbsStart = null,
                HobbsEnd = null,
                TachStart = null,
                TachEnd = null,
                FuelUsedKg = sortie.FuelQuantity,
                Notes = sortie.Notes
            };

            return View(vm);
        }

        // POST: SquadronActivities/CompleteSortie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSortie(CompleteSortieVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _svc.CompleteSortieAsync(vm.SortieId, vm.TakeOffUtc, vm.LandingUtc, vm.HobbsStart, vm.HobbsEnd, vm.TachStart, vm.TachEnd, vm.FuelUsedKg, vm.CompletedBy);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unknown error");
                return View(vm);
            }

            return RedirectToAction("Details", "Odvs", new { id = vm.OdvId });
        }
    }
}
