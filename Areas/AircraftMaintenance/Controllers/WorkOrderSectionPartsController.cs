using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class WorkOrderSectionPartsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public WorkOrderSectionPartsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/WorkOrderSectionParts/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

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
        public IActionResult Create(int id)
        {
            return View(new WorkOrderSectionPartFormViewModel { WorkOrderSectionId = id });
        }

        // POST: AircraftMaintenance/WorkOrderSectionParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderSectionPartFormViewModel vm)
        {
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
        public async Task<IActionResult> Edit(int id, WorkOrderSectionPartFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var entity = await _uow.WorkOrderSectionParts.GetByIdAsync(id);
            if (entity == null) return NotFound();

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
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrderSectionParts.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var sectionId = entity.WorkOrderSectionId;

            _uow.WorkOrderSectionParts.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Équipement supprimé.";
            return RedirectToAction(nameof(Index), new { id = sectionId });
        }
    }
}