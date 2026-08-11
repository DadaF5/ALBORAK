using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class WorkSectionsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public WorkSectionsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/WorkSections
        public async Task<IActionResult> Index()
        {
            var items = await _uow.WorkSections.GetAllWithDetailsAsync();

            var vm = items.Select(x => new WorkSectionListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                AcTypeId = x.AcTypeId,
                AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkSections/Create
        public async Task<IActionResult> Create()
        {
            var vm = new WorkSectionFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkSections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkSectionFormViewModel vm)
        {
            if (await _uow.WorkSections.ExistsByCodeAsync(vm.AcTypeId, vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new WorkSection
            {
                AcTypeId = vm.AcTypeId,
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Description = vm.Description,
                SortOrder = (byte)vm.SortOrder,
                IsActive = vm.IsActive
            };

            await _uow.WorkSections.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Section créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/WorkSections/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new WorkSectionFormViewModel
            {
                Id = entity.Id,
                AcTypeId = entity.AcTypeId,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkSections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkSectionFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (await _uow.WorkSections.ExistsByCodeAsync(vm.AcTypeId, vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.AcTypeId = vm.AcTypeId;
            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Description = vm.Description;
            entity.SortOrder = (byte)vm.SortOrder;
            entity.IsActive = vm.IsActive;

            _uow.WorkSections.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Section modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/WorkSections/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var acType = await _uow.AcTypes.GetByIdAsync(entity.AcTypeId);

            var vm = new WorkSectionListItemViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                AcTypeId = entity.AcTypeId,
                AcTypeLabel = acType != null ? $"{acType.Code} — {acType.Name}" : "—",
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkSections/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _uow.WorkSections.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Section supprimée définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Impossible de supprimer — utilisée par des ordres de travail. Désactivez-la plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AircraftMaintenance/WorkSections/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            _uow.WorkSections.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Section réactivée." : "Section désactivée.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(WorkSectionFormViewModel vm)
        {
            var acTypes = await _uow.AcTypes.GetAllAsync();
            vm.AcTypes = acTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.Code)
                .Select(a => new AcTypeLookupViewModel
                {
                    Id = a.Id,
                    Code = a.Code ?? string.Empty,
                    Name = a.Name
                })
                .ToList();
        }
    }
}