using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class WorkOrderSectionSignOffsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public WorkOrderSectionSignOffsController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkOrderSectionSignOffs/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            var signOffs = await _uow.WorkOrderSectionSignOffs.GetOrCreateCanonicalAsync(id);
            var labelByLevel = FRAProject.Areas.AircraftMaintenance.Models.WorkOrderSectionSignOff.CanonicalLevels
                .ToDictionary(l => l.Level, l => l.Label);

            var vm = new WorkOrderSectionSignOffPageViewModel
            {
                WorkOrderSectionId = id,
                SectionLabel = $"{section.WorkSection?.Code} — {section.WorkSection?.Name}",
                FormNumber = section.FormNumber,
                SignOffs = signOffs.Select(x => new WorkOrderSectionSignOffItemViewModel
                {
                    Id = x.Id,
                    Level = x.Level,
                    LevelLabel = labelByLevel.GetValueOrDefault(x.Level, x.Level),
                    SortOrder = x.SortOrder,
                    SignedByName = x.SignedByName,
                    StampReference = x.StampReference,
                    SignedAtUtc = x.SignedAtUtc,
                    Remarks = x.Remarks
                }).ToList()
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSectionSignOffs/Sign
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Sign(int signOffId, string signedByName, string? stampReference, string? remarks)
        {
            var entity = await _uow.WorkOrderSectionSignOffs.GetByIdAsync(signOffId);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            if (string.IsNullOrWhiteSpace(signedByName))
            {
                TempData["Error"] = "Le nom du signataire est requis.";
                return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
            }

            entity.SignedByName = signedByName.Trim();
            entity.StampReference = stampReference;
            entity.Remarks = remarks;
            entity.SignedAtUtc = DateTime.UtcNow;

            _uow.WorkOrderSectionSignOffs.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Visa enregistré avec succès.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }

        // POST: AircraftMaintenance/WorkOrderSectionSignOffs/Unsign
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Unsign(int signOffId)
        {
            var entity = await _uow.WorkOrderSectionSignOffs.GetByIdAsync(signOffId);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            entity.SignedAtUtc = null;

            _uow.WorkOrderSectionSignOffs.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Visa annulé.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task<bool> IsSectionInScopeAsync(WorkOrderSection section)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;
            if (section.WorkOrder == null) return false; // can't verify — fail closed
            return await IsAircraftInScopeAsync(section.WorkOrder.AircraftId);
        }

        private async Task<bool> IsSectionIdInScopeAsync(int workOrderSectionId)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(workOrderSectionId);
            if (section == null) return false;
            return await IsSectionInScopeAsync(section);
        }

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
    }
}
