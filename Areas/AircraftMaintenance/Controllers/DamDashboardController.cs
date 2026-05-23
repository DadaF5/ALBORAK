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
            // TODO Phase future — MaintenanceDue module not yet built.
            // DueSoon card renders a "module à venir" notice when list is empty.

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
                    TotalDueSoon = 0,   // TODO: MaintenanceDue phase future
                    TotalRestrictions = activeRestrictions.Count()
                },
                Fleet = fleet,
                DueSoon = [],   // TODO: MaintenanceDue phase future
                Restrictions = restrictionVms
            };

            return View(vm);
        }
    }
}