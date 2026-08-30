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
    //
    // BATCH 14 (2026-08-30) — Dadda shared the real legacy WebForms board
    // (CIPL_FlyingProgram.aspx/.vb) as the layout users are used to, and
    // asked for an Escadron filter matching it, extended to unrestricted/
    // Admin users only (a scoped user's own squadron scope already limits
    // what they see — see Index below). ONE change in this file: Index
    // gained an optional squadronId parameter. Nothing else here changed.
    //
    // BATCH 15 (2026-08-30) — SortieCrewsController.cs arrived, unblocking
    // the legacy "Add Sortie to ODV" one-step card. Two additive fields
    // populated here (AcTypesFull/CrewMembersFull on OdvIndexVm) feed that
    // new card's client-side cascading dropdowns; no existing field or
    // action here changed shape. See OdvPlanning/Index.cshtml's own Batch
    // 15 comment for the full design and the SortiesController.Create
    // change that makes the combined card's AJAX orchestration possible.
    //
    // BATCH 16 (2026-08-30) — real per-AcMainGroup crew-role structures
    // confirmed (F16 vs C130), surfacing that a single global AircraftRole
    // list doesn't fit every squadron. One more additive field here
    // (AcMainGroupsFull) feeds the combined card's redesigned per-row Role
    // dropdown — see AircraftRoleCatalog.cs and OdvPlanning/Index.cshtml's
    // Batch 16 comment. Also fixes a real Batch 15 bug: AcTypesFull was
    // typed against the wrong AcType (this file already had the correct
    // using — FRAProject.Areas.Settings.Models — so it was unaffected, but
    // OdvIndexVm.cs and OdvPlanning/Index.cshtml were not; both fixed).
    //
    // BATCH 17 (2026-08-30) — FIX: Batch 16's AcMainGroupsFull/
    // AircraftRoleCatalog keying was wrong — a real query showed the
    // F16-2B AcMainGroup contains BOTH the single-seat F16C and the
    // two-seat F16D, so keying role filtering on the group gave the
    // single-seat jet the two-seat jet's extra crew roles. Re-keyed on
    // AcType.Code instead (see AircraftRoleCatalog.cs). AcMainGroupsFull
    // is removed here — AcTypesFull (already populated below since Batch
    // 15) already carries each AcType's Code, so nothing new is needed.
    //
    // BATCH 19 (2026-08-30) — Dadda compared the real legacy
    // CIPL_FlyingProgram.aspx/.vb against this rebuild and asked for two
    // real behavioural changes (confirmed against the real .aspx.vb, not
    // guessed): (1) legacy has exactly ONE Escadron selector total —
    // btnCreateODV_Click reads squadron straight off the same ddlSqnID
    // used for filtering — so Create ODV's own Escadron dropdown is
    // removed; an unrestricted user's new-ODV squadron now always follows
    // whichever one the Filtres bar is narrowed to (see Index below and
    // OdvIndexVm.CanCreateOdv). (2) added the Wing filter legacy has
    // ("Wing: C.I.P.L" in the screenshot) — Squadron.WingId is real,
    // confirmed non-nullable, so every squadron belongs to exactly one
    // Wing; narrows the Escadron dropdown/board the same "narrow, never
    // widen" way squadronId already did.
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
        // BATCH 14: added `int? squadronId` — an OPTIONAL manual narrowing
        // filter on top of real UserScope, matching the legacy Escadron
        // dropdown. Only meaningful for an unrestricted (Admin) user, who
        // otherwise sees every squadron's ODVs with no way to narrow the
        // list to one, same as the legacy page's Escadron dropdown did.
        // A scoped (non-Admin) user's own UserScope already limits results
        // to their assigned squadron(s) — passing squadronId for a scoped
        // user is intersected with their real scope below, never used to
        // widen it beyond what they're actually allowed to see.
        [HttpGet("")]
        // BATCH 19 (2026-08-30) — one more optional param, wingId, per
        // Dadda's choice to add the legacy page's Wing filter (real
        // .aspx.vb screenshot showed "Wing: C.I.P.L" alongside Escadron/
        // Flying Date). Squadron.WingId is a real, required (non-nullable)
        // field — confirmed from Squadron.cs — so every squadron belongs
        // to exactly one Wing; this narrows which squadrons the Escadron
        // dropdown/board consider, same "narrow, never widen" rule as the
        // existing squadronId param.
        public async Task<IActionResult> Index(DateTime? odvDate, int? squadronId, int? wingId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Wing → its Squadron ids, used below both to narrow the board
            // query and to keep squadronId from a stale Wing/Escadron
            // combination (e.g. Wing changed but the old Escadron value is
            // still in the URL) from being applied.
            HashSet<int>? wingSquadronIds = null;
            if (wingId.HasValue)
            {
                wingSquadronIds = (await _unitOfWork.Squadrons.GetWhereAsync(s => s.WingId == wingId.Value))
                    .Select(s => s.Id)
                    .ToHashSet();
            }

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

            // BATCH 19: squadronId is only honored if it actually belongs
            // to the selected Wing (when one is selected) — otherwise a
            // stale combination would silently narrow the board to a
            // squadron the user can no longer see reflected in the
            // Escadron dropdown.
            var effectiveSquadronId = (wingId.HasValue && squadronId.HasValue && !wingSquadronIds!.Contains(squadronId.Value))
                ? null
                : squadronId;

            var vm = new OdvIndexVm
            {
                SelectedDate = selectedDate,
                SelectedSquadronId = effectiveSquadronId,
                SelectedWingId = wingId,
                IsUnrestrictedScope = scope.IsUnrestricted,
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
            else if (effectiveSquadronId.HasValue)
            {
                // NEW (Batch 19, 2026-08-30) — matches the real legacy
                // btnCreateODV_Click, which reads its squadron straight off
                // the same ddlSqnID used for filtering: an unrestricted
                // user's Create-ODV squadron is now whichever one the
                // Filtres bar is narrowed to, not a second dropdown of its
                // own (removed from the view this batch — see
                // OdvIndexVm.CanCreateOdv's comment). No AcMainGroup
                // equivalent exists to default here — legacy's Create ODV
                // never had a Groupe avion field either; that one stays a
                // manual choice for everyone, as before.
                vm.CreateModel.SquadronId = effectiveSquadronId.Value;

                var squadron = await _unitOfWork.Squadrons.GetByIdAsync(effectiveSquadronId.Value);
                ViewBag.SquadronName = squadron?.Name;
            }
            // else (unrestricted, no squadron currently filtered): leave
            // CreateModel.SquadronId at 0 — vm.CanCreateOdv below reflects
            // that there's no real single-squadron context to create
            // against yet, same as legacy never having an "all squadrons"
            // state for ddlSqnID.

            vm.CanCreateOdv = vm.CreateModel.SquadronId > 0;

            // Populate dropdowns (scoped)
            await PopulateSelectListsAsync(vm, scope, wingId);

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

                // BATCH 19: Wing narrows the same way squadronId already
                // does — intersect the user's real scope, never widen it.
                if (wingId.HasValue)
                {
                    allowedSquadronIds = allowedSquadronIds.Intersect(wingSquadronIds!).ToHashSet();
                }

                // BATCH 14: a scoped user's optional squadronId is only
                // ever used to further NARROW their own real scope, never
                // to widen it — intersect, don't replace. Uses
                // effectiveSquadronId (Batch 19) so a stale squadronId left
                // over from before a Wing change is ignored rather than
                // zeroing out the board.
                if (effectiveSquadronId.HasValue)
                {
                    allowedSquadronIds = allowedSquadronIds.Contains(effectiveSquadronId.Value)
                        ? new HashSet<int> { effectiveSquadronId.Value }
                        : new HashSet<int>(); // asked for a squadron outside their scope — show nothing, not an error
                }
            }
            else if (effectiveSquadronId.HasValue)
            {
                // BATCH 14: unrestricted (Admin) user manually narrowed via
                // the Escadron dropdown — reuse the same repository
                // parameter shape the scoped path already uses, no new
                // repository method needed.
                allowedSquadronIds = new HashSet<int> { effectiveSquadronId.Value };
            }
            else if (wingId.HasValue)
            {
                // NEW (Batch 19, 2026-08-30) — unrestricted user narrowed
                // by Wing only (no specific Escadron chosen yet).
                allowedSquadronIds = wingSquadronIds;
            }

            vm.Odvs = await _unitOfWork.Odvs.GetBoardForDateAsync(selectedDate, allowedSquadronIds, allowedAcMainGroupIds);

            // Load AcTypes for Sortie creation
            var allAcTypes = (await _unitOfWork.AcTypes.GetAllAsync())
                .OrderBy(a => a.Name)
                .ToList();

            vm.AcTypes = allAcTypes
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();

            // NEW (Batch 15, 2026-08-30) — see the Batch 15 comment on
            // vm.CrewMembersFull. Same list already fetched above, just
            // also exposed with its AcMainGroupId for client-side filtering.
            vm.AcTypesFull = allAcTypes;

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
            else if (model.SquadronId <= 0)
            {
                // NEW (Batch 19, 2026-08-30) — an unrestricted user no
                // longer picks the squadron via its own dropdown on this
                // form (removed this batch — Create ODV now always targets
                // whichever squadron the Filtres bar is narrowed to, per
                // the real legacy behaviour). model.SquadronId comes from
                // a hidden field the view leaves at 0 when nothing is
                // filtered (Model.CanCreateOdv is false and the form is
                // disabled in that state) — this is the server-side half of
                // that guard, in case the hidden field is ever missing or
                // tampered with (JS disabled, devtools, etc.).
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.SquadronId", new[] { "Select a squadron in the Filtres bar first." } }
                });
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
        private async Task PopulateSelectListsAsync(OdvIndexVm vm, UserScope scope, int? wingId = null)
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

            // NEW (Batch 19, 2026-08-30) — Wing filter, per Dadda's choice
            // to add legacy's "Wing: C.I.P.L" dropdown. Narrows the
            // Escadron list to that Wing's squadrons only (Squadron.WingId
            // is real and required — every squadron belongs to exactly one
            // Wing). Applied on top of whatever the scope branch above
            // already produced, so a scoped user's Wing filter still can't
            // widen past their own real scope.
            if (wingId.HasValue)
            {
                squadrons = squadrons.Where(s => s.WingId == wingId.Value);
            }

            vm.Squadrons = squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();

            vm.Wings = (await _unitOfWork.Wings.GetAllAsync())
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
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

            // NEW (Batch 15, 2026-08-30) — full entities for the "Ajouter
            // une sortie à un ODV" combined card's client-side cascading
            // filters (AcType by AcMainGroupId, CrewMember by SquadronId).
            // Reuses the exact same crewMembers/acMainGroups queries already
            // fetched above for this same scope — no new query. AcTypesFull
            // is intentionally unscoped (same as vm.AcTypes below already
            // was, unchanged) since AcType eligibility for a given ODV is
            // resolved client-side by AcMainGroupId, and re-checked
            // server-side by SortiesController.Create regardless.
            vm.CrewMembersFull = crewMembers
                .OrderBy(cm => cm.Captain)
                .ToList();

            // REMOVED (Batch 17, 2026-08-30) — Batch 16 populated
            // vm.AcMainGroupsFull here for the combined card's per-row Role
            // dropdown. AircraftRoleCatalog is now keyed on AcType.Code,
            // not AcMainGroup.Code (see AircraftRoleCatalog.cs's header),
            // and vm.AcTypesFull above already carries each AcType's Code
            // — so this field is no longer needed.

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
