using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class DamDashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DamDashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            // ── Aircraft + lookups ────────────────────────────────────────
            var aircraft = await _uow.Aircraft.GetWhereAsync(a => a.IsActive);
            var acTypes = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var statuses = await _uow.AcStatusTypes.GetWhereAsync(s => s.IsActive);

            var typeMap = acTypes.ToDictionary(t => t.Id, t => t.Name);
            var statusMap = statuses.ToDictionary(
                s => s.Id, s => new { s.Code, s.Name });
            var aircraftById = aircraft.ToDictionary(a => a.Id);

            // ── Certificates ──────────────────────────────────────────────
            var certs = await _uow.AircraftCertificates
                .GetWhereAsync(c => c.IsActive);
            var certLookup = certs
                .GroupBy(c => c.AircraftId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── Restrictions ──────────────────────────────────────────────
            var activeRestrictions = await _uow.AircraftRestrictions
                .GetWhereAsync(r => r.IsActive);

            // ── Due items ─────────────────────────────────────────────────
            // Now wired using the same InspectionStatusCalculator logic as
            // the DueList view — counts every Aircraft x InspectionType
            // combination currently OVERDUE or ALERT.
            //
            // NOTE: DueSoon (the list, not just the count) is left as [] —
            // I don't have DamDashboardVm's declared type for that property
            // in this session, and guessing its shape risks a compile
            // error. Send DamDashboardVm.cs (and FleetStatusRowVm/
            // AircraftCertificateVm/RestrictionVm if they're in the same
            // file) and I'll populate the actual list in a quick follow-up.
            var totalDueSoon = await ComputeDueSoonCountAsync(aircraft.Select(a => a.Id).ToHashSet());

            // ── Fleet rows ────────────────────────────────────────────────
            var fleet = aircraft
                .OrderBy(a => a.Registration)
                .Select(a =>
                {
                    var st = statusMap.TryGetValue(a.AcStatusTypeId, out var s)
                             ? s : null;
                    certLookup.TryGetValue(a.Id, out var acCerts);
                    acCerts ??= [];

                    AircraftCertificateVm? MapCert(string type)
                    {
                        var c = acCerts.FirstOrDefault(x => x.CertType == type);
                        return c == null ? null : new AircraftCertificateVm
                        {
                            CertType = c.CertType,
                            Reference = c.Reference,
                            ExpiryDate = c.ExpiryDate
                        };
                    }

                    return new FleetStatusRowVm
                    {
                        AircraftId = a.Id,
                        TailNumber = a.TailNo.ToString(),
                        AcTypeName = typeMap.TryGetValue(a.AcTypeId, out var tn)
                                          ? tn : "—",
                        StatusCode = st?.Code ?? "—",
                        StatusLabel = st?.Name ?? "—",
                        FlightHours = a.TotalFlightMinutes / 60,
                        Cycles = a.TotalCycles,
                        Landings = a.TotalLandings,
                        CdN = MapCert("CdN"),
                        CEN = MapCert("CEN"),
                        PEA = MapCert("PEA")
                    };
                }).ToList();

            // ── Restrictions list ─────────────────────────────────────────
            var restrictionVms = activeRestrictions
                .OrderByDescending(r => r.Severity)
                .Take(10)
                .Select(r =>
                {
                    aircraftById.TryGetValue(r.AircraftId, out var ac);
                    typeMap.TryGetValue(ac?.AcTypeId ?? 0, out var tn);
                    return new RestrictionVm
                    {
                        AircraftCode = ac?.Registration ?? "—",
                        AcTypeName = tn ?? "—",
                        Reference = r.Reference,
                        Description = r.Description,
                        ExpiryDate = r.ExpiryDate,
                        IsCritical = r.Severity == "CRITICAL"
                    };
                }).ToList();

            var vm = new DamDashboardVm
            {
                Kpi = new DamKpiVm
                {
                    TotalAircraft = aircraft.Count(),
                    TotalNavigable = aircraft.Count(a =>
                        statusMap.TryGetValue(a.AcStatusTypeId, out var s) &&
                        s.Code == "OPR"),
                    TotalDueSoon = totalDueSoon,
                    TotalRestrictions = activeRestrictions.Count()
                },
                Fleet = fleet,
                DueSoon = [],   // TODO: populate once DamDashboardVm's DueSoon item type is confirmed
                Restrictions = restrictionVms
            };

            return View(vm);
        }

        // Counts Aircraft x InspectionType combinations currently OVERDUE
        // or ALERT, across the given set of active aircraft. Same
        // calculation as DueListController.Index(), summarized to a count
        // for the dashboard KPI card.
        private async Task<int> ComputeDueSoonCountAsync(HashSet<int> activeAircraftIds)
        {
            var aircrafts = (await _uow.Aircraft.GetAllAsync())
                .Where(a => activeAircraftIds.Contains(a.Id))
                .ToList();

            var inspectionTypes = (await _uow.InspectionTypes.GetAllWithDetailsAsync())
                .Where(it => it.IsActive)
                .ToList();

            var allStates = await _uow.InspectionStates.GetAllWithDetailsAsync();
            var statesByAircraftAndType = allStates
                .ToDictionary(s => (s.AircraftId, s.InspectionTypeId));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var count = 0;

            foreach (var aircraft in aircrafts)
            {
                var currentHours = aircraft.TotalFlightMinutes / 60;
                var currentCycles = aircraft.TotalCycles;

                foreach (var it in inspectionTypes.Where(t => t.AcTypeId == aircraft.AcTypeId))
                {
                    statesByAircraftAndType.TryGetValue((aircraft.Id, it.Id), out var state);

                    var status = InspectionStatusCalculator.ComputeStatus(
                        currentHours, currentCycles, today,
                        state?.NextDueHours, state?.NextDueCycles, state?.NextDueDate, it);

                    if (status == "OVERDUE" || status == "ALERT")
                        count++;
                }
            }

            return count;
        }
    }
}
