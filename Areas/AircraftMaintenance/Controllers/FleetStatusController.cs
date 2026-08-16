// Areas/AircraftMaintenance/Controllers/FleetStatusController.cs
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class FleetStatusController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public FleetStatusController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var aircraft = await _uow.Aircraft.GetWhereAsync(a => a.IsActive);
            var acTypes = (await _uow.AcTypes.GetAllAsync()).ToDictionary(t => t.Id);
            var statuses = (await _uow.AcStatusTypes.GetAllAsync()).ToDictionary(s => s.Id);

            if (!scope.IsUnrestricted)
            {
                aircraft = aircraft.Where(a =>
                    a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypes.TryGetValue(a.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId))));
            }

            var vm = aircraft
                .OrderBy(a => acTypes.TryGetValue(a.AcTypeId, out var t) ? t.Code : "")
                .ThenBy(a => a.Registration)
                .Select(a =>
                {
                    acTypes.TryGetValue(a.AcTypeId, out var acType);
                    statuses.TryGetValue(a.AcStatusTypeId, out var status);

                    return new FleetStatusRowVm
                    {
                        AircraftId = a.Id,
                        Registration = a.Registration,
                        TailNo = a.TailNo,
                        AcTypeCode = acType?.Code ?? "—",
                        AcTypeName = acType?.Name ?? "—",
                        Serviceable = a.Serviceable,
                        StatusCode = status?.Code ?? "—",
                        StatusName = status?.Name ?? "—"
                    };
                }).ToList();

            return View(vm);
        }
    }
}