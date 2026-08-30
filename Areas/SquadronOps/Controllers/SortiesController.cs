using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // REDESIGNED 2026-08-29 — "redesign from zero" pass. Was: FRAContext
    // injected directly (Batch 1/2 stopgap). Now: IUnitOfWork only, via the
    // new ISortiePlanningRepository (Sortie CRUD) and ISquadronRepository
    // (scope resolution) — see Areas/SquadronOps/Repositories/ for why each
    // one is shaped the way it is, and specifically why Sortie CRUD is NOT
    // on the existing Maintenance-owned ISortieRepository/"Sorties".
    //
    // Sortie has no Squadron/AcMainGroup of its own — it belongs to an Odv,
    // which carries both — so scope is resolved via the parent Odv, same
    // pattern as WorkOrderSection resolving via its parent WorkOrder in the
    // AircraftMaintenance conversion.
    //
    // ════════════════════════════════════════════════════════════════════
    // BATCH 11 (2026-08-29) — actor-gated workflow steps added:
    // RecordDeparture/RecordArrival (ATC/Tower), RecordPostFlight
    // (CrewChief), and AssignAircraft is now CrewChief-gated. Per Dadda's
    // confirmed sequence:
    //   "SquadronPlanner plans odv with sortie(s)...
    //    ODV/Sorties planned => Crewchief assign aircraft,
    //    ATC engine start and TOFF time =>
    //    Mission flown on RTB =>
    //    ATC real landing time + airfield activities data (TGO's, full
    //    stop landing) =>
    //    Crewchief post flight data (fuel and oil or snag or any other
    //    info) =>
    //    at squadron => all relevant mission data...=> Mission closed."
    //
    // Authorization design (per Dadda's confirmed decision, see handoff
    // doc "Batch 11" section for the full reasoning):
    //   - SquadronOpsRead/SquadronOpsWrite (ModuleAccessRequirement, via
    //     UserAssignment/ModuleRole/UserScope) stays the DATA-SCOPE gate —
    //     unchanged, still applied at the controller level below.
    //   - A SECOND, STACKED [Authorize] attribute on each actor-specific
    //     action gates WHO may call it, using the Identity roles already
    //     seeded in IdentitySeed.cs (SquadronPlanner/CrewChief/Tower) via
    //     new sibling policies to the pre-existing (previously unused)
    //     RequireCrewChiefOrAdmin: RequireTowerOrAdmin and
    //     RequireSquadronPlannerOrAdmin (added to Program.cs this batch).
    //     "Tower" IS "ATC" — same seeded role, per Dadda ("Tower = ATC").
    //   - This does NOT extend UserScope with RoleCodes — that earlier
    //     plan is dropped in favor of the simpler, already-seeded-role
    //     approach.
    //
    // BATCH 12 (2026-08-30) — ground-time ownership corrected. Per Dadda's
    // question on who actually feeds Block-Off/Block-On/Engine Start/Stop
    // in real practice (cross-checked against the real USAF AFTO 781 form):
    // ATC/tower cannot reliably observe ramp-side events like engine start
    // or chocks-off — that's ground crew (CrewChief) territory in
    // virtually every air force. EngineStartTime moved OFF RecordDeparture
    // (ATC keeps only RealTOFF) and, together with EngineStopTime/
    // BlockOffTime/BlockOnTime, now lives on CrewChief's RecordPostFlight —
    // all four as simple optional fields, recorded retrospectively
    // alongside fuel/oil/notes rather than a new real-time gated stage.
    // ENGINE_HOURS is now wired at Finalize as a result (both timestamps
    // finally have a source).
    // ════════════════════════════════════════════════════════════════════
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class SortiesController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;
        // NEW (Batch 8) — for the TGO_LANDINGS (and future aircraft-type-
        // specific dimension) increment at Finalize. NOT reached through
        // IUnitOfWork — it's registered as its own service in DI, same as
        // the real AircraftReadingProvider.cs shows (constructor takes
        // FRAContext directly, a separate architecture track from the
        // repository/UnitOfWork pattern everything else here follows).
        private readonly IAircraftReadingProvider _aircraftReadingProvider;

        // NEW (Batch 11) — CrewChief's post-flight snag report goes
        // through the real, existing Snag system rather than a
        // SquadronOps-only field. Already registered in DI
        // (Program.cs: AddScoped<ISnagService, SnagService>()).
        private readonly ISnagService _snagService;

        public SortiesController(IUnitOfWork unitOfWork,
                                 UserManager<ApplicationUser> userManager,
                                 IUserScopeService userScopeService,
                                 IAircraftReadingProvider aircraftReadingProvider,
                                 ISnagService snagService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userScopeService = userScopeService;
            _aircraftReadingProvider = aircraftReadingProvider;
            _snagService = snagService;
        }

        // GET: Sorties/Create?odvId=123
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(int odvId)
        {
            // NOTE: original loaded .Include(o => o.AcMainGroup) here, but
            // only ever reads odv.AcMainGroupId — a scalar FK already on
            // Odv itself. The AcMainGroup navigation was never
            // dereferenced, so the Include was dead weight. Dropped it —
            // a plain lookup is enough, no behaviour change.
            var odv = await _unitOfWork.Odvs.GetByIdAsync(odvId);

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
            var acTypes = (await _unitOfWork.AcTypes.GetWhereAsync(t => t.AcMainGroupId == acMainGroupId))
                .OrderBy(t => t.Name)
                .ToList();

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
        //
        // CHANGED (Batch 15, 2026-08-30) — one additive branch, per Dadda's
        // choice to rebuild the legacy "Add Sortie to ODV" one-step card as
        // client-orchestrated AJAX (OdvPlanning/Index.cshtml now POSTs here
        // directly, then chains into SortieCrews/Create for Captain/
        // Co-Pilot/Pax, since no combined backend endpoint exists). Every
        // existing caller of this action (the real Sorties/Create.cshtml
        // page, and its own GET/POST round trip) posts a normal browser
        // form with no X-Requested-With header, so it is completely
        // unaffected — same View(model)-on-failure / redirect-on-success
        // behaviour as before this batch. Only a request carrying
        // X-Requested-With: XMLHttpRequest (the same header the board's
        // existing data-ajax="true" forms already send) gets the new JSON
        // branch: BadRequest(errors) on failure, Json({sortieId,
        // sortieCode}) on success, instead of View(model)/Redirect.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(SortieCreateVm model, int? acMainGroupId)
        {
            bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            var odv = await _unitOfWork.Odvs.GetByIdAsync(model.OdvId);
            if (odv == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsOdvInScopeAsync(odv.SquadronId, odv.AcMainGroupId, scope))
                return Forbid();

            // Server-side guard — the dropdown only offers AcTypes within the
            // Odv's own AcMainGroup, but AcTypeId is still a posted value.
            var chosenAcType = await _unitOfWork.AcTypes.GetFirstOrDefaultAsync(t => t.Id == model.AcTypeId);
            if (chosenAcType == null || chosenAcType.AcMainGroupId != odv.AcMainGroupId)
            {
                ModelState.AddModelError(nameof(model.AcTypeId), "Selected aircraft type does not match this ODV's aircraft group.");
            }

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                    return BadRequest(errors);
                }

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

            _unitOfWork.SortiePlanning.Add(sortie);
            await _unitOfWork.CompleteAsync();

            if (isAjax)
            {
                return Json(new { sortieId = sortie.Id, sortieCode = sortie.SortieCode });
            }

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
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);

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

            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.Id);

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
                var newAcType = await _unitOfWork.AcTypes.GetFirstOrDefaultAsync(t => t.Id == model.AcTypeId);
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

            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Index", "OdvPlanning");
        }

        // GET: Sorties/AssignAircraft/5
        //
        // NEW (2026-08-29, Batch 9). Dedicated aircraft-assignment step,
        // per Dadda's confirmed decision — its own action, matching
        // SortieStatus.AircraftAssigned as a real workflow stage, rather
        // than a field on Create/Edit. Also per Dadda's confirmed
        // decisions: keeps Aircraft.Status in sync (Available <-> Assigned,
        // reversed in Cancel/Finalize above), and restricts the picker to
        // aircraft matching the sortie's AcTypeId AND currently Available
        // (see PopulateEligibleAircraft below).
        //
        // BATCH 11: now CrewChief-gated — "Crewchief assign aircraft" per
        // Dadda's sequence. Stacked on top of the module-level
        // SquadronOpsWrite check.
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireCrewChiefOrAdmin")]
        public async Task<IActionResult> AssignAircraft(int id)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status == SortieStatus.Canceled || sortie.Status == SortieStatus.Finalized)
            {
                TempData["Warning"] = "This sortie is cancelled or finalized — its aircraft assignment can no longer be changed.";
                return RedirectToAction("Index", "OdvPlanning");
            }

            var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);

            var vm = new SortieAssignAircraftVm
            {
                SortieId = sortie.Id,
                AircraftId = sortie.AircraftId ?? 0,
                SortieCode = sortie.SortieCode,
                AcTypeName = acType?.Name
            };

            await PopulateEligibleAircraft(vm, sortie.AcTypeId, sortie.AircraftId);

            return View(vm);
        }

        // POST: Sorties/AssignAircraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireCrewChiefOrAdmin")]
        public async Task<IActionResult> AssignAircraft(SortieAssignAircraftVm model)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.SortieId);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status == SortieStatus.Canceled || sortie.Status == SortieStatus.Finalized)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie is cancelled or finalized — its aircraft assignment can no longer be changed." } }
                });
            }

            var newAircraft = await _unitOfWork.Aircraft.GetByIdAsync(model.AircraftId);
            if (newAircraft == null)
            {
                ModelState.AddModelError(nameof(model.AircraftId), "Selected aircraft was not found.");
            }
            else if (newAircraft.AcTypeId != sortie.AcTypeId)
            {
                // Defense in depth — the dropdown only offers aircraft
                // matching the sortie's own AcTypeId, but AircraftId is
                // still a posted value.
                ModelState.AddModelError(nameof(model.AircraftId), "Selected aircraft does not match this sortie's aircraft type.");
            }
            else if (newAircraft.Status != AircraftStatus.Available && newAircraft.Id != sortie.AircraftId)
            {
                // Allow re-posting the SAME aircraft that's already
                // assigned to this sortie (it legitimately shows as
                // Assigned, not Available) — reject any other non-Available
                // aircraft.
                ModelState.AddModelError(nameof(model.AircraftId), "Selected aircraft is not currently available.");
            }

            if (!ModelState.IsValid)
            {
                model.SortieCode = sortie.SortieCode;
                var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                model.AcTypeName = acType?.Name;
                await PopulateEligibleAircraft(model, sortie.AcTypeId, sortie.AircraftId);
                return View(model);
            }

            var now = DateTime.UtcNow;

            // Release the previously-assigned aircraft, if any and if it's
            // actually changing.
            if (sortie.AircraftId.HasValue && sortie.AircraftId.Value != model.AircraftId)
            {
                var previousAircraft = await _unitOfWork.Aircraft.GetByIdAsync(sortie.AircraftId.Value);
                if (previousAircraft != null)
                {
                    previousAircraft.Status = AircraftStatus.Available;
                    previousAircraft.LastModifiedAt = now;
                }
            }

            sortie.AircraftId = model.AircraftId;
            // Only ever advance the workflow forward from Planned — never
            // regress a sortie that's already past that stage (e.g. a
            // future "reassign after Landed" case, if that's ever allowed)
            // back down to AircraftAssigned.
            if (sortie.Status == SortieStatus.Planned)
                sortie.Status = SortieStatus.AircraftAssigned;
            sortie.UpdatedAtUtc = now;

            newAircraft!.Status = AircraftStatus.Assigned;
            newAircraft.LastModifiedAt = now;

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // Aircraft eligible for assignment: same AcTypeId as the sortie,
        // AND currently Available — OR the aircraft already assigned to
        // THIS sortie (so re-opening the form to change/confirm the
        // assignment still shows it, even though its own Status is
        // Assigned rather than Available by that point). Per Dadda's
        // confirmed decision — both filters, not just AcType.
        private async Task PopulateEligibleAircraft(SortieAssignAircraftVm vm, int acTypeId, int? currentAircraftId)
        {
            var eligible = (await _unitOfWork.Aircraft.GetWhereAsync(a =>
                    a.AcTypeId == acTypeId &&
                    (a.Status == AircraftStatus.Available || a.Id == currentAircraftId)))
                .OrderBy(a => a.Registration)
                .ToList();

            vm.Aircraft = eligible.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.DisplayName,
                Selected = a.Id == currentAircraftId
            }).ToList();

            if (!eligible.Any())
            {
                TempData["Warning"] = "No available aircraft of this type to assign.";
            }
        }

        // ════════════════════════════════════════════════════════════════
        // GET/POST: Sorties/RecordDeparture/5 — NEW (Batch 11, 2026-08-29)
        //
        // ATC/Tower pre-flight step. Only allowed once an aircraft is
        // assigned (Status == AircraftAssigned); advances to Airborne.
        // Gated RequireTowerOrAdmin on top of module-level SquadronOpsWrite
        // — "Tower" IS "ATC" per Dadda's confirmation.
        //
        // CHANGED (Batch 12, 2026-08-30) — EngineStartTime REMOVED from
        // this action. Per Dadda's question on who actually feeds engine
        // start/stop and block-off/block-on in real practice: ATC/tower has
        // no reliable way to observe engine ignition (it happens on the
        // ramp, often before the aircraft is even on frequency) — that's
        // ground-crew territory, cross-checked against the real USAF AFTO
        // 781 form's own division of labor (pilot logs TOFF/Landing, crew
        // chief owns the aircraft-side ground data). RecordDeparture now
        // captures ONLY RealTOFF — the one transition tower can actually
        // attest to. EngineStartTime moved to CrewChief's RecordPostFlight
        // below, alongside EngineStopTime/BlockOffTime/BlockOnTime.
        // ════════════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireTowerOrAdmin")]
        public async Task<IActionResult> RecordDeparture(int id)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.AircraftAssigned)
            {
                TempData["Warning"] = $"This sortie is not ready for departure (current status: {sortie.Status}). An aircraft must be assigned first, and departure cannot be re-recorded once the sortie has moved past this stage.";
                return RedirectToAction("Index", "OdvPlanning");
            }

            var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);

            var vm = new SortieRecordDepartureVm
            {
                SortieId = sortie.Id,
                SortieCode = sortie.SortieCode,
                AcTypeName = acType?.Name,
                RealTOFF = DateTime.UtcNow
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireTowerOrAdmin")]
        public async Task<IActionResult> RecordDeparture(SortieRecordDepartureVm model)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.SortieId);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.AircraftAssigned)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { $"This sortie is not ready for departure (current status: {sortie.Status})." } }
                });
            }

            if (!ModelState.IsValid)
            {
                var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                model.SortieCode = sortie.SortieCode;
                model.AcTypeName = acType?.Name;
                return View(model);
            }

            sortie.RealTOFF = model.RealTOFF;
            sortie.Status = SortieStatus.Airborne;
            sortie.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // ════════════════════════════════════════════════════════════════
        // GET/POST: Sorties/RecordArrival/5 — NEW (Batch 11, 2026-08-29)
        //
        // ATC/Tower post-flight step: "ATC real landing time + airfield
        // activities data (total TGO's, and full stop landing)". Only
        // allowed once airborne (Status == Airborne); advances to Landed.
        // This is now the ONLY place Landings/TGOsLandings are set — see
        // the comment on those two fields in Sortie.cs.
        // ════════════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireTowerOrAdmin")]
        public async Task<IActionResult> RecordArrival(int id)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.Airborne)
            {
                TempData["Warning"] = $"This sortie is not airborne (current status: {sortie.Status}) — arrival cannot be recorded yet.";
                return RedirectToAction("Index", "OdvPlanning");
            }

            var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);

            var vm = new SortieRecordArrivalVm
            {
                SortieId = sortie.Id,
                SortieCode = sortie.SortieCode,
                AcTypeName = acType?.Name,
                RealTOFF = sortie.RealTOFF,
                RealLandingTime = DateTime.UtcNow
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireTowerOrAdmin")]
        public async Task<IActionResult> RecordArrival(SortieRecordArrivalVm model)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.SortieId);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.Airborne)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { $"This sortie is not airborne (current status: {sortie.Status})." } }
                });
            }

            if (sortie.RealTOFF.HasValue && model.RealLandingTime.HasValue && model.RealLandingTime < sortie.RealTOFF)
            {
                ModelState.AddModelError(nameof(model.RealLandingTime), "Landing time cannot be before take-off time.");
            }

            if (!ModelState.IsValid)
            {
                var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                model.SortieCode = sortie.SortieCode;
                model.AcTypeName = acType?.Name;
                model.RealTOFF = sortie.RealTOFF;
                return View(model);
            }

            sortie.RealLandingTime = model.RealLandingTime;
            sortie.Landings = model.Landings;
            sortie.TGOsLandings = model.TGOsLandings;
            sortie.Status = SortieStatus.Landed;
            sortie.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // ════════════════════════════════════════════════════════════════
        // GET/POST: Sorties/RecordPostFlight/5 — NEW (Batch 11, 2026-08-29)
        //
        // CrewChief's post-flight step: "Crewchief post flight data (fuel
        // and oil or snag or any other info)". Only allowed once landed
        // (Status == Landed); advances to CrewChiefReported, which is what
        // unblocks Squadron's Finalize below. Snag reporting is optional
        // and goes through the real ISnagService, per Dadda's confirmed
        // decision to reuse the existing AircraftMaintenance Snag system
        // rather than a SquadronOps-only field.
        //
        // BATCH 12: also captures BlockOffTime/EngineStartTime/
        // EngineStopTime/BlockOnTime — see the class-level Batch 12 comment.
        // ════════════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireCrewChiefOrAdmin")]
        public async Task<IActionResult> RecordPostFlight(int id)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.Landed)
            {
                TempData["Warning"] = $"This sortie is not ready for a post-flight report (current status: {sortie.Status}) — it must be recorded as landed by ATC first.";
                return RedirectToAction("Index", "OdvPlanning");
            }

            var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);

            var vm = new SortieRecordPostFlightVm
            {
                SortieId = sortie.Id,
                SortieCode = sortie.SortieCode,
                AcTypeName = acType?.Name,
                RealTOFF = sortie.RealTOFF,
                RealLandingTime = sortie.RealLandingTime
            };

            await PopulateAtaOptions(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireCrewChiefOrAdmin")]
        public async Task<IActionResult> RecordPostFlight(SortieRecordPostFlightVm model)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.SortieId);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status != SortieStatus.Landed)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { $"This sortie is not ready for a post-flight report (current status: {sortie.Status})." } }
                });
            }

            // Conditional validation — the snag block is only required when
            // ReportSnag is checked. DataAnnotations can't express "required
            // only if this other field is true" cleanly, so it's done here.
            if (model.ReportSnag)
            {
                if (!model.AtaId.HasValue)
                    ModelState.AddModelError(nameof(model.AtaId), "ATA chapter is required when reporting a snag.");
                if (!model.Severity.HasValue)
                    ModelState.AddModelError(nameof(model.Severity), "Severity is required when reporting a snag.");
                if (!model.Impact.HasValue)
                    ModelState.AddModelError(nameof(model.Impact), "Airworthiness impact is required when reporting a snag.");
                if (!model.ReportedBy.HasValue)
                    ModelState.AddModelError(nameof(model.ReportedBy), "Reported by (Aircrew/Maintenance) is required when reporting a snag.");
                if (string.IsNullOrWhiteSpace(model.SnagDescription))
                    ModelState.AddModelError(nameof(model.SnagDescription), "Description is required when reporting a snag.");
            }

            if (sortie.AircraftId == null)
            {
                // Shouldn't be reachable — a sortie can't reach Landed
                // without an assigned aircraft — but guard anyway rather
                // than let a null-AircraftId slip into the Snag report.
                ModelState.AddModelError("", "This sortie has no aircraft assigned — cannot record post-flight data.");
            }

            // NEW (Batch 12) — ground-time sanity checks. Soft/non-blocking
            // where the underlying data is genuinely just operator-entered
            // free text (per Dadda: "keep their text field"), matching the
            // Finalize duration-mismatch warning's own precedent rather than
            // hard-blocking on a ground crew's retrospective, from-memory
            // entry.
            if (model.EngineStopTime.HasValue && model.EngineStartTime.HasValue && model.EngineStopTime < model.EngineStartTime)
            {
                ModelState.AddModelError(nameof(model.EngineStopTime), "Engine stop time cannot be before engine start time.");
            }
            if (model.BlockOnTime.HasValue && model.BlockOffTime.HasValue && model.BlockOnTime < model.BlockOffTime)
            {
                ModelState.AddModelError(nameof(model.BlockOnTime), "Block-on time cannot be before block-off time.");
            }

            if (!ModelState.IsValid)
            {
                var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                model.SortieCode = sortie.SortieCode;
                model.AcTypeName = acType?.Name;
                model.RealTOFF = sortie.RealTOFF;
                model.RealLandingTime = sortie.RealLandingTime;
                await PopulateAtaOptions(model);
                return View(model);
            }

            var actor = await GetActorNameAsync();
            var now = DateTime.UtcNow;

            sortie.FuelUsedLiters = model.FuelUsedLiters;
            sortie.PostFlightOilUsedLiters = model.OilUsedLiters;
            sortie.PostFlightNotes = model.Notes;
            sortie.PostFlightReportedAtUtc = now;
            sortie.PostFlightReportedBy = actor;

            // NEW (Batch 12) — ground times, moved here from ATC's
            // RecordDeparture (EngineStartTime) and newly captured
            // (EngineStopTime/BlockOffTime/BlockOnTime). See
            // SortieRecordPostFlightVm's class comment for the full
            // reasoning (who actually observes these events in real
            // practice).
            sortie.BlockOffTime = model.BlockOffTime;
            sortie.EngineStartTime = model.EngineStartTime;
            sortie.EngineStopTime = model.EngineStopTime;
            sortie.BlockOnTime = model.BlockOnTime;

            if (model.ReportSnag)
            {
                var aircraftId = sortie.AircraftId!.Value;

                // DiscoveryFH — best-effort snapshot of the aircraft's FH
                // AT THE MOMENT OF DISCOVERY. This sortie's own flight time
                // has not been added to Aircraft.TotalFlightMinutes yet
                // (that happens at Finalize, further below/later) — so this
                // intentionally reads the PRE-sortie total, which is the
                // correct "as of discovery" semantic per Snag.DiscoveryFH's
                // own doc comment ("Position-at-discovery snapshot —
                // immutable"). Not yet confirmed with Dadda as the intended
                // interpretation — flagging rather than silently guessing
                // past it.
                var aircraft = await _unitOfWork.Aircraft.GetByIdAsync(aircraftId);

                // BaseId resolution for the required Snag.DiscoveryBaseId —
                // Sortie.BaseId is nullable and not confirmed to always be
                // set (see handoff doc's open item on Odv/Sortie.BaseId);
                // fall back to the parent Odv's BaseId. Fails loudly rather
                // than defaulting to an invalid FK of 0 if neither resolves.
                var discoveryBaseId = sortie.BaseId ?? sortie.Odv?.BaseId;
                if (!discoveryBaseId.HasValue)
                {
                    ModelState.AddModelError("", "Cannot report a snag — this sortie/ODV has no Base on record.");
                    var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                    model.SortieCode = sortie.SortieCode;
                    model.AcTypeName = acType?.Name;
                    model.RealTOFF = sortie.RealTOFF;
                    model.RealLandingTime = sortie.RealLandingTime;
                    await PopulateAtaOptions(model);
                    return View(model);
                }

                var dto = new SnagCreateDto
                {
                    AircraftId = aircraftId,
                    AtaId = model.AtaId!.Value,
                    Severity = model.Severity!.Value,
                    Impact = model.Impact!.Value,
                    ReportedBy = model.ReportedBy!.Value,
                    DiscoveryPhase = DiscoveryPhase.FLIGHT,
                    DiscoveryFH = aircraft?.TotalFlightMinutes ?? 0,
                    DiscoveryDate = DateOnly.FromDateTime(now),
                    DiscoveryBaseId = discoveryBaseId.Value,
                    Description = model.SnagDescription!
                };

                var (success, message, snagId) = await _snagService.ReportAsync(dto, actor ?? "unknown");
                if (!success)
                {
                    ModelState.AddModelError("", $"Snag could not be reported: {message}");
                    var acType = await _unitOfWork.AcTypes.GetByIdAsync(sortie.AcTypeId);
                    model.SortieCode = sortie.SortieCode;
                    model.AcTypeName = acType?.Name;
                    model.RealTOFF = sortie.RealTOFF;
                    model.RealLandingTime = sortie.RealLandingTime;
                    await PopulateAtaOptions(model);
                    return View(model);
                }

                sortie.SnagId = snagId;
            }

            sortie.Status = SortieStatus.CrewChiefReported;
            sortie.UpdatedAtUtc = now;

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // ATA chapter dropdown for the snag block. IAtaRepository is
        // assumed to extend IGenericRepository<Ata> (same "specialist
        // extends generic" convention as every other specialist repo on
        // IUnitOfWork) — GetAllAsync() has not been directly confirmed on
        // it specifically; flag and adjust if it doesn't compile as-is.
        private async Task PopulateAtaOptions(SortieRecordPostFlightVm vm)
        {
            var chapters = (await _unitOfWork.Ata.GetAllAsync())
                .OrderBy(a => a.Code)
                .ToList();

            vm.AtaOptions = chapters.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Code} — {a.Name}",
                Selected = a.Id == vm.AtaId
            }).ToList();
        }

        // POST: Sorties/Cancel/5
        // Individual per-Sortie cancellation with its own reason — separate
        // from, and independent of, cancelling the parent Odv (which
        // cascades to all its Sorties from OdvPlanningController.Cancel
        // instead). Does not touch the parent Odv or sibling Sorties.
        // Unchanged behaviour from Batch 2 — data access moved to
        // IUnitOfWork.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(id);

            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status == SortieStatus.Canceled)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie is already cancelled." } }
                });
            }

            if (sortie.Status == SortieStatus.Finalized)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie is already finalized and cannot be cancelled — its flight/engine hours have already been recorded." } }
                });
            }

            sortie.Status = SortieStatus.Canceled;
            sortie.CancellationReason = reason;
            sortie.CancelledAtUtc = DateTime.UtcNow;
            // FIX (2026-08-29) — CancelledBy was added to Sortie.cs back in
            // Batch 2 but this action never actually set it. Caught while
            // adding Finalize below, which needs the same
            // "who did this" lookup. Odv has no CancelledBy field (only
            // Sortie does), so nothing to fix on the OdvPlanningController
            // side.
            sortie.CancelledBy = await GetActorNameAsync();
            sortie.UpdatedAtUtc = DateTime.UtcNow;

            // NEW (Batch 9) — release the assigned aircraft back to
            // Available on cancellation, per Dadda's confirmed decision to
            // keep Aircraft.Status in sync with Sortie assignment. Mirrors
            // the same release logic in Finalize and AssignAircraft below.
            // NOTE: this only covers per-Sortie Cancel here — the
            // Odv-level cascade cancellation (OdvPlanningController.Cancel)
            // is a different controller not reviewed in this batch, so any
            // sortie cancelled that way does NOT yet release its aircraft.
            // Flagging, not guessing at that file's shape.
            if (sortie.AircraftId.HasValue)
            {
                var assignedAircraft = await _unitOfWork.Aircraft.GetByIdAsync(sortie.AircraftId.Value);
                if (assignedAircraft != null)
                {
                    assignedAircraft.Status = AircraftStatus.Available;
                    assignedAircraft.LastModifiedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // POST: Sorties/Finalize
        //
        // NEW (2026-08-29). This is the Squadron final report step from
        // Dadda's original sequence: "...Crewchief post flight data...at
        // squadron => all relevant mission (sortie) data, flight duration,
        // Instrument, instrument simulated, approaches and more =>
        // Mission(sortie) closed." SortieFinalizeVm already existed in the
        // real project — reused as-is (field names/shapes unchanged) rather
        // than inventing a new one, since it already matches Sortie's own
        // "Squadron final report metrics" fields one-to-one. What's NEW
        // here is the action itself: no Finalize action for the current
        // (post-redesign) SortiesController existed anywhere before this.
        // The old FRAProject.Controllers.OdvsController.FinalizeSortieData
        // (pre-restart code, FRAContext-direct) is confirmed dead/leftover
        // and is NOT ported from or built on — this is written fresh
        // against IUnitOfWork.
        //
        // Two things this action fixes relative to just wiring the VM
        // straight through:
        //   1. SortieFinalizeVm.DayMinutes/NightMinutes are minutes;
        //      Sortie.DayHours/NightHours are hours (double). Converted
        //      explicitly below — found while reviewing the real VM file,
        //      not something the VM itself or anything seen so far handles.
        //   2. The DurationMinutes-vs-computed-airframe-time warning from
        //      the F16 block-off/holding-point conversation (locked spec,
        //      Batch 4) — non-blocking, per Dadda's decision, comparing the
        //      entered DurationMinutes against (RealLandingTime - RealTOFF)
        //      when both are set. 5-minute tolerance is still the same
        //      un-confirmed placeholder flagged in Batch 4.
        //
        // ════════════════════════════════════════════════════════════════
        // BATCH 11 CHANGES:
        //   1. NEW GUARD — Finalize now BLOCKS unless
        //      sortie.Status == SortieStatus.CrewChiefReported, i.e. ATC's
        //      departure AND arrival steps AND CrewChief's post-flight
        //      report must all have already happened, per Dadda's
        //      confirmed sequence being a strict order, not independent
        //      optional steps. This is a NEW business rule this batch
        //      introduces — flagged prominently, easy to relax (e.g. to
        //      "!= Canceled && != Finalized" like before) if some sortie
        //      types legitimately skip ATC/CrewChief involvement.
        //   2. Landings/TGOsLandings are NO LONGER copied from
        //      model — ATC's RecordArrival already set them on the Sortie
        //      directly. SortieFinalizeVm still HAS these two properties
        //      (real file, left unchanged) but they're now ignored here.
        //      If a real view still posts into them, its two inputs should
        //      be removed/hidden — not something this batch can fix
        //      without seeing that view.
        //   3. FlightLog.OilUsedLiters is now populated from
        //      sortie.PostFlightOilUsedLiters (CrewChief's entry).
        //      FlightLog.Notes is populated from sortie.PostFlightNotes.
        //      FlightLog.FuelUsedKg is deliberately left unset — see the
        //      Liters-vs-Kg mismatch flagged on Sortie.FuelUsedLiters.
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        [Authorize(Policy = "RequireSquadronPlannerOrAdmin")]
        public async Task<IActionResult> Finalize(SortieFinalizeVm model)
        {
            if (model == null || model.SortieId <= 0)
                return BadRequest("sortieId required");

            var sortie = await _unitOfWork.SortiePlanning.GetByIdWithOdvAsync(model.SortieId);
            if (sortie == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (sortie.Odv == null || !await IsOdvInScopeAsync(sortie.Odv.SquadronId, sortie.Odv.AcMainGroupId, scope))
                return Forbid();

            if (sortie.Status == SortieStatus.Canceled)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie is cancelled and cannot be finalized." } }
                });
            }

            if (sortie.Status == SortieStatus.Finalized)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie is already finalized." } }
                });
            }

            // NEW (Batch 11) — see the big comment block above this action.
            if (sortie.Status != SortieStatus.CrewChiefReported)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { $"This sortie cannot be finalized yet (current status: {sortie.Status}). " +
                                  "ATC must record departure and arrival, and CrewChief must submit the post-flight " +
                                  "report, before Squadron can close out the mission." } }
                });
            }

            // NEW (Batch 8) — per Dadda's confirmed decision: Finalize must
            // feed Aircraft's real component-life counters and FlightLog,
            // and both need a specific airframe (sortie.AircraftId), not
            // just the planned AcTypeId. Nothing in Create/Edit sets
            // AircraftId anywhere in this codebase today — the
            // "AircraftAssigned" SortieStatus stage has no action behind it
            // yet — so this blocks rather than silently skipping the
            // maintenance-side update, per Dadda's choice.
            if (sortie.AircraftId == null)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { "This sortie has no aircraft (tail number) assigned yet. " +
                                  "Assign an aircraft before finalizing — flight/engine hours cannot be recorded against an unknown airframe." } }
                });
            }

            // ── Squadron final report field mapping ──
            // BATCH 11: Landings/TGOsLandings intentionally NOT mapped from
            // model here anymore — see the class-level Batch 11 comment.
            sortie.DurationMinutes = model.DurationMinutes;
            sortie.Approachs = model.Approachs;
            sortie.InstSimulated = model.InstSimulated;
            sortie.InstActual = model.InstActual;
            sortie.IFRHours = model.IFRHours;
            sortie.Interceptions = model.Interceptions;
            sortie.RadarContacts = model.RadarContacts;
            sortie.AppContacts = model.AppContacts;
            sortie.SquadronReportNotes = model.SquadronReportNotes;

            // FIX — minutes -> hours. Only overwrite when the form actually
            // supplied a value; leaves any existing DayHours/NightHours
            // untouched otherwise (e.g. a partial re-finalize is not
            // possible today since Finalized is terminal-ish, but this
            // matches the "don't stomp what wasn't provided" convention
            // used elsewhere, e.g. Cancel's reason handling).
            if (model.DayMinutes.HasValue)
                sortie.DayHours = model.DayMinutes.Value / 60.0;
            if (model.NightMinutes.HasValue)
                sortie.NightHours = model.NightMinutes.Value / 60.0;

            // ── DurationMinutes-vs-airframe-time check (locked spec) ──
            const int ToleranceMinutes = 5; // placeholder, not confirmed by Dadda
            if (sortie.RealTOFF.HasValue && sortie.RealLandingTime.HasValue && model.DurationMinutes.HasValue)
            {
                var expectedMinutes = (sortie.RealLandingTime.Value - sortie.RealTOFF.Value).TotalMinutes;
                var diff = model.DurationMinutes.Value - expectedMinutes;

                if (Math.Abs(diff) > ToleranceMinutes)
                {
                    TempData["Warning"] =
                        $"Entered duration ({model.DurationMinutes.Value} min) differs from computed " +
                        $"airframe time ({expectedMinutes:F0} min, RealTOFF to RealLandingTime) by " +
                        $"{diff:F0} min. Ground delays before TOFF should not be included in Duration " +
                        "— consider Block Duration instead.";
                }
            }

            var actor = await GetActorNameAsync();
            var now = DateTime.UtcNow;

            sortie.Status = SortieStatus.Finalized;
            sortie.IsFinalized = true;
            sortie.FinalizedAtUtc = now;
            sortie.FinalizedBy = actor;

            // ADDED (2026-08-29) — per Dadda's clarification: IsCompleted/
            // CompletedAtUtc/CompletedBy are squadron-level post-flight
            // fields, set in this SAME action rather than a separate
            // "CrewChief" step as originally assumed in Batch 6's README.
            // Reusing the actor/timestamp already resolved above rather
            // than calling GetActorNameAsync()/DateTime.UtcNow twice.
            sortie.IsCompleted = true;
            sortie.CompletedAtUtc = now;
            sortie.CompletedBy = actor;

            sortie.UpdatedAtUtc = now;

            // ════════════════════════════════════════════════════════════
            // NEW (Batch 8) — maintenance-log feed + component-life
            // increments. Per Dadda: "the data entered by squadron ops
            // should go to maintenance log and increment maintenance data
            // (FH, cycles and other variables)". AircraftId is guaranteed
            // non-null here (guard above).
            // ════════════════════════════════════════════════════════════
            var aircraftId = sortie.AircraftId.Value;

            var aircraft = await _unitOfWork.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null)
            {
                // Shouldn't happen — AircraftId is FK-constrained — but this
                // is a hard data-integrity problem, not a normal validation
                // error, so fail loudly rather than silently skipping the
                // increment.
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "", new[] { $"Aircraft {aircraftId} referenced by this sortie was not found." } }
                });
            }

            // FH source = Squadron-entered DurationMinutes — Dadda's
            // explicit, locked choice (not computed RealTOFF/
            // RealLandingTime, which is what I'd recommended). This
            // reopens the F16 ground-delay risk directly against real
            // component life if DurationMinutes is ever entered wrong; the
            // duration-mismatch warning above is the only safeguard, by
            // design — do not silently "fix" this to the computed value.
            aircraft.TotalFlightMinutes += sortie.DurationMinutes ?? 0;

            // ASSUMPTION, not confirmed by Dadda: one cycle per finalized
            // sortie. Matches FlightLog.Cycles's own model default (= 1)
            // below. Sortie.Cycles exists on the model but SortieFinalizeVm
            // never captures it, so there's no per-sortie override coming
            // from the form today — flagging rather than guessing at a
            // formula for anything other than a flat +1.
            aircraft.TotalCycles += 1;

            // FULLSTOP_LANDINGS — full-stop landings, distinct from
            // TGOsLandings (touch-and-goes, handled below via
            // AircraftReadingProvider). Confirmed by
            // AircraftReadingProvider.GetCurrentReadingsAsync's own mapping
            // (TotalLandings -> "FULLSTOP_LANDINGS"). BATCH 11: sortie.
            // Landings now comes from ATC's RecordArrival, not this action.
            aircraft.TotalLandings += sortie.Landings ?? 0;

            // NEW (Batch 9) — release the aircraft back to Available now
            // that the sortie is done, per Dadda's confirmed decision to
            // keep Aircraft.Status in sync with Sortie assignment. Mirrors
            // the same release logic in Cancel and AssignAircraft.
            aircraft.Status = AircraftStatus.Available;

            aircraft.LastModifiedAt = now;
            // No explicit Update()/Add() call needed — GetByIdAsync fetched
            // a tracked entity (GenericRepository uses _context.Set<T>()
            // .FindAsync), same convention as Edit's sortie mutation above:
            // mutate directly, EF picks it up on SaveChanges.

            // FlightLog — "the authoritative record created once a Sortie
            // is completed" (FlightLog.cs's own doc comment), one row per
            // finalized sortie. TakeOffUtc/LandingUtc map straight from
            // Sortie's own RealTOFF/RealLandingTime (the airframe-time
            // pair, never Block*). Hobbs/Tach are left unset — nothing in
            // the current Finalize form (or SortieFinalizeVm) captures
            // them; Sortie does carry its own HobbsStart/HobbsEnd/
            // TachStart/TachEnd (double?) but nothing populates those
            // either, and they don't even match FlightLog's decimal? type
            // — a real gap, not silently papered over, just out of scope
            // for this batch.
            //
            // BATCH 11: OilUsedLiters and Notes now populated from
            // CrewChief's post-flight report (sortie.PostFlightOilUsedLiters/
            // PostFlightNotes). FuelUsedKg deliberately left unset — see
            // the Liters-vs-Kg mismatch comment on Sortie.FuelUsedLiters.
            var flightLog = new FlightLog
            {
                SortieId = sortie.Id,
                AircraftId = aircraftId,
                TakeOffUtc = sortie.RealTOFF,
                LandingUtc = sortie.RealLandingTime,
                DurationMinutes = sortie.DurationMinutes,
                Cycles = 1,
                OilUsedLiters = sortie.PostFlightOilUsedLiters,
                Notes = sortie.PostFlightNotes,
                CreatedAtUtc = now,
                CreatedBy = actor
            };
            _unitOfWork.FlightLogs.Add(flightLog);

            // TGO_LANDINGS — the one aircraft-type-specific dimension
            // SortieFinalizeVm already captures (TGOsLandings). Routed
            // through the generic AircraftReading table per the confirmed
            // Hybrid decision, never through a new Aircraft column.
            // BATCH 11: sortie.TGOsLandings now comes from ATC's
            // RecordArrival, not Squadron's Finalize form.
            //
            // CAVEAT, inherited from AircraftReadingProvider's own existing
            // architecture (flagged in its doc comment, not introduced
            // here): it takes FRAContext directly and calls
            // SaveChangesAsync() internally. Calling it here — after every
            // other change above has already been staged on the same
            // DbContext — means this one call flushes the ENTIRE Finalize
            // transaction (sortie fields, aircraft counters, the new
            // FlightLog row, and the reading) together; the
            // _unitOfWork.CompleteAsync() call further below then commits
            // zero additional changes. Not broken, but NOT a clean single
            // transaction either — if this call throws partway, whatever it
            // already wrote is not rolled back the way a real
            // Task<IActionResult> failure elsewhere in this action would
            // be. Flagging as pre-existing debt to fix if/when
            // AircraftReadingProvider moves onto IUnitOfWork.
            if (sortie.TGOsLandings is int tgo && tgo != 0)
            {
                await _aircraftReadingProvider.IncrementReadingAsync(aircraftId, "TGO_LANDINGS", tgo);
            }

            // ENGINE_HOURS — NEW (Batch 12, 2026-08-30). Both
            // EngineStartTime and EngineStopTime are now captured (by
            // CrewChief's RecordPostFlight), so this can finally be wired
            // up. Stored in MINUTES, not fractional hours, despite the
            // "HOURS" name — IAircraftReadingProvider's own doc comment
            // confirms FH ("this module's minutes convention exactly") and
            // this project has no other precedent for the unit an
            // AircraftReading row holds, so ENGINE_HOURS follows the same
            // convention as FH/TGO_LANDINGS rather than introducing a
            // fractional-hours exception. Flagging: not explicitly
            // confirmed by Dadda — if ENGINE_HOURS is meant to be true
            // decimal hours, this needs a different delta (and possibly a
            // provider change, since IncrementReadingAsync's delta is
            // `int`, not `decimal`).
            if (sortie.EngineStartTime.HasValue && sortie.EngineStopTime.HasValue)
            {
                var engineMinutes = (int)Math.Round((sortie.EngineStopTime.Value - sortie.EngineStartTime.Value).TotalMinutes);
                if (engineMinutes > 0)
                {
                    await _aircraftReadingProvider.IncrementReadingAsync(aircraftId, "ENGINE_HOURS", engineMinutes);
                }
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index", "OdvPlanning");
        }

        // ── Scope helpers ────────────────────────────────────────────────

        // Matches OdvPlanningController.IsSquadronInScopeAsync exactly — both
        // now delegate to the same ISquadronRepository.GetScopeInfoAsync, so
        // there is exactly ONE place left that resolves a squadron's
        // authorization-scope base (Squadron -> Wing -> Department -> Base).
        // This is what eliminates the duplication that caused the original
        // Wing.BaseId-vs-Department.BaseId bug.
        private async Task<bool> IsOdvInScopeAsync(int squadronId, int acMainGroupId, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            if (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(acMainGroupId))
                return false;

            var info = await _unitOfWork.Squadrons.GetScopeInfoAsync(squadronId);
            if (info == null) return false;

            if (!scope.AllowedBaseIds.Contains(info.Value.BaseId)) return false;
            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(info.Value.WingId)) return false;

            return true;
        }

        // "Who did this" for CancelledBy/FinalizedBy/PostFlightReportedBy.
        // No existing real controller was seen setting these Sortie
        // audit-actor fields anywhere, so there's no established convention
        // to match — this is a reasonable, simple choice (UserManager's
        // UserName, falling back to the claims-principal name), not a
        // confirmed house pattern. Flagging rather than presenting it as
        // settled.
        private async Task<string?> GetActorNameAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.UserName ?? User.Identity?.Name;
        }

        // Populates AcTypes strictly within the Odv's own AcMainGroup —
        // scope only decides WHETHER the user can edit this Sortie at all
        // (already checked by IsOdvInScopeAsync above), not which AcTypes
        // show once they're in.
        private async Task PopulateAcTypesForOdv(int odvAcMainGroupId, int? currentAcTypeId, UserScope scope)
        {
            var acTypes = (await _unitOfWork.AcTypes.GetWhereAsync(t => t.AcMainGroupId == odvAcMainGroupId))
                .OrderBy(t => t.Name)
                .ToList();

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
            var acTypes = (await _unitOfWork.AcTypes.GetWhereAsync(t => t.AcMainGroupId == acMainGroupId))
                .OrderBy(t => t.Name)
                .ToList();

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
