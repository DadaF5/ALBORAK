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
    public class WorkOrderSectionTasksController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public WorkOrderSectionTasksController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkOrderSectionTasks/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            var tasks = await _uow.WorkOrderSectionTasks.GetByWorkOrderSectionIdAsync(id);

            ViewBag.WorkOrderSectionId = id;
            ViewBag.SectionLabel = $"{section.WorkSection?.Code} — {section.WorkSection?.Name}";
            ViewBag.FormNumber = section.FormNumber;

            var vm = tasks.Select(x => new WorkOrderSectionTaskListItemViewModel
            {
                Id = x.Id,
                DesignationTravaux = x.DesignationTravaux,
                TempsAlloueMinutes = x.TempsAlloueMinutes,
                Date = x.Date,
                TempsPasseSystemeMinutes = x.TempsPasseSystemeMinutes,
                TempsPasseRetouchesMinutes = x.TempsPasseRetouchesMinutes,
                ExecutantNom = x.ExecutantNom,
                IsSigned = x.ExecutantSignedAtUtc.HasValue
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrderSectionTasks/Create/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Create(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            return View(new WorkOrderSectionTaskFormViewModel { WorkOrderSectionId = id });
        }

        // POST: AircraftMaintenance/WorkOrderSectionTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(WorkOrderSectionTaskFormViewModel vm)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(vm.WorkOrderSectionId);
            if (section == null) return NotFound();

            if (!await IsSectionInScopeAsync(section))
                return Forbid();

            if (!ModelState.IsValid) return View(vm);

            var entity = new WorkOrderSectionTask
            {
                WorkOrderSectionId = vm.WorkOrderSectionId,
                DesignationTravaux = vm.DesignationTravaux.Trim(),
                TempsAlloueMinutes = vm.TempsAlloueMinutes,
                Date = vm.Date,
                TempsPasseSystemeMinutes = vm.TempsPasseSystemeMinutes,
                TempsPasseRetouchesMinutes = vm.TempsPasseRetouchesMinutes,
                ExecutantSpecial = vm.ExecutantSpecial,
                ExecutantNom = vm.ExecutantNom,
                ExecutantSignedAtUtc = string.IsNullOrEmpty(vm.ExecutantNom) ? null : DateTime.UtcNow,
                SortOrder = vm.SortOrder,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.WorkOrderSectionTasks.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Travail ajouté avec succès.";
            return RedirectToAction(nameof(Index), new { id = vm.WorkOrderSectionId });
        }

        // GET: AircraftMaintenance/WorkOrderSectionTasks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.WorkOrderSectionTasks.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            var vm = new WorkOrderSectionTaskFormViewModel
            {
                Id = entity.Id,
                WorkOrderSectionId = entity.WorkOrderSectionId,
                DesignationTravaux = entity.DesignationTravaux,
                TempsAlloueMinutes = entity.TempsAlloueMinutes,
                Date = entity.Date,
                TempsPasseSystemeMinutes = entity.TempsPasseSystemeMinutes,
                TempsPasseRetouchesMinutes = entity.TempsPasseRetouchesMinutes,
                ExecutantSpecial = entity.ExecutantSpecial,
                ExecutantNom = entity.ExecutantNom,
                SortOrder = entity.SortOrder
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSectionTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, WorkOrderSectionTaskFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var entity = await _uow.WorkOrderSectionTasks.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            if (!ModelState.IsValid) return View(vm);

            var executantChanged = entity.ExecutantNom != vm.ExecutantNom;

            entity.DesignationTravaux = vm.DesignationTravaux.Trim();
            entity.TempsAlloueMinutes = vm.TempsAlloueMinutes;
            entity.Date = vm.Date;
            entity.TempsPasseSystemeMinutes = vm.TempsPasseSystemeMinutes;
            entity.TempsPasseRetouchesMinutes = vm.TempsPasseRetouchesMinutes;
            entity.ExecutantSpecial = vm.ExecutantSpecial;
            entity.ExecutantNom = vm.ExecutantNom;
            if (executantChanged && !string.IsNullOrEmpty(vm.ExecutantNom))
                entity.ExecutantSignedAtUtc = DateTime.UtcNow;
            entity.SortOrder = vm.SortOrder;

            _uow.WorkOrderSectionTasks.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Travail modifié avec succès.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }

        // POST: AircraftMaintenance/WorkOrderSectionTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrderSectionTasks.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionIdInScopeAsync(entity.WorkOrderSectionId))
                return Forbid();

            var sectionId = entity.WorkOrderSectionId;

            _uow.WorkOrderSectionTasks.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Travail supprimé.";
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
