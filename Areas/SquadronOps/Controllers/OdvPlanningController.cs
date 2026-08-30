using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // REDESIGNED 2026-08-29 — "redesign from zero" pass. Was: FRAContext
    // injected directly and queried in every action (Batch 1/2 stopgap).
    // Now: talks only to IUnitOfWork, same rule as the rest of the app.
    // All eager-loading that the plain generic repos can't do lives in the
    // new specialist repositories (IOdvRepository, ISquadronRepository) —
    // see Areas/SquadronOps/Repositories/ for the why behind each one.
    //
    // Also carries forward, unchanged in behaviour, everything fixed in
    // Batch 1/2: real SquadronOpsRead/Write policy checks (not the dead
    // "SquadronOps" AspNetRole), real UserScope-based filtering instead of
    // the legacy single-squadron/group fields, the Wing-vs-Department
    // scope-base fix, and the Odv-cancels-all-Sorties cascade with its
    // Finalized-sortie-left-alone judgment call.
    [Route("Odvplanning")]
    [Authorize(Policy = "SquadronOpsRead")]
    [Area("SquadronOps")]
    public class OdvPlanningController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OdvPlanningController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;

        public OdvPlanningController(
            IUnitOfWork unitOfWork,
            ILogger<OdvPlanningController> logger,
            UserManager<ApplicationUser> userManager,
            IUserScopeService userScopeService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _userScopeService = userScopeService;
        }

        // =============================
        // GET: /Odvs
        // =============================
        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? odvDate)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Persist board date
            DateTime selectedDate;

            if (odvDate.HasValue)
            {
                selectedDate = odvDate.Value.Date;
                TempData["OdvDate"] = selectedDate.ToString("yyyy-MM-dd");
            }
            else if (TempData["OdvDate"] != null)
            {
                selectedDate = DateTime.Parse(TempData["OdvDate"]!.ToString()!);
                TempData.Keep("OdvDate");
            }
            else
            {
                selectedDate = DateTime.Today;
                TempData["OdvDate"] = selectedDate.ToString("yyyy-MM-dd");
            }

            var vm = new OdvIndexVm
            {
                SelectedDate = selectedDate,
                CreateModel = new OdvCreateVm
                {
                    OdvDate = selectedDate
                }
            };

            // Default the Create form's Squadron/AcMainGroup from the
            // user's legacy "home" fields, for convenience only — Create
            // itself re-validates against real scope regardless of what's
            // defaulted here.
            if (!scope.IsUnrestricted)
            {
                var user = await GetCurrentUserAsync();

                if (user.SquadronId.HasValue && user.AcMainGroupId.HasValue &&
                    await IsSquadronInScopeAsync(user.SquadronId.Value, scope) &&
                    scope.AllowedAcMainGroupIds.Contains(user.AcMainGroupId.Value))
                {
                    vm.CreateModel.SquadronId = user.SquadronId.Value;
                    vm.CreateModel.AcMainGroupId = user.AcMainGroupId.Value;

                    var squadron = await _unitOfWork.Squadrons.GetByIdAsync(user.SquadronId.Value);
                    ViewBag.SquadronName = squadron?.Name;

                    var acMainGroup = await _unitOfWork.AcMainGroups.GetByIdAsync(user.AcMainGroupId.Value);
                    ViewBag.AcMainGroupName = acMainGroup?.Name;
                }
                // else: legacy field missing/stale/out of scope — leave
                // CreateModel unset, PopulateSelectListsAsync still offers
                // the user's real in-scope squadrons/groups to choose from.
            }

            // Populate dropdowns (scoped)
            await PopulateSelectListsAsync(vm, scope);

            // Load ODVs for that date + scope
            HashSet<int>? allowedSquadronIds = null;
            HashSet<int>? allowedAcMainGroupIds = null;

            if (!scope.IsUnrestricted)
            {
                allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                // Empty set = "no AcMainGroup restriction", same semantics
                // as UserScope.AllowedAcMainGroupIds.Any() == false below.
                allowedAcMainGroupIds = scope.AllowedAcMainGroupIds.Any()
                    ? scope.AllowedAcMainGroupIds.ToHashSet()
                    : new HashSet<int>();
            }

            vm.Odvs = await _unitOfWork.Odvs.GetBoardForDateAsync(selectedDate, allowedSquadronIds, allowedAcMainGroupIds);

            // Load AcTypes for Sortie creation
            vm.AcTypes = (await _unitOfWork.AcTypes.GetAllAsync())
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();

            return View(vm);
        }

        // =============================
        // POST: /Odvs/Create (P1)
        // =============================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create([FromForm, Bind(Prefix = "CreateModel")] OdvCreateVm model)
        {
            _logger.LogDebug("ODV Create POST: {@Model}", model);

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // NOTE: relies on IAcMainGroupRepository extending
            // IGenericRepository<AcMainGroup> (so AnyAsync/GetByIdAsync
            // are inherited). Confirmed for IWorkOrderRepository by
            // reading the real file; NOT independently confirmed for
            // IAcMainGroupRepository itself (I haven't seen that
            // interface). It's already wired into the real IUnitOfWork as
            // "IAcMainGroupRepository AcMainGroups { get; }" and the
            // codebase's own convention (every specialist repo seen so far
            // except the narrow Maintenance ISortieRepository) makes this
            // a safe bet — but flagging it as the one remaining assumption
            // in this batch. If AcMainGroups turns out not to extend the
            // generic interface, this is a one-line fix.
            bool acGroupExists = await _unitOfWork.AcMainGroups.AnyAsync(g => g.Id == model.AcMainGroupId);

            if (!acGroupExists)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.AcMainGroupId", new[] { "Aircraft Main Group does not exist." } }
                });
            }

            if (model.AcMainGroupId <= 0)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.AcMainGroupId", new[] { "Aircraft Main Group is missing or invalid." } }
                });
            }

            // Real scope enforcement — replaces the old "force to the user's
            // single legacy squadron/group" behaviour. A scoped user can now
            // create an ODV for ANY squadron/AcMainGroup within their real
            // UserAssignment scope, not just their one legacy "home" value.
            // The dropdowns only offer in-scope options, but this is still a
            // posted value and must be re-validated server-side regardless.
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(model.SquadronId, scope))
                {
                    return BadRequest(new Dictionary<string, string[]>
                    {
                        { "CreateModel.SquadronId", new[] { "This squadron is outside your assigned scope." } }
                    });
                }

                if (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(model.AcMainGroupId))
                {
                    return BadRequest(new Dictionary<string, string[]>
                    {
                        { "CreateModel.AcMainGroupId", new[] { "This aircraft group is outside your assigned scope." } }
                    });
                }
            }

            // Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ExtractModelStateErrors("CreateModel"));
            }

            // Mission ownership check
            var missionAllowed = await IsMissionAllowedAsync(
                model.MissionId!,
                model.SquadronId,
                scope);

            if (!missionAllowed)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.MissionId", new[] { "Mission not allowed." } }
                });
            }

            // Default BaseId from the squadron's current operating base
            // (Squadron.CurrentBaseId, e.g. Squadron 312 currently at 2nd
            // AFB while its Wing's home is 6th AFB). Falls back to null
            // (unset) if the squadron has no CurrentBaseId configured yet.
            var squadron = await _unitOfWork.Squadrons.GetByIdAsync(model.SquadronId);
            var squadronBaseId = squadron?.CurrentBaseId;

            // Create ODV
            var odv = new Odv
            {
                SquadronId = model.SquadronId,
                BaseId = squadronBaseId,
                AcMainGroupId = model.AcMainGroupId,
                MissionId = model.MissionId,
                OdvDate = model.OdvDate!.Date,
                Zone = model.Zone,
                MissionType = model.MissionType,
                Area = model.Area,
                TOFF = model.TOFF ?? TimeSpan.Zero,
                Obs = model.Obs,
                CallSignId = model.CallSignId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _unitOfWork.Odvs.Add(odv);
            await _unitOfWork.CompleteAsync();

            // Redirect back to SAME DATE
            return RedirectToAction(nameof(Index), new
            {
                odvDate = model.OdvDate!.ToString("yyyy-MM-dd")
            });
        }

        // =============================
        // GET: /Odvs/Edit/{id}
        // =============================
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, DateTime? odvDate)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var odv = await _unitOfWork.Odvs.GetByIdAsync(id);

            if (odv == null)
                return NotFound();

            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(odv.SquadronId, scope) ||
                    (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(odv.AcMainGroupId)))
                    return Forbid();
            }

            var vm = new OdvEditVm
            {
                Id = odv.Id,
                MissionId = odv.MissionId,
                CallSignId = odv.CallSignId,
                TOFF = odv.TOFF ?? TimeSpan.Zero,
                Area = odv.Area,
                Obs = odv.Obs
            };

            // reuse dropdown population
            ViewBag.Missions = (await _unitOfWork.Missions.GetAllAsync())
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToList();

            ViewBag.CallSigns = (await _unitOfWork.CallSigns.GetAllAsync())
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                .ToList();

            ViewBag.ReturnDate = odvDate;

            return View(vm);
        }

        // Edit POST
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, OdvEditVm model, DateTime? odvDate)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var odv = await _unitOfWork.Odvs.GetByIdAsync(id);
            if (odv == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(odv.SquadronId, scope) ||
                    (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(odv.AcMainGroupId)))
                    return Forbid();
            }

            odv.MissionId = model.MissionId;
            odv.CallSignId = model.CallSignId;
            odv.TOFF = model.TOFF;
            odv.Area = model.Area;
            odv.Obs = model.Obs;
            odv.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", new { odvDate });
        }

        // =============================
        // POST: /Odvplanning/Cancel/{id}  (2026-08-29)
        // =============================
        // Cancels a whole ODV: sets OdvStatus.Cancelled (already existed)
        // + the dedicated Odv.CancellationReason field, and CASCADES the
        // same reason to every related Sortie's own CancellationReason
        // (Sortie.SortieStatus.Canceled), per Dadda's direct instruction.
        // Scoped the same way as Edit. Unchanged behaviour from Batch 2 —
        // only the data access underneath moved to IUnitOfWork.
        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Cancel(int id, string? reason, DateTime? odvDate)
        {
            var odv = await _unitOfWork.Odvs.GetByIdWithSortiesAsync(id);
            if (odv == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(odv.SquadronId, scope) ||
                    (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(odv.AcMainGroupId)))
                    return Forbid();
            }

            if (odv.OdvStatus == Enums.OdvStatus.Cancelled)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This ODV is already cancelled." } }
                });
            }

            var utcNow = DateTime.UtcNow;

            odv.OdvStatus = Enums.OdvStatus.Cancelled;
            odv.CancellationReason = reason;
            odv.CancelledAtUtc = utcNow;
            odv.UpdatedAtUtc = utcNow;

            // Cascade — cancelling the Odv cancels every related Sortie
            // with the SAME reason. Deliberately skips Sorties already
            // Finalized: a Finalized sortie has already had its
            // FH/ENGINE_HOURS/TGO_LANDINGS effects applied to
            // Aircraft/AircraftReadings — silently cancelling it now would
            // not reverse those, so it's left as Finalized and flagged via
            // TempData instead of quietly mis-stating its status. Also
            // skips Sorties already individually Canceled (no-op, not an
            // error).
            var skippedFinalized = 0;
            if (odv.Sorties != null)
            {
                foreach (var sortie in odv.Sorties)
                {
                    if (sortie.Status == SortieStatus.Finalized)
                    {
                        skippedFinalized++;
                        continue;
                    }

                    if (sortie.Status == SortieStatus.Canceled)
                        continue;

                    sortie.Status = SortieStatus.Canceled;
                    sortie.CancellationReason = reason;
                    sortie.CancelledAtUtc = utcNow;
                    sortie.UpdatedAtUtc = utcNow;
                }
            }

            if (skippedFinalized > 0)
            {
                TempData["Warning"] =
                    $"ODV cancelled. {skippedFinalized} already-finalized sortie(s) were left as Finalized — " +
                    "cancelling them would not reverse their recorded flight/engine hours.";
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index), new { odvDate });
        }

        // =============================
        // Select-list population
        // =============================
        private async Task PopulateSelectListsAsync(OdvIndexVm vm, UserScope scope)
        {
            IEnumerable<Squadron> squadrons;
            IEnumerable<AcMainGroup> acMainGroups;
            IEnumerable<CallSign> callSigns;
            IEnumerable<Aircraft> aircrafts;
            IEnumerable<CrewMember> crewMembers;

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);

                squadrons = await _unitOfWork.Squadrons.GetWhereAsync(s => allowedSquadronIds.Contains(s.Id));
                crewMembers = await _unitOfWork.CrewMembers.GetWhereAsync(cm => allowedSquadronIds.Contains(cm.SquadronId));

                if (scope.AllowedAcMainGroupIds.Any())
                {
                    acMainGroups = await _unitOfWork.AcMainGroups.GetWhereAsync(g => scope.AllowedAcMainGroupIds.Contains(g.Id));
                    aircrafts = await _unitOfWork.Aircraft.GetWhereAsync(a => a.AcType != null && scope.AllowedAcMainGroupIds.Contains(a.AcType.AcMainGroupId));
                }
                else
                {
                    acMainGroups = await _unitOfWork.AcMainGroups.GetAllAsync();
                    aircrafts = await _unitOfWork.Aircraft.GetAllAsync();
                }

                callSigns = await _unitOfWork.CallSigns.GetWhereAsync(c => c.BaseId == null || scope.AllowedBaseIds.Contains(c.BaseId.Value));
            }
            else
            {
                squadrons = await _unitOfWork.Squadrons.GetAllAsync();
                acMainGroups = await _unitOfWork.AcMainGroups.GetAllAsync();
                callSigns = await _unitOfWork.CallSigns.GetAllAsync();
                aircrafts = await _unitOfWork.Aircraft.GetAllAsync();
                crewMembers = await _unitOfWork.CrewMembers.GetAllAsync();
            }

            vm.Squadrons = squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();

            vm.AcMainGroups = acMainGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToList();

            vm.Missions = await GetMissionSelectListAsync(scope);

            vm.CallSigns = callSigns
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                .ToList();

            vm.Aircrafts = aircrafts
                .OrderBy(a => a.Registration)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Registration })
                .ToList();

            vm.CrewMembers = crewMembers
                .OrderBy(cm => cm.NickName)
                .Select(cm => new SelectListItem { Value = cm.Id.ToString(), Text = cm.Captain })
                .ToList();

            vm.ZoneList = Enum.GetValues(typeof(Enums.Zone))
                .Cast<Enums.Zone>()
                .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                .ToList();

            vm.MissionTypeList = Enum.GetValues(typeof(Enums.MissionType))
                .Cast<Enums.MissionType>()
                .Select(m => new SelectListItem { Value = ((int)m).ToString(), Text = m.ToString() })
                .ToList();
        }

        // =============================
        // Mission scoping helpers
        // =============================
        private async Task<List<SelectListItem>> GetMissionSelectListAsync(UserScope scope)
        {
            var missions = (await _unitOfWork.Missions.GetWhereAsync(m => m.IsActive)).AsEnumerable();

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                missions = missions.Where(m => m.SquadronId == null || allowedSquadronIds.Contains(m.SquadronId.Value));
            }

            return missions
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                })
                .ToList();
        }

        private async Task<bool> IsMissionAllowedAsync(int missionId, int squadronId, UserScope scope)
        {
            var mission = await _unitOfWork.Missions.GetFirstOrDefaultAsync(m => m.Id == missionId && m.IsActive);
            if (mission == null) return false;
            if (scope.IsUnrestricted) return true;
            if (mission.SquadronId == null) return true; // global mission

            // A global-scope match on the mission's OWN squadron isn't
            // enough on its own — the ODV's target squadron must also
            // match the mission's squadron, same intent as the original
            // single-squadron check, just expressed against the posted
            // squadronId instead of a forced legacy value.
            return mission.SquadronId == squadronId && await IsSquadronInScopeAsync(squadronId, scope);
        }

        // =============================
        // Scope helpers
        // =============================
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                throw new InvalidOperationException("Authenticated user not found.");
            return user;
        }

        private async Task<bool> IsSquadronInScopeAsync(int squadronId, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            var info = await _unitOfWork.Squadrons.GetScopeInfoAsync(squadronId);
            if (info == null) return false;

            if (!scope.AllowedBaseIds.Contains(info.Value.BaseId)) return false;
            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(info.Value.WingId)) return false;

            return true;
        }

        private async Task<HashSet<int>> GetInScopeSquadronIdsAsync(UserScope scope)
        {
            return await _unitOfWork.Squadrons.GetInScopeIdsAsync(scope.AllowedBaseIds, scope.AllowedWingIds);
        }

        // =============================
        // ModelState helper (AJAX)
        // =============================
        private Dictionary<string, string[]> ExtractModelStateErrors(string prefix)
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var kv in ModelState)
            {
                if (kv.Value == null || kv.Value.Errors.Count == 0) continue;

                var key = kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? kv.Key
                    : $"{prefix}.{kv.Key}";

                errors[key] = kv.Value.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                        ? e.Exception?.Message ?? "Invalid value"
                        : e.ErrorMessage)
                    .ToArray();
            }

            return errors;
        }
    }
}
