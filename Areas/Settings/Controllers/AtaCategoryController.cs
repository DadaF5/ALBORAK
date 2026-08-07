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
    public class AtaCategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AtaCategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _uow.AtaCategories.GetAllAsync();
            var vm = items
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
                .Select(x => new AtaCategoryListItemViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                }).ToList();

            return View(vm);
        }

        public IActionResult Create() => View(new AtaCategoryFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AtaCategoryFormViewModel vm)
        {
            if (await _uow.AtaCategories.ExistsByCodeAsync(vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code), "Ce code existe déjà.");
            }

            if (!ModelState.IsValid) return View(vm);

            var entity = new AtaCategory
            {
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Description = vm.Description,
                SortOrder = (byte)vm.SortOrder,
                IsActive = vm.IsActive
            };

            await _uow.AtaCategories.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Catégorie ATA créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.AtaCategories.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AtaCategoryFormViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AtaCategoryFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (await _uow.AtaCategories.ExistsByCodeAsync(vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code), "Ce code existe déjà.");
            }

            if (!ModelState.IsValid) return View(vm);

            var entity = await _uow.AtaCategories.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Description = vm.Description;
            entity.SortOrder = (byte)vm.SortOrder;
            entity.IsActive = vm.IsActive;

            _uow.AtaCategories.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Catégorie ATA modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AtaCategories.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var vm = new AtaCategoryListItemViewModel
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

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.AtaCategories.GetByIdAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _uow.AtaCategories.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Catégorie ATA supprimée définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Impossible de supprimer — utilisée par des chapitres ATA. Désactivez-la plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.AtaCategories.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            _uow.AtaCategories.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Catégorie réactivée." : "Catégorie désactivée.";
            return RedirectToAction(nameof(Index));
        }
    }
}