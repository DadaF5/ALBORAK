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
    public class WorkOrderSectionPartsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public WorkOrderSectionPartsController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkOrderSectionParts/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            var parts = await _uow.WorkOrderSectionParts.GetByWorkOrderSectionIdAsync(id);

            ViewBag.WorkOrderSectionId = id;
            ViewBag.SectionLabel = $"{section.WorkSection?.Code} — {section.WorkSection?.Name}";
            ViewBag.FormNumber = section.FormNumber;

            var vm = parts.Select(x => new WorkOrderSectionPartListItemViewModel
            {
                Id = x.Id,
                OldNomenclature = x.OldNomenclature,
                OldNumero = x.OldNumero,
                NewNomenclature = x.NewNomenclature,
                NewNumero = x.NewNumero,
                DesignationEtPosition = x.DesignationEtPosition,
                MotifDepose = x.MotifDepose,
                Date = x.Date,
                ExecutantNom = x.ExecutantNom,
                IsSigned = x.ExecutantSignedAtUtc.HasValue
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrderSectionParts/Create/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Create(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            return View(new WorkOrderSectionPartFormViewModel { WorkOrderSectionId = id });
        }

        // POST: AircraftMaintenance/WorkOrderSectionParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(WorkOrderSectionPartFormViewModel vm)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(vm.WorkOrderSectionId);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            if (!ModelState.IsValid) return View(vm);

            var entity = new WorkOrderSectionPart
            {
                WorkOrderSectionId = vm.WorkOrderSectionId,
                OldNomenclature = vm.OldNomenclature,
                OldNumero = vm.OldNumero,
                OldVieillissement = vm.OldVieillissement,
                NewNomenclature = vm.NewNomenclature,
                NewNumero = vm.NewNumero,
                NewVieillissement = vm.NewVieillissement,
                DesignationEtPosition = vm.DesignationEtPosition,
                MotifDepose = vm.MotifDepose,
                Symbole = vm.Symbole,
                TempsAlloueMinutes = vm.TempsAlloueMinutes,
                Date = vm.Date,
                TempsPasseMinutes = vm.TempsPasseMinutes,
                ExecutantSpecial = vm.ExecutantSpecial,
                ExecutantNom = vm.ExecutantNom,
                ExecutantSignedAtUtc = string.IsNullOrEmpty(vm.ExecutantNom) ? null : DateTime.UtcNow,
                SortOrder = vm.SortOrder,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.WorkOrderSectionParts.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Équipement ajouté avec succès.";
            return RedirectToAction(nameof(Index), new { id = vm.WorkOrderSectionId });
        }

        // GET: AircraftMaintenance/WorkOrderSectionParts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.WorkOrderSectionParts.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            var vm = new WorkOrderSectionPartFormViewModel
            {
                Id = entity.Id,
                WorkOrderSectionId = entity.WorkOrderSectionId,
                OldNomenclature = entity.OldNomenclature,
                OldNumero = entity.OldNumero,
                OldVieillissement = entity.OldVieillissement,
                NewNomenclature = entity.NewNomenclature,
                NewNumero = entity.NewNumero,
                NewVieillissement = entity.NewVieillissement,
                DesignationEtPosition = entity.DesignationEtPosition,
                MotifDepose = entity.MotifDepose,
                Symbole = entity.Symbole,
                TempsAlloueMinutes = entity.TempsAlloueMinutes,
                Date = entity.Date,
                TempsPasseMinutes = entity.TempsPasseMinutes,
                ExecutantSpecial = entity.ExecutantSpecial,
                ExecutantNom = entity.ExecutantNom,
                SortOrder = entity.SortOrder
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSectionParts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, WorkOrderSectionPartFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var entity = await _uow.WorkOrderSectionParts.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            if (!ModelState.IsValid) return View(vm);

            var executantChanged = entity.ExecutantNom != vm.ExecutantNom;

            entity.OldNomenclature = vm.OldNomenclature;
            entity.OldNumero = vm.OldNumero;
            entity.OldVieillissement = vm.OldVieillissement;
            entity.NewNomenclature = vm.NewNomenclature;
            entity.NewNumero = vm.NewNumero;
            entity.NewVieillissement = vm.NewVieillissement;
            entity.DesignationEtPosition = vm.DesignationEtPosition;
            entity.MotifDepose = vm.MotifDepose;
            entity.Symbole = vm.Symbole;
            entity.TempsAlloueMinutes = vm.TempsAlloueMinutes;
            entity.Date = vm.Date;
            entity.TempsPasseMinutes = vm.TempsPasseMinutes;
            entity.ExecutantSpecial = vm.ExecutantSpecial;
            entity.ExecutantNom = vm.ExecutantNom;
            if (executantChanged && !string.IsNullOrEmpty(vm.ExecutantNom))
                entity.ExecutantSignedAtUtc = DateTime.UtcNow;
            entity.SortOrder = vm.SortOrder;

            _uow.WorkOrderSectionParts.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Équipement modifié avec succès.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }

        // POST: AircraftMaintenance/WorkOrderSectionParts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrderSectionParts.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            var sectionId = entity.WorkOrderSectionId;

            _uow.WorkOrderSectionParts.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Équipement supprimé.";
            return RedirectToAction(nameof(Index), new { id = sectionId });
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
