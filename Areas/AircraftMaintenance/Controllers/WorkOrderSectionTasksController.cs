using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class WorkOrderSectionTasksController : Controller
    {
        private readonly IUnitOfWork _uow;

        public WorkOrderSectionTasksController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/WorkOrderSectionTasks/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

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
        public IActionResult Create(int id)
        {
            return View(new WorkOrderSectionTaskFormViewModel { WorkOrderSectionId = id });
        }

        // POST: AircraftMaintenance/WorkOrderSectionTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderSectionTaskFormViewModel vm)
        {
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
        public async Task<IActionResult> Edit(int id, WorkOrderSectionTaskFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var entity = await _uow.WorkOrderSectionTasks.GetByIdAsync(id);
            if (entity == null) return NotFound();

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
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrderSectionTasks.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var sectionId = entity.WorkOrderSectionId;

            _uow.WorkOrderSectionTasks.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Travail supprimé.";
            return RedirectToAction(nameof(Index), new { id = sectionId });
        }
    }
}