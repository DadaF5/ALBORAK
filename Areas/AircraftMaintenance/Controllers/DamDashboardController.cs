using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using FRAProject.Areas.Settings.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class DamDashboardController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public DamDashboardController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // ── Aircraft + lookups ────────────────────────────────────────
            var aircraft = await _uow.Aircraft.GetWhereAsync(a => a.IsActive);

            // BGNT: base-scoped, NOT AcMainGroup-restricted — a BGNT
            // supervises every aircraft type at their base, unlike a
            // TECHNICIAN role. Only AllowedBaseIds applies here.
            if (!scope.IsUnrestricted)
            {
                aircraft = aircraft.Where(a => a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value));
            }

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
                .Where(c => aircraftById.ContainsKey(c.AircraftId)) // scope-consistent
                .GroupBy(c => c.AircraftId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── Restrictions ──────────────────────────────────────────────
            // Filtered to the same scoped aircraft set as everything else,
            // for consistency — a BGNT sees restrictions for their base only,
            // not the whole fleet. Revisit if situational fleet-wide
            // awareness turns out to matter more than base consistency.
            var activeRestrictions = (await _uow.AircraftRestrictions
                .GetWhereAsync(r => r.IsActive))
                .Where(r => aircraftById.ContainsKey(r.AircraftId))
                .ToList();

            // ── Due items — list AND count computed together, single pass,
            //    so they can never drift apart ──────────────────────────
            var dueSoon = await ComputeDueSoonListAsync(aircraft.Select(a => a.Id).ToHashSet(), typeMap, aircraftById);

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
                        TotalFlightMinutes = a.TotalFlightMinutes / 60,
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
                    TotalDueSoon = dueSoon.Count,       // derived from the same list — can't drift
                    TotalRestrictions = activeRestrictions.Count
                },
                Fleet = fleet,
                DueSoon = dueSoon,
                Restrictions = restrictionVms
            };

            return View(vm);
        }

        // Builds the actual Échéances Proches list — every Aircraft x
        // InspectionType combination currently OVERDUE or ALERT, across
        // the given (already scope-filtered) set of aircraft. Replaces
        // the old count-only ComputeDueSoonCountAsync — the count is now
        // just this list's length, computed once, so the KPI card and the
        // detail list can never disagree.
        private async Task<List<DueSoonVm>> ComputeDueSoonListAsync(
            HashSet<int> activeAircraftIds,
            Dictionary<int, string> typeMap,
            Dictionary<int, Aircraft> aircraftById)   // was Dictionary<int, Models.Aircraft>
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
            var result = new List<DueSoonVm>();

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

                    if (status != "OVERDUE" && status != "ALERT") continue;

                    result.Add(new DueSoonVm
                    {
                        AircraftCode = aircraft.Registration,
                        AcTypeName = typeMap.TryGetValue(aircraft.AcTypeId, out var tn) ? tn : "—",
                        TaskName = $"{it.Code} — {it.Name}",
                        DueFh = state?.NextDueHours,
                        CurrentFh = currentHours,
                        DueDate = state?.NextDueDate
                    });
                }
            }

            // Worst-first: overdue (>=95% compliance per DueSoonVm's own
            // AlertClass logic) before merely "approaching" items
            return result.OrderByDescending(d => d.CompliancePct).ToList();
        }
    }
}