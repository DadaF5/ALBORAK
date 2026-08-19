using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class DueListController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public DueListController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/DueList
        // Read-only report — no write actions, so the controller-level
        // MaintenanceRead policy is the only auth check needed here. Scope
        // is applied to the underlying aircraft list, same Base+AcMainGroup
        // pattern as Snags/AircraftCertificates/AircraftRestrictions.
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var acTypes = await _uow.AcTypes.GetAllAsync();
            var acTypesById = acTypes.ToDictionary(t => t.Id);

            var aircrafts = (await _uow.Aircraft.GetAllAsync())
                .Where(a => a.IsActive)
                .ToList();

            if (!scope.IsUnrestricted)
            {
                aircrafts = aircrafts.Where(a =>
                    a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypesById.TryGetValue(a.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId))))
                    .ToList();
            }

            var acTypeLabelById = acTypes.ToDictionary(t => t.Id, t => $"{t.Code} — {t.Name}");

            var inspectionTypes = (await _uow.InspectionTypes.GetAllWithDetailsAsync())
                .Where(it => it.IsActive)
                .ToList();

            var allStates = await _uow.InspectionStates.GetAllWithDetailsAsync();
            var statesByAircraftAndType = allStates
                .ToDictionary(s => (s.AircraftId, s.InspectionTypeId));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = new List<DueListItemViewModel>();

            foreach (var aircraft in aircrafts)
            {
                var currentHours = aircraft.TotalFlightMinutes / 60;
                var currentCycles = aircraft.TotalCycles;

                var relevantTypes = inspectionTypes.Where(it => it.AcTypeId == aircraft.AcTypeId);

                foreach (var it in relevantTypes)
                {
                    statesByAircraftAndType.TryGetValue((aircraft.Id, it.Id), out var state);

                    var nextDueHours = state?.NextDueHours;
                    var nextDueCycles = state?.NextDueCycles;
                    var nextDueDate = state?.NextDueDate;

                    var status = InspectionStatusCalculator.ComputeStatus(
                        currentHours, currentCycles, today,
                        nextDueHours, nextDueCycles, nextDueDate, it);

                    result.Add(new DueListItemViewModel
                    {
                        AircraftId = aircraft.Id,
                        AircraftLabel = aircraft.Registration,
                        AcTypeLabel = acTypeLabelById.GetValueOrDefault(aircraft.AcTypeId, "—"),
                        InspectionTypeId = it.Id,
                        InspectionTypeCode = it.Code,
                        InspectionTypeName = it.Name,
                        CurrentHours = currentHours,
                        CurrentCycles = currentCycles,
                        LastDoneHours = state?.LastDoneHours,
                        LastDoneDate = state?.LastDoneDate,
                        NextDueHours = nextDueHours,
                        NextDueCycles = nextDueCycles,
                        NextDueDate = nextDueDate,
                        RemainingHours = nextDueHours.HasValue ? nextDueHours.Value - currentHours : null,
                        Status = status
                    });
                }
            }

            var vm = result
                .OrderBy(x => x.StatusSeverity)
                .ThenBy(x => x.AircraftLabel)
                .ThenBy(x => x.InspectionTypeCode)
                .ToList();

            // Filter dropdown — scoped the same way as the aircraft list
            // above, so a scoped user isn't offered AcTypes they can't see
            // any due-list rows for.
            var visibleAcTypes = acTypes.Where(t => t.IsActive);
            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                visibleAcTypes = visibleAcTypes.Where(t => scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId));
            }
            ViewBag.AllAcTypeLabels = visibleAcTypes
                .OrderBy(t => t.Code)
                .Select(t => $"{t.Code} — {t.Name}")
                .ToList();

            return View(vm);
        }
    }
}
