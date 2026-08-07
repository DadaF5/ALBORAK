using FRAProject.Areas.Settings.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class AtaController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AtaController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: Settings/Ata
        public async Task<IActionResult> Index()
        {
            var items = await _uow.Ata.GetAllWithDetailsAsync();

            var vm = items
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
                .Select(x => new AtaListItemViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    CategoryName = x.AtaCategory?.Name,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                }).ToList();

            return View(vm);
        }

        // GET: Settings/Ata/Create
        public async Task<IActionResult> Create()
        {
            var vm = new AtaFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: Settings/Ata/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AtaFormViewModel vm)
        {
            if (await _uow.Ata.ExistsByCodeAsync(vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code), "Ce code ATA existe déjà.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new Ata
            {
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Description = vm.Description,
                AtaCategoryId = vm.AtaCategoryId,
                SortOrder = (byte)vm.SortOrder,
                IsActive = vm.IsActive
            };

            await _uow.Ata.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Chapitre ATA créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/Ata/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.Ata.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AtaFormViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                AtaCategoryId = entity.AtaCategoryId,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: Settings/Ata/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AtaFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (await _uow.Ata.ExistsByCodeAsync(vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code), "Ce code ATA existe déjà.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = await _uow.Ata.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Description = vm.Description;
            entity.AtaCategoryId = vm.AtaCategoryId;
            entity.SortOrder = (byte)vm.SortOrder;
            entity.IsActive = vm.IsActive;

            _uow.Ata.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Chapitre ATA modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/Ata/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.Ata.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AtaListItemViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            };

            return View(vm);
        }

        // POST: Settings/Ata/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.Ata.GetByIdAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _uow.Ata.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Chapitre ATA supprimé définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Impossible de supprimer — utilisé par des job cards. Désactivez-le plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/Ata/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.Ata.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;

            _uow.Ata.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Chapitre ATA réactivé." : "Chapitre ATA désactivé.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(AtaFormViewModel vm)
        {
            var categories = await _uow.AtaCategories.GetAllAsync();
            vm.Categories = categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Select(c => new AtaCategoryLookupViewModel { Id = c.Id, Code = c.Code, Name = c.Name })
                .ToList();
        }
    }
}