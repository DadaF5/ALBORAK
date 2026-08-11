using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class DueListController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DueListController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/DueList
        public async Task<IActionResult> Index()
        {
            var aircrafts = (await _uow.Aircraft.GetAllAsync())
                .Where(a => a.IsActive)
                .ToList();

            var acTypes = await _uow.AcTypes.GetAllAsync();
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

            ViewBag.AllAcTypeLabels = acTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.Code)
                .Select(t => $"{t.Code} — {t.Name}")
                .ToList();

            return View(vm);
        }
    }
}