using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Mapping;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all. Also used a dead role-based
    // bypass (UserCanSeeAllAircraftTypes checked "Administrator",
    // "SuperAdmin", "MaintenanceSupervisor", "FlightOpsManager" — none of
    // which are real seeded AspNetRoles) plus the legacy
    // ApplicationUser.AcMainGroupId field directly. Sortie has no Squadron/
    // AcMainGroup of its own — it belongs to an Odv, which carries both —
    // so scope is resolved via the parent Odv, same pattern as
    // WorkOrderSection resolving via its parent WorkOrder in the
    // AircraftMaintenance conversion.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class SortiesController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;

        public SortiesController(FRAContext context,
                                 UserManager<ApplicationUser> userManager,
                                 IUserScopeService userScopeService)
        {
            _context = context;
            _userManager = userManager;
            _userScopeService = userScopeService;
        }

        // GET: Sorties/Create?odvId=123
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(int odvId)
        {
            var odv = await _context.Odvs
                .AsNoTracking()
                .Include(o => o.AcMainGroup)
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            var acMainGroupId = odv.AcMainGroupId;

            if (acMainGroupId <= 0)
            {
                TempData["Warning"] = "No valid AcMainGroupId is associated with this Odv.";
                ViewBag.AcTypes = new List<SelectListItem>();
                return View(new SortieCreateVm { OdvId = odvId });
            }

            // populate aircraft types for the given AcMainGroupId
            var acTypes = await _context.AcTypes
                .Where(t => t.AcMainGroupId == acMainGroupId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            if (!acTypes.Any())
            {
                TempData["Warning"] = $"No aircraft types found for AcMainGroupId {acMainGroupId}.";
                ViewBag.AcTypes = new List<SelectListItem>();
            }
            else
            {
                ViewBag.AcTypes = acTypes.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();
            }

            var vm = new SortieCreateVm
            {
                OdvId = odvId,
                Sequence = 1 // default, can be changed later
            };

            ViewBag.OdvInfo = $"{odv.MissionId} | {odv.OdvDate:yyyy-MM-dd}";
            return View(vm);
        }

        // POST: Sorties/Create?odvId=123
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(SortieCreateVm model, int? acMainGroupId)
        {
            var odv = await _context.Odvs.AsNoTracking().FirstOrDefaultAsync(o => o.Id == model.OdvId);
            if (odv == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            // Server-side guard — the dropdown only offers AcTypes within the
            // Odv's own AcMainGroup, but AcTypeId is still a posted value.
            var chosenAcType = await _context.AcTypes.FirstOrDefaultAsync(t => t.Id == model.AcTypeId);
            if (chosenAcType == null || chosenAcType.AcMainGroupId != odv.AcMainGroupId)
            {
                ModelState.AddModelError(nameof(model.AcTypeId), "Selected aircraft type does not match this ODV's aircraft group.");
            }

            if (!ModelState.IsValid)
            {
                if (acMainGroupId.HasValue)
                {
                    await PopulateAcTypesByMainGroup(acMainGroupId.Value);
                }

                return View(model);
            }

            var sortie = new Sortie
            {
                OdvId = model.OdvId,
                SortieCode = model.SortieCode,
                Configuration = model.Configuration,
                Sequence = model.Sequence,
                AcTypeId = model.AcTypeId,
                Status = SortieStatus.Planned,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Sorties.Add(sortie);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "OdvPlanning",
                new { odvDate = DateTime.UtcNow.ToString("yyyy-MM-dd") }
            );
        }

        // GET: Sorties/Edit/5
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            var vm = new SortieCreateVm
            {
                Id = sortie.Id,
                OdvId = sortie.OdvId,
                SortieCode = sortie.SortieCode,
                Configuration = sortie.Configuration,
                Sequence = sortie.Sequence,
                AcTypeId = sortie.AcTypeId, // This is the current type
                FuelQuantity = sortie.FuelQuantity
            };

            // Pass the current AcTypeId to ensure it appears in dropdown
            await PopulateAcTypesForOdv(sortie.Odv.AcMainGroupId, sortie.AcTypeId, scope);

            return View(vm);
        }

        // POST: Sorties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, SortieCreateVm model)
        {
            if (id != model.Id)
                return BadRequest();

            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateAcTypesForOdv(sortie.Odv.AcMainGroupId, model.AcTypeId, scope);
                return View(model);
            }

            // The new aircraft type must stay within the Odv's own
            // AcMainGroup — replaces the old per-user AcMainGroupId check,
            // which was really guarding the wrong boundary (Odv's group,
            // not the editing user's group).
            if (model.AcTypeId != sortie.AcTypeId)
            {
                var newAcType = await _context.AcTypes.FirstOrDefaultAsync(t => t.Id == model.AcTypeId);
                if (newAcType == null || newAcType.AcMainGroupId != sortie.Odv.AcMainGroupId)
                {
                    ModelState.AddModelError("AcTypeId", "You cannot select an aircraft type outside this ODV's assigned group.");
                    await PopulateAcTypesForOdv(sortie.Odv.AcMainGroupId, model.AcTypeId, scope);
                    return View(model);
                }
            }

            // Update sortie properties
            sortie.SortieCode = model.SortieCode;
            sortie.Configuration = model.Configuration;
            sortie.Sequence = model.Sequence;
            sortie.AcTypeId = model.AcTypeId;
            sortie.FuelQuantity = model.FuelQuantity;
            sortie.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "OdvPlanning");
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

        // Populates AcTypes strictly within the Odv's own AcMainGroup —
        // scope only decides WHETHER the user can edit this Sortie at all
        // (already checked by IsOdvInScopeAsync above), not which AcTypes
        // show once they're in.
        private async Task PopulateAcTypesForOdv(int odvAcMainGroupId, int? currentAcTypeId, UserScope scope)
        {
            var acTypes = await _context.AcTypes
                .Where(t => t.AcMainGroupId == odvAcMainGroupId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.AcTypes = acTypes.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = t.Id == currentAcTypeId
            }).ToList();

            if (!acTypes.Any())
            {
                TempData["Warning"] = "No aircraft types available for this ODV's aircraft group.";
            }
        }

        private async Task PopulateAcTypesByMainGroup(int acMainGroupId)
        {
            var acTypes = await _context.AcTypes
                .Where(t => t.AcMainGroupId == acMainGroupId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            var selectList = acTypes.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            if (!selectList.Any())
            {
                TempData["Warning"] = "No aircraft types available for the selected Aircraft Maintenance Group.";
            }

            ViewBag.AcTypes = selectList;
        }
    }
}
