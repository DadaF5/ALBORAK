using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all. Scoped via the parent
    // Sortie -> Odv, same pattern as SortiesController/SortieCrewsController.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class SquadronActivitiesController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly SquadronActivityService _svc;
        private readonly IUserScopeService _userScopeService;

        public SquadronActivitiesController(FRAContext context, SquadronActivityService svc, IUserScopeService userScopeService)
        {
            _context = context;
            _svc = svc;
            _userScopeService = userScopeService;
        }

        // GET: SquadronActivities/CompleteSortie/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> CompleteSortie(int id)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Aircraft)
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sortie == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            var vm = new CompleteSortieVm
            {
                SortieId = sortie.Id,
                OdvId = sortie.Id,
                TakeOffUtc = sortie.StartTime ?? DateTime.UtcNow,
                LandingUtc = sortie.LandingTime ?? DateTime.UtcNow,
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
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> CompleteSortie(CompleteSortieVm vm)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == vm.SortieId);

            if (sortie == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

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

        // ── Scope helpers ────────────────────────────────────────────────

        private async Task<bool> IsOdvInScopeAsync(int squadronId, int acMainGroupId, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            if (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(acMainGroupId))
                return false;

            var info = await _context.Squadrons
                .Where(s => s.Id == squadronId)
                .Select(s => new { s.WingId, WingBaseId = s.Wing!.BaseId })
                .FirstOrDefaultAsync();

            if (info == null) return false;
            if (info.WingBaseId == null || !scope.AllowedBaseIds.Contains(info.WingBaseId.Value)) return false;
            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(info.WingId)) return false;

            return true;
        }
    }
}
