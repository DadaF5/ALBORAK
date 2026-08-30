using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.Services;
using FRAProject.Areas.SquadronOps.ViewModels;
using FRAProject.Data;
using FRAProject.Enums;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all, and scoped crew-member
    // dropdowns via raw User.FindFirst("SquadronId")/("AcMainGroupId")
    // claims (a fourth, ad-hoc scoping mechanism, distinct from both the
    // legacy ApplicationUser fields and the real UserAssignment system).
    // SortieCrew has no Squadron/AcMainGroup of its own — it belongs to a
    // Sortie, which belongs to an Odv, which carries both — so scope is
    // resolved by walking up that chain, same pattern used throughout the
    // AircraftMaintenance conversion.
    //
    // CHANGED (Batch 16, 2026-08-30) — AircraftRole dropdown filtered via
    // the new AircraftRoleCatalog.
    //
    // FIX (Batch 17, 2026-08-30) — Batch 16 keyed the filter on the
    // sortie's Odv.AcMainGroup.Code ("F16-2B"). Real data showed that
    // group contains BOTH the single-seat F16C and two-seat F16D — keying
    // on the group meant a single-seat sortie was wrongly offered the
    // two-seat roles too. Re-keyed on the Sortie's own AcType.Code
    // instead (via the real Sortie.AcType navigation property) — every
    // call site now loads Sortie.AcType instead of Odv.AcMainGroup (same
    // one-more-Include shape, no new query). Still injects FRAContext
    // directly rather than IUnitOfWork, same as the file arrived — that
    // inconsistency is flagged, not fixed, since it's outside what was
    // asked.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class SortieCrewsController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly ILogger<SortieCrewsController> _logger;
        private readonly IUserScopeService _userScopeService;

        public SortieCrewsController(FRAContext context, ILogger<SortieCrewsController> logger, IUserScopeService userScopeService)
        {
            _context = context;
            _logger = logger;
            _userScopeService = userScopeService;
        }

        // GET: SortieCrews/Index?sortieId=123
        [HttpGet]
        public async Task<IActionResult> Index(int sortieId)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null)
            {
                return NotFound($"Sortie with ID {sortieId} not found.");
            }

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            // Get all crew assignments for this sortie
            var crewAssignments = await _context.SortieCrews
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Person)
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Squadron)
                .Where(sc => sc.SortieId == sortieId)
                .OrderBy(sc => sc.Seat)
                .ThenBy(sc => sc.IsPrimary ? 0 : 1)
                .ToListAsync();

            ViewBag.SortieId = sortieId;
            ViewBag.SortieCode = sortie.SortieCode;
            ViewBag.OdvId = sortie.OdvId;

            return View(crewAssignments);
        }

        // GET: SortieCrews/Create?sortieId=123
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(int sortieId)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .Include(s => s.AcType)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null)
            {
                return NotFound($"Sortie with ID {sortieId} not found.");
            }

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            // Crew members are drawn from the Sortie's own ODV squadron —
            // replaces the old raw-claim single-squadron filter.
            var odvSquadronId = sortie.Odv.SquadronId;

            // Get already assigned crew members to exclude them
            var assignedCrewIds = await _context.SortieCrews
                .Where(sc => sc.SortieId == sortieId)
                .Select(sc => sc.CrewMemberId)
                .ToListAsync();

            // Filter available crew members
            var availableCrewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active &&
                       !assignedCrewIds.Contains(cm.Id) &&
                       cm.SquadronId == odvSquadronId)
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}"
                })
                .ToListAsync();

            var vm = new SortieCrewCreateVm
            {
                SortieId = sortieId,
                SortieCode = sortie.SortieCode
            };

            ViewBag.CrewMembers = availableCrewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions(sortie.AcType?.Code);

            return View(vm);
        }

        // POST: SortieCrews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(SortieCrewCreateVm model)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .Include(s => s.AcType)
                .FirstOrDefaultAsync(s => s.Id == model.SortieId);
            if (sortie == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            // Defense in depth — the dropdown only offers crew from the
            // Odv's own squadron, but CrewMemberId is still a posted value.
            var crewMember = await _context.CrewMembers.FindAsync(model.CrewMemberId);
            if (crewMember == null || crewMember.SquadronId != sortie.Odv.SquadronId)
            {
                ModelState.AddModelError("CrewMemberId", "Selected crew member does not belong to this ODV's squadron.");
            }

            // NEW (Batch 16), FIXED (Batch 17) — defense in depth for the
            // role filter, same spirit as the CrewMemberId check above:
            // the dropdown only offers roles valid for this Sortie's own
            // AcType, but AircraftRole is still a posted value.
            var allowedRoles = AircraftRoleCatalog.GetAllowedRoles(sortie.AcType?.Code);
            if (!allowedRoles.Contains(model.AircraftRole))
            {
                ModelState.AddModelError(nameof(model.AircraftRole), "Selected role is not valid for this aircraft type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model.SortieId, sortie.Odv.SquadronId, sortie.AcType?.Code);
                return View(model);
            }

            // Check if crew member is already assigned to this sortie
            var existingAssignment = await _context.SortieCrews
                .FirstOrDefaultAsync(sc => sc.SortieId == model.SortieId && sc.CrewMemberId == model.CrewMemberId);

            if (existingAssignment != null)
            {
                ModelState.AddModelError("CrewMemberId", "This crew member is already assigned to this sortie.");
                await PopulateDropdowns(model.SortieId, sortie.Odv.SquadronId, sortie.AcType?.Code);
                return View(model);
            }

            var sortieCrew = new SortieCrew
            {
                SortieId = model.SortieId,
                CrewMemberId = model.CrewMemberId,
                Seat = model.Seat,
                AircraftRole = model.AircraftRole,
                Role = model.Role,
                IsPrimary = model.IsPrimary,
                Remarks = model.Remarks
            };

            _context.SortieCrews.Add(sortieCrew);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew member added successfully.";
            return RedirectToAction("Index", new { sortieId = model.SortieId });
        }

        // GET: SortieCrews/Edit/5
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var sortieCrew = await _context.SortieCrews
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Person)
                .Include(sc => sc.Sortie)
                    .ThenInclude(s => s.Odv)
                .Include(sc => sc.Sortie)
                    .ThenInclude(s => s.AcType)
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            var odv = sortieCrew.Sortie?.Odv;
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (odv == null || !await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            var vm = new SortieCrewCreateVm
            {
                Id = sortieCrew.Id,
                SortieId = sortieCrew.SortieId,
                CrewMemberId = sortieCrew.CrewMemberId,
                Seat = sortieCrew.Seat,
                AircraftRole = sortieCrew.AircraftRole,
                Role = sortieCrew.Role,
                IsPrimary = sortieCrew.IsPrimary,
                Remarks = sortieCrew.Remarks,
                CrewMemberName = sortieCrew.CrewMember?.Captain,
                SortieCode = sortieCrew.Sortie?.SortieCode
            };

            var crewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active && cm.SquadronId == odv.SquadronId)
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}",
                    Selected = cm.Id == sortieCrew.CrewMemberId
                })
                .ToListAsync();

            ViewBag.CrewMembers = crewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions(sortieCrew.Sortie?.AcType?.Code);

            return View(vm);
        }

        // POST: SortieCrews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, SortieCrewCreateVm model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var sortieCrew = await _context.SortieCrews
                .Include(sc => sc.Sortie)
                    .ThenInclude(s => s.Odv)
                .Include(sc => sc.Sortie)
                    .ThenInclude(s => s.AcType)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            var odv = sortieCrew.Sortie?.Odv;
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (odv == null || !await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            // NEW (Batch 16), FIXED (Batch 17) — same defense-in-depth
            // role check as Create, now keyed on the Sortie's own AcType.
            var acTypeCode = sortieCrew.Sortie?.AcType?.Code;
            var allowedRoles = AircraftRoleCatalog.GetAllowedRoles(acTypeCode);
            if (!allowedRoles.Contains(model.AircraftRole))
            {
                ModelState.AddModelError(nameof(model.AircraftRole), "Selected role is not valid for this aircraft type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model.SortieId, odv.SquadronId, acTypeCode, model.CrewMemberId);
                return View(model);
            }

            // Check for duplicate assignment (if crew member changed)
            if (sortieCrew.CrewMemberId != model.CrewMemberId)
            {
                var existingAssignment = await _context.SortieCrews
                    .FirstOrDefaultAsync(sc => sc.SortieId == model.SortieId &&
                                               sc.CrewMemberId == model.CrewMemberId &&
                                               sc.Id != id);

                if (existingAssignment != null)
                {
                    ModelState.AddModelError("CrewMemberId", "This crew member is already assigned to this sortie.");
                    await PopulateDropdowns(model.SortieId, odv.SquadronId, acTypeCode, model.CrewMemberId);
                    return View(model);
                }

                var crewMember = await _context.CrewMembers.FindAsync(model.CrewMemberId);
                if (crewMember == null || crewMember.SquadronId != odv.SquadronId)
                {
                    ModelState.AddModelError("CrewMemberId", "Selected crew member does not belong to this ODV's squadron.");
                    await PopulateDropdowns(model.SortieId, odv.SquadronId, acTypeCode, model.CrewMemberId);
                    return View(model);
                }
            }

            sortieCrew.CrewMemberId = model.CrewMemberId;
            sortieCrew.Seat = model.Seat;
            sortieCrew.AircraftRole = model.AircraftRole;
            sortieCrew.Role = model.Role;
            sortieCrew.IsPrimary = model.IsPrimary;
            sortieCrew.Remarks = model.Remarks;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew assignment updated successfully.";
            return RedirectToAction("Index", new { sortieId = model.SortieId });
        }

        // POST: SortieCrews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var sortieCrew = await _context.SortieCrews
                .Include(sc => sc.Sortie)
                    .ThenInclude(s => s.Odv)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            var odv = sortieCrew.Sortie?.Odv;
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (odv == null || !await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            var sortieId = sortieCrew.SortieId;

            _context.SortieCrews.Remove(sortieCrew);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew assignment removed successfully.";
            return RedirectToAction("Index", new { sortieId });
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private List<SelectListItem> GetSeatOptions()
        {
            return Enum.GetValues(typeof(CrewSeat))
                .Cast<CrewSeat>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                })
                .ToList();
        }

        // CHANGED (Batch 16, 2026-08-30) — filters via
        // AircraftRoleCatalog.GetAllowedRoles() instead of unconditionally
        // listing every AircraftRole value.
        // FIXED (Batch 17, 2026-08-30) — parameter is now the Sortie's own
        // AcType.Code, not its Odv's AcMainGroup.Code (see class header
        // comment and AircraftRoleCatalog.cs for why). Pass null (or an
        // unconfigured Code) to keep the "every role" fallback behaviour.
        private List<SelectListItem> GetAircraftRoleOptions(string? acTypeCode)
        {
            return AircraftRoleCatalog.GetAllowedRoles(acTypeCode)
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();
        }

        private async Task PopulateDropdowns(int sortieId, int odvSquadronId, string? acTypeCode, int? currentCrewMemberId = null)
        {
            var crewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active && cm.SquadronId == odvSquadronId)
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}",
                    Selected = cm.Id == currentCrewMemberId
                })
                .ToListAsync();

            ViewBag.CrewMembers = crewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions(acTypeCode);
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
