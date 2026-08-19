using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class WorkOrdersController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISnagService _snagService;
        private readonly IUserScopeService _userScopeService;

        public WorkOrdersController(
            IUnitOfWork uow, UserManager<ApplicationUser> userManager,
            ISnagService snagService, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userManager = userManager;
            _snagService = snagService;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkOrders
        // WorkOrder is aircraft-instance data — scoped by Base+AcMainGroup,
        // same pattern as Snags/AircraftCertificates/AircraftRestrictions.
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var items = await _uow.WorkOrders.GetAllWithDetailsAsync();

            if (!scope.IsUnrestricted)
            {
                var acTypesById = (await _uow.AcTypes.GetAllAsync()).ToDictionary(t => t.Id);
                items = items.Where(w =>
                    w.Aircraft != null
                    && w.Aircraft.BaseId.HasValue
                    && scope.AllowedBaseIds.Contains(w.Aircraft.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypesById.TryGetValue(w.Aircraft.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId)))).ToList();
            }

            var vm = items.Select(w => new WorkOrderListItemViewModel
            {
                Id = w.Id,
                WONumber = w.WONumber,
                AircraftId = w.AircraftId,
                AircraftLabel = w.Aircraft?.Registration ?? "—",
                InspectionTypeLabels = w.WorkOrderInspectionTypes
                    .Where(x => x.InspectionType != null)
                    .Select(x => x.InspectionType!.Code)
                    .ToList(),
                WOType = w.WOType,
                WOKind = w.WOKind,
                Status = w.Status,
                OpenHours = w.OpenHours,
                OpenCycles = w.OpenCycles,
                OpenDate = w.OpenDate,
                CloseHours = w.CloseHours,
                CloseCycles = w.CloseCycles,
                CloseDate = w.CloseDate
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrders/Create
        public async Task<IActionResult> Create()
        {
            var vm = new WorkOrderFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrders/GetSelectableInspectionTypes?aircraftId=5
        // Called via AJAX when the aircraft dropdown changes on Create.
        // Returns only InspectionTypes that are actually worth scheduling
        // for THIS aircraft right now:
        //   - matches the aircraft's AcType
        //   - NOT already covered by another active (non-CLOSED) WorkOrder
        //     for this aircraft (prevents double-scheduling, e.g. two WOs
        //     both claiming PE4)
        //   - status is OVERDUE, ALERT, or UNKNOWN (never done before) —
        //     "OK" (comfortably not due yet, e.g. PE1/PE2/PE3 right after
        //     a 900h inspection when next due is still far off) is excluded
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetSelectableInspectionTypes(int aircraftId)
        {
            // The dropdown only offers in-scope aircraft, but this is a
            // directly-callable AJAX endpoint — a tampered aircraftId
            // could otherwise be used to probe another base/group's data.
            if (!await IsAircraftInScopeAsync(aircraftId))
                return Forbid();

            var aircraft = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null) return Json(new List<object>());

            var allTypes = await _uow.InspectionTypes.GetAllWithDetailsAsync();
            var relevant = allTypes.Where(it => it.AcTypeId == aircraft.AcTypeId && it.IsActive).ToList();

            var activeTypeIds = await _uow.WorkOrders.GetActiveInspectionTypeIdsForAircraftAsync(aircraftId);

            var allStates = await _uow.InspectionStates.GetAllWithDetailsAsync();
            var statesByType = allStates
                .Where(s => s.AircraftId == aircraftId)
                .ToDictionary(s => s.InspectionTypeId);

            var currentHours = aircraft.TotalFlightMinutes / 60;
            var currentCycles = aircraft.TotalCycles;
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var result = new List<object>();

            foreach (var it in relevant.OrderBy(t => t.SortOrder))
            {
                if (activeTypeIds.Contains(it.Id)) continue; // point 3 — already scheduled

                string status;
                if (statesByType.TryGetValue(it.Id, out var state))
                {
                    status = InspectionStatusCalculator.ComputeStatus(
                        currentHours, currentCycles, currentDate,
                        state.NextDueHours, state.NextDueCycles, state.NextDueDate, it);
                }
                else
                {
                    status = "UNKNOWN"; // never done on this aircraft — worth offering
                }

                if (status == "OK") continue; // point 2 — not due yet, don't clutter the list

                result.Add(new
                {
                    id = it.Id,
                    acTypeId = it.AcTypeId,
                    label = $"{it.Code} — {it.Name} ({status})",
                    status
                });
            }

            return Json(result);
        }

        // POST: AircraftMaintenance/WorkOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(WorkOrderFormViewModel vm)
        {
            // Defense in depth — dropdown only offers in-scope aircraft, but
            // AircraftId is still a posted value and can be tampered with.
            if (!await IsAircraftInScopeAsync(vm.AircraftId))
                return Forbid();

            if (vm.WOKind == "PLANNED" && !vm.SelectedInspectionTypeIds.Any())
            {
                ModelState.AddModelError(nameof(vm.SelectedInspectionTypeIds),
                    "Sélectionnez au moins un type d'inspection pour un OT planifié.");
            }

            // Server-side guard against AcType mismatch — the client-side
            // checkbox filtering can be bypassed (disabled JS, direct POST),
            // so this is the real enforcement point. Without this, nothing
            // stops selecting a C130H aircraft with an F5E InspectionType.
            if (vm.WOKind == "PLANNED" && vm.SelectedInspectionTypeIds.Any())
            {
                var aircraft = await _uow.Aircraft.GetByIdAsync(vm.AircraftId);
                if (aircraft != null)
                {
                    var selectedTypes = await _uow.InspectionTypes.GetAllWithDetailsAsync();
                    var mismatched = selectedTypes
                        .Where(it => vm.SelectedInspectionTypeIds.Contains(it.Id))
                        .Where(it => it.AcTypeId != aircraft.AcTypeId)
                        .ToList();

                    if (mismatched.Any())
                    {
                        var names = string.Join(", ", mismatched.Select(it => it.Code));
                        ModelState.AddModelError(nameof(vm.SelectedInspectionTypeIds),
                            $"Ces types d'inspection ne correspondent pas au type d'aéronef sélectionné : {names}.");
                    }

                    // Prevent scheduling the same InspectionType twice for
                    // the same aircraft while an earlier WorkOrder covering
                    // it is still active (not yet CLOSED). Without this,
                    // two separate WOs could both claim to cover "PE4",
                    // and it becomes ambiguous which one actually updates
                    // InspectionState at close time.
                    var activeTypeIds = await _uow.WorkOrders.GetActiveInspectionTypeIdsForAircraftAsync(vm.AircraftId);
                    var alreadyScheduled = selectedTypes
                        .Where(it => vm.SelectedInspectionTypeIds.Contains(it.Id))
                        .Where(it => activeTypeIds.Contains(it.Id))
                        .ToList();

                    if (alreadyScheduled.Any())
                    {
                        var names = string.Join(", ", alreadyScheduled.Select(it => it.Code));
                        ModelState.AddModelError(nameof(vm.SelectedInspectionTypeIds),
                            $"Ces types d'inspection sont déjà planifiés dans un OT actif pour cet aéronef : {names}.");
                    }
                }
            }

            // These fields are system-managed (set by Open/Close workflow
            // actions), not user input at Create time — ignore whatever
            // came from the form for them.
            ModelState.Remove(nameof(vm.WONumber));
            ModelState.Remove(nameof(vm.Status));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new WorkOrder
            {
                AircraftId = vm.AircraftId,
                WOType = vm.WOType,
                WOKind = vm.WOKind,
                Status = "DRAFT",
                WONumber = "PENDING", // finalized on Open
                OpenDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Remarks = vm.Remarks,
                CreatedAtUtc = DateTime.UtcNow
            };

            if (vm.WOKind == "PLANNED")
            {
                foreach (var itId in vm.SelectedInspectionTypeIds)
                {
                    entity.WorkOrderInspectionTypes.Add(new WorkOrderInspectionType
                    {
                        InspectionTypeId = itId
                    });
                }
            }

            await _uow.WorkOrders.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Ordre de travail créé en brouillon. Ouvrez-le pour lui attribuer un numéro.";
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }

        // GET: AircraftMaintenance/WorkOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            return View(await MapToDetailsVmAsync(entity));
        }

        // GET: AircraftMaintenance/WorkOrders/Print/5
        // Now renders the REAL Formule 12/13 structure — one Formule 13
        // block per WorkOrderSection, with its equipment-exchange table
        // (Tableau II), travaux-effectués table (Tableau III), and the
        // real 4-level visa chain. Matches the actual scanned forms, not
        // a generic placeholder.
        public async Task<IActionResult> Print(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            var vm = await MapToDetailsVmAsync(entity);
            vm.Sections = await BuildSectionsForPrintAsync(id);

            return View(vm);
        }

        // Fetches every WorkOrderSection for this WorkOrder, plus each
        // one's Parts (Tableau II), Tasks (Tableau III), and SignOffs
        // (4-level chain — auto-created if not yet opened, so the print
        // always shows all 4 visa slots, blank or filled, matching the
        // real paper form).
        private async Task<List<WorkOrderSectionPrintViewModel>> BuildSectionsForPrintAsync(int workOrderId)
        {
            var sections = await _uow.WorkOrderSections.GetByWorkOrderIdWithDetailsAsync(workOrderId);
            var result = new List<WorkOrderSectionPrintViewModel>();

            foreach (var s in sections)
            {
                var parts = await _uow.WorkOrderSectionParts.GetByWorkOrderSectionIdAsync(s.Id);
                var tasks = await _uow.WorkOrderSectionTasks.GetByWorkOrderSectionIdAsync(s.Id);
                var signOffs = await _uow.WorkOrderSectionSignOffs.GetOrCreateCanonicalAsync(s.Id);
                var labelByLevel = WorkOrderSectionSignOff.CanonicalLevels
                    .ToDictionary(l => l.Level, l => l.Label);

                result.Add(new WorkOrderSectionPrintViewModel
                {
                    SectionCode = s.WorkSection?.Code ?? "—",
                    SectionName = s.WorkSection?.Name ?? "—",
                    FormNumber = s.FormNumber,
                    OrganismeResponsable = s.OrganismeResponsable,
                    TypeTravail = s.TypeTravail,
                    DateDebut = s.DateDebut,
                    DateFin = s.DateFin,
                    TempsAlloueMinutes = s.TempsAlloueMinutes,
                    TempsPasseSystematiqueMinutes = s.TempsPasseSystematiqueMinutes,
                    TempsPasseRetoucheMinutes = s.TempsPasseRetoucheMinutes,
                    VieillissementHours = s.VieillissementHours,
                    Directives = s.Directives,
                    TechnicalOrderReference = s.TechnicalOrderReference,
                    DirectiveIssuedByName = s.DirectiveIssuedByName,
                    DirectiveIssuedAtUtc = s.DirectiveIssuedAtUtc,
                    Parts = parts.Select(p => new WorkOrderSectionPartPrintItemViewModel
                    {
                        OldNomenclature = p.OldNomenclature,
                        OldNumero = p.OldNumero,
                        OldVieillissement = p.OldVieillissement,
                        NewNomenclature = p.NewNomenclature,
                        NewNumero = p.NewNumero,
                        NewVieillissement = p.NewVieillissement,
                        DesignationEtPosition = p.DesignationEtPosition,
                        MotifDepose = p.MotifDepose,
                        Symbole = p.Symbole,
                        TempsAlloueMinutes = p.TempsAlloueMinutes,
                        Date = p.Date,
                        TempsPasseMinutes = p.TempsPasseMinutes,
                        ExecutantSpecial = p.ExecutantSpecial,
                        ExecutantNom = p.ExecutantNom
                    }).ToList(),
                    Tasks = tasks.Select(t => new WorkOrderSectionTaskPrintItemViewModel
                    {
                        DesignationTravaux = t.DesignationTravaux,
                        TempsAlloueMinutes = t.TempsAlloueMinutes,
                        Date = t.Date,
                        TempsPasseSystemeMinutes = t.TempsPasseSystemeMinutes,
                        TempsPasseRetouchesMinutes = t.TempsPasseRetouchesMinutes,
                        ExecutantSpecial = t.ExecutantSpecial,
                        ExecutantNom = t.ExecutantNom
                    }).ToList(),
                    SignOffs = signOffs.OrderBy(so => so.SortOrder).Select(so => new WorkOrderSectionSignOffItemViewModel
                    {
                        Id = so.Id,
                        Level = so.Level,
                        LevelLabel = labelByLevel.GetValueOrDefault(so.Level, so.Level),
                        SortOrder = so.SortOrder,
                        SignedByName = so.SignedByName,
                        StampReference = so.StampReference,
                        SignedAtUtc = so.SignedAtUtc,
                        Remarks = so.Remarks
                    }).ToList()
                });
            }

            return result;
        }

        // GET: AircraftMaintenance/WorkOrders/Edit/5  (Remarks only)
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            return View(new WorkOrderEditViewModel { Id = entity.Id, WONumber = entity.WONumber, Remarks = entity.Remarks });
        }

        // POST: AircraftMaintenance/WorkOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, WorkOrderEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var entity = await _uow.WorkOrders.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            if (!ModelState.IsValid) return View(vm);

            entity.Remarks = vm.Remarks;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.WorkOrders.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Ordre de travail modifié.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: AircraftMaintenance/WorkOrders/Delete/5  (DRAFT only)
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            if (entity.Status != "DRAFT")
            {
                TempData["Error"] = "Seuls les OT en brouillon peuvent être supprimés.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(await MapToDetailsVmAsync(entity));
        }

        // POST: AircraftMaintenance/WorkOrders/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            if (entity.Status != "DRAFT")
            {
                TempData["Error"] = "Seuls les OT en brouillon peuvent être supprimés.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _uow.WorkOrders.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Ordre de travail supprimé.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AircraftMaintenance/WorkOrders/Open/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Open(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            if (entity.Status != "DRAFT")
            {
                TempData["Error"] = "Cet OT n'est plus en brouillon.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var aircraft = await _uow.Aircraft.GetByIdAsync(entity.AircraftId);
            if (aircraft == null) return NotFound();

            var year = DateTime.UtcNow.Year;
            entity.WONumber = await _uow.WorkOrders.GenerateNextWONumberAsync(year);
            entity.Status = "OPEN";
            entity.OpenDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.OpenHours = aircraft.TotalFlightMinutes / 60;
            entity.OpenCycles = aircraft.TotalCycles;
            entity.OpenLandings = aircraft.TotalLandings;
            entity.OpenedByUserId = _userManager.GetUserId(User);
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.WorkOrders.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = $"OT ouvert sous le numéro {entity.WONumber}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: AircraftMaintenance/WorkOrders/PopulateJobCards/5
        public async Task<IActionResult> PopulateJobCards(int id)
        {
            var workOrder = await _uow.WorkOrders.GetByIdAsync(id);
            if (workOrder == null) return NotFound();

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

            var vm = await BuildPopulateVmAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrders/PopulateJobCards
        // The authoritative job card list is RE-RESOLVED here from the
        // WO's own InspectionTypes (same logic as the GET) rather than
        // trusted from client input — selectedJobCardIds only narrows
        // which of the resolved cards actually get added, so a tampered
        // form can't inject an unrelated JobCardId.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> PopulateJobCards(int workOrderId, List<int> selectedJobCardIds)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(workOrderId);
            if (workOrder == null) return NotFound();

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

            if (!selectedJobCardIds.Any())
            {
                TempData["Error"] = "Sélectionnez au moins une job card.";
                return RedirectToAction(nameof(PopulateJobCards), new { id = workOrderId });
            }

            var resolved = await ResolveJobCardsForWorkOrderAsync(workOrder);
            var existingJobCardIds = workOrder.WorkOrderJobCards.Select(x => x.JobCardId).ToHashSet();

            int added = 0, skipped = 0;

            foreach (var jc in resolved.Where(r => selectedJobCardIds.Contains(r.JobCardId)))
            {
                if (existingJobCardIds.Contains(jc.JobCardId))
                {
                    skipped++;
                    continue;
                }

                await _uow.WorkOrderJobCards.AddAsync(new WorkOrderJobCard
                {
                    WorkOrderId = workOrderId,
                    JobCardId = jc.JobCardId,
                    MaintenanceProgramId = jc.MaintenanceProgramId,
                    SortOrder = jc.SortOrder,
                    IsMandatory = jc.IsMandatory,
                    Status = "PENDING",
                    CreatedAtUtc = DateTime.UtcNow
                });

                existingJobCardIds.Add(jc.JobCardId);
                added++;
            }

            await _uow.CompleteAsync();

            if (workOrder.Status == "OPEN" && added > 0)
            {
                workOrder.Status = "IN_PROGRESS";
                workOrder.UpdatedAtUtc = DateTime.UtcNow;
                _uow.WorkOrders.Update(workOrder);
                await _uow.CompleteAsync();
            }

            TempData["Success"] = skipped > 0
                ? $"{added} job card(s) ajoutée(s). {skipped} déjà présente(s), ignorée(s)."
                : $"{added} job card(s) ajoutée(s) avec succès.";

            return RedirectToAction(nameof(Details), new { id = workOrderId });
        }

        // POST: AircraftMaintenance/WorkOrders/UpdateJobCardStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> UpdateJobCardStatus(int workOrderJobCardId, string status, string? naJustification, string? observations)
        {
            var line = await _uow.WorkOrderJobCards.GetByIdAsync(workOrderJobCardId);
            if (line == null) return NotFound();

            var workOrder = await _uow.WorkOrders.GetByIdAsync(line.WorkOrderId);
            if (workOrder == null) return NotFound();

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

            line.Status = status;
            line.NAJustification = status == "N_A" ? naJustification : null;
            line.Observations = observations;
            line.UpdatedAtUtc = DateTime.UtcNow;

            if (status == "IN_PROGRESS" && line.StartedAtUtc == null)
                line.StartedAtUtc = DateTime.UtcNow;
            if (status == "DONE" || status == "N_A")
                line.CompletedAtUtc = DateTime.UtcNow;

            _uow.WorkOrderJobCards.Update(line);
            await _uow.CompleteAsync();

            return RedirectToAction(nameof(Details), new { id = line.WorkOrderId });
        }

        // POST: AircraftMaintenance/WorkOrders/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Close(int id)
        {
            var entity = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            if (entity.Status != "OPEN" && entity.Status != "IN_PROGRESS")
            {
                TempData["Error"] = "Cet OT ne peut pas être clôturé dans son état actuel.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var mandatoryPending = entity.WorkOrderJobCards
                .Where(x => x.IsMandatory)
                .Any(x => x.Status != "DONE" && x.Status != "N_A");

            if (mandatoryPending)
            {
                TempData["Error"] = "Impossible de clôturer — des job cards obligatoires ne sont pas terminées (DONE ou N/A).";
                return RedirectToAction(nameof(Details), new { id });
            }

            var aircraft = await _uow.Aircraft.GetByIdAsync(entity.AircraftId);
            if (aircraft == null) return NotFound();

            entity.Status = "CLOSED";
            entity.CloseDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.CloseHours = aircraft.TotalFlightMinutes / 60;
            entity.CloseCycles = aircraft.TotalCycles;
            entity.CloseLandings = aircraft.TotalLandings;
            entity.ClosedByUserId = _userManager.GetUserId(User);
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.WorkOrders.Update(entity);
            await _uow.CompleteAsync();

            // ── InspectionState auto-update ─────────────────────────────
            // For each InspectionType this WO satisfies, update (or create)
            // the per-aircraft due-tracking row: last done position, next
            // due position, and a fresh status snapshot.
            foreach (var wit in entity.WorkOrderInspectionTypes)
            {
                if (wit.InspectionType == null) continue;

                var state = await _uow.InspectionStates.GetByAircraftAndTypeAsync(entity.AircraftId, wit.InspectionTypeId);
                var isNew = state == null;
                state ??= new InspectionState
                {
                    AircraftId = entity.AircraftId,
                    InspectionTypeId = wit.InspectionTypeId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                state.LastDoneHours = entity.CloseHours;
                state.LastDoneCycles = entity.CloseCycles;
                state.LastDoneDate = entity.CloseDate;
                state.LastWorkOrderId = entity.Id;

                var (nextHours, nextCycles, nextDate) = InspectionStatusCalculator.ComputeNextDue(
                    entity.CloseHours, entity.CloseCycles, entity.CloseDate, wit.InspectionType);

                state.NextDueHours = nextHours;
                state.NextDueCycles = nextCycles;
                state.NextDueDate = nextDate;

                state.StatusSnapshot = InspectionStatusCalculator.ComputeStatus(
                    entity.CloseHours, entity.CloseCycles, entity.CloseDate,
                    nextHours, nextCycles, nextDate, wit.InspectionType);

                state.UpdatedAtUtc = DateTime.UtcNow;

                if (isNew)
                    await _uow.InspectionStates.AddAsync(state);
                else
                    _uow.InspectionStates.Update(state);
            }

            await _uow.CompleteAsync();

            // ── Auto-close linked Snags (corrective WOs only) ───────────
            // A corrective WO that closes is assumed to have resolved the
            // defect(s) it was opened for. Every Snag linked via
            // WorkOrderSnag gets closed and flagged ResolvedOnClose.
            if (entity.WOKind == "CORRECTIVE")
            {
                await _snagService.CloseLinkedSnagsAsync(entity.Id, entity.ClosedByUserId!);
            }

            TempData["Success"] = "Ordre de travail clôturé avec succès.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Helpers ──────────────────────────────────────────────────────

        // Mirrors the Base+AcMainGroup check used inline in SnagsController,
        // but for a single already-known AircraftId (form posts, entity
        // lookups) rather than a list being built for a dropdown.
        private async Task<bool> IsAircraftInScopeAsync(int aircraftId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;

            var aircraft = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null || !aircraft.BaseId.HasValue ||
                !scope.AllowedBaseIds.Contains(aircraft.BaseId.Value))
                return false;

            if (!scope.AllowedAcMainGroupIds.Any())
                return true;

            var acType = await _uow.AcTypes.GetByIdAsync(aircraft.AcTypeId);
            return acType != null &&
                   scope.AllowedAcMainGroupIds.Contains(acType.AcMainGroupId);
        }

        private async Task<WorkOrderDetailsViewModel> MapToDetailsVmAsync(WorkOrder w)
        {
            string? openedByName = null;
            string? closedByName = null;

            if (!string.IsNullOrEmpty(w.OpenedByUserId))
            {
                var openedBy = await _userManager.FindByIdAsync(w.OpenedByUserId);
                openedByName = openedBy?.FullLabel ?? openedBy?.UserName;
            }
            if (!string.IsNullOrEmpty(w.ClosedByUserId))
            {
                var closedBy = await _userManager.FindByIdAsync(w.ClosedByUserId);
                closedByName = closedBy?.FullLabel ?? closedBy?.UserName;
            }

            return new WorkOrderDetailsViewModel
            {
                Id = w.Id,
                WONumber = w.WONumber,
                AircraftId = w.AircraftId,
                AircraftLabel = w.Aircraft?.Registration ?? "—",
                AircraftSerialNumber = w.Aircraft?.SerialNumber,
                AircraftIntCode = w.Aircraft?.IntCode,
                AircraftTailNo = w.Aircraft?.TailNo ?? 0,
                AcTypeId = w.Aircraft?.AcTypeId ?? 0,
                AcTypeLabel = w.Aircraft?.AcType != null
                    ? $"{w.Aircraft.AcType.Code} — {w.Aircraft.AcType.Name}"
                    : "—",
                ManufacturerLabel = w.Aircraft?.AcType?.AircraftManufacturer?.Name ?? w.Aircraft?.Manufacturer,
                MaxEngines = w.Aircraft?.AcType?.MaxEngines ?? 0,
                InspectionTypeLabels = w.WorkOrderInspectionTypes
                    .Where(x => x.InspectionType != null)
                    .Select(x => $"{x.InspectionType!.Code} — {x.InspectionType.Name}")
                    .ToList(),
                WOType = w.WOType,
                WOKind = w.WOKind,
                Status = w.Status,
                OpenHours = w.OpenHours,
                OpenCycles = w.OpenCycles,
                OpenLandings = w.OpenLandings,
                OpenDate = w.OpenDate,
                CloseHours = w.CloseHours,
                CloseCycles = w.CloseCycles,
                CloseLandings = w.CloseLandings,
                CloseDate = w.CloseDate,
                OpenedByUserName = openedByName,
                ClosedByUserName = closedByName,
                Remarks = w.Remarks,
                CreatedAtUtc = w.CreatedAtUtc,
                UpdatedAtUtc = w.UpdatedAtUtc,
                JobCards = w.WorkOrderJobCards
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new WorkOrderJobCardItemViewModel
                    {
                        Id = x.Id,
                        JobCardId = x.JobCardId,
                        JobCardLabel = x.JobCard != null ? $"{x.JobCard.CardCode} — {x.JobCard.Title}" : "—",
                        MaintenanceProgramId = x.MaintenanceProgramId,
                        MaintenanceProgramLabel = x.MaintenanceProgram?.Code ?? "—",
                        SortOrder = x.SortOrder,
                        IsMandatory = x.IsMandatory,
                        Status = x.Status,
                        NAJustification = x.NAJustification,
                        Observations = x.Observations,
                        StartedAtUtc = x.StartedAtUtc,
                        CompletedAtUtc = x.CompletedAtUtc,
                        SignOffs = [] // sign-off management not built yet
                    }).ToList()
            };
        }

        private async Task PopulateDropdownsAsync(WorkOrderFormViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var aircrafts = await _uow.Aircraft.GetAllAsync();
            var acTypes = await _uow.AcTypes.GetAllAsync();
            var acTypeCodeById = acTypes.ToDictionary(t => t.Id, t => t.Code ?? "—");
            var acTypesById = acTypes.ToDictionary(t => t.Id);

            var visibleAircrafts = aircrafts.Where(a => a.IsActive);
            if (!scope.IsUnrestricted)
            {
                visibleAircrafts = visibleAircrafts.Where(a =>
                    a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypesById.TryGetValue(a.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId))));
            }

            vm.Aircrafts = visibleAircrafts
                .OrderBy(a => acTypeCodeById.GetValueOrDefault(a.AcTypeId, "—"))
                .ThenBy(a => a.Registration)
                .Select(a => new AircraftLookupViewModel
                {
                    Id = a.Id,
                    Registration = a.Registration,
                    AcTypeCode = acTypeCodeById.GetValueOrDefault(a.AcTypeId, "—"),
                    AcTypeId = a.AcTypeId,
                    DisplayName = $"{acTypeCodeById.GetValueOrDefault(a.AcTypeId, "—")} — {a.Registration}"
                }).ToList();

            // InspectionType checklist is scoped by the same AcMainGroup
            // restriction — a scoped user shouldn't see, e.g., F16
            // InspectionTypes in the checkbox list even before picking an
            // aircraft, since GetSelectableInspectionTypes further narrows
            // by the chosen aircraft's exact AcType anyway.
            var inspectionTypes = await _uow.InspectionTypes.GetAllWithDetailsAsync();
            var visibleInspectionTypes = inspectionTypes.Where(it => it.IsActive);
            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                visibleInspectionTypes = visibleInspectionTypes.Where(it =>
                    it.AcType != null && scope.AllowedAcMainGroupIds.Contains(it.AcType.AcMainGroupId));
            }

            vm.InspectionTypes = visibleInspectionTypes
                .Select(it => new LookupOptionViewModel
                {
                    Id = it.Id,
                    Label = $"{it.AcType?.Code ?? "—"} — {it.Code} — {it.Name}"
                }).ToList();

            // Used by the checkbox list + AcType filtering JS in Create.cshtml
            vm.InspectionTypeItems = visibleInspectionTypes
                .OrderBy(it => it.AcType?.Code)
                .ThenBy(it => it.SortOrder)
                .Select(it => new InspectionTypeCheckItemViewModel
                {
                    Id = it.Id,
                    AcTypeId = it.AcTypeId,
                    Label = $"{it.Code} — {it.Name} ({it.IntervalHours}h)"
                }).ToList();
        }

        private async Task<PopulateJobCardsViewModel?> BuildPopulateVmAsync(int workOrderId)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(workOrderId);
            if (workOrder == null) return null;

            var resolved = await ResolveJobCardsForWorkOrderAsync(workOrder);
            var alreadyAssignedIds = workOrder.WorkOrderJobCards.Select(x => x.JobCardId).ToHashSet();

            return new PopulateJobCardsViewModel
            {
                WorkOrderId = workOrder.Id,
                WONumber = workOrder.WONumber,
                AircraftLabel = workOrder.Aircraft?.Registration ?? "—",
                AvailableJobCards = resolved
                    .Where(jc => !alreadyAssignedIds.Contains(jc.JobCardId))
                    .OrderBy(jc => jc.ProgramLabel).ThenBy(jc => jc.CardCode)
                    .Select(jc => new JobCardSelectItemViewModel
                    {
                        JobCardId = jc.JobCardId,
                        CardCode = jc.CardCode,
                        Title = jc.Title,
                        ProgramLabel = jc.ProgramLabel,
                        IsMandatory = jc.IsMandatory
                    }).ToList()
            };
        }

        // Resolves the full chain: WorkOrder.WorkOrderInspectionTypes ->
        // InspectionTypeProgram -> MaintenanceProgram -> ProgramJobCard ->
        // JobCard. This is the concrete implementation of "for a 1200h WO
        // covering PE1+PE2+PE4, list every job card from all three
        // programs" — union across all linked programs, deduplicated by
        // JobCardId (a card assigned to two of the WO's programs only
        // appears once).
        private async Task<List<(int JobCardId, string CardCode, string Title, int MaintenanceProgramId, string ProgramLabel, bool IsMandatory, int SortOrder)>>
            ResolveJobCardsForWorkOrderAsync(WorkOrder workOrder)
        {
            var inspectionTypeIds = workOrder.WorkOrderInspectionTypes
                .Select(wit => wit.InspectionTypeId)
                .ToList();

            if (!inspectionTypeIds.Any())
                return [];

            var links = await _uow.InspectionTypePrograms.GetByInspectionTypeIdsAsync(inspectionTypeIds);
            var programIds = links.Select(l => l.MaintenanceProgramId).Distinct().ToList();

            var results = new List<(int, string, string, int, string, bool, int)>();
            var seenJobCardIds = new HashSet<int>();

            foreach (var programId in programIds)
            {
                var program = links.First(l => l.MaintenanceProgramId == programId).MaintenanceProgram;
                var programLabel = program?.Code ?? "—";

                var programCards = await _uow.ProgramJobCards.GetByProgramIdWithDetailsAsync(programId);

                foreach (var pjc in programCards)
                {
                    if (!seenJobCardIds.Add(pjc.JobCardId)) continue; // dedupe across programs

                    results.Add((
                        pjc.JobCardId,
                        pjc.JobCard?.CardCode ?? "—",
                        pjc.JobCard?.Title ?? "—",
                        programId,
                        programLabel,
                        pjc.IsMandatory,
                        pjc.SortOrder
                    ));
                }
            }

            return results;
        }
    }
}
