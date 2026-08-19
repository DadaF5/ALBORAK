using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class WorkSectionsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public WorkSectionsController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkSections
        // AcMainGroup-level setup data (real aircraft family: F16/F5/C130/
        // AJET) — scoped by AcMainGroup, same as JobCards/MaintenancePrograms/
        // InspectionTypes. Now a direct check on WorkSection.AcMainGroupId,
        // no AcType indirection needed (see WorkSection.cs for why this
        // moved off AcTypeId).
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var items = await _uow.WorkSections.GetAllWithDetailsAsync();

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                items = items.Where(x =>
                    scope.AllowedAcMainGroupIds.Contains(x.AcMainGroupId)).ToList();
            }

            var vm = items.Select(x => new WorkSectionListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                AcMainGroupId = x.AcMainGroupId,
                AcMainGroupLabel = x.AcMainGroup != null ? $"{x.AcMainGroup.Code} — {x.AcMainGroup.Name}" : "—",
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            // Full AcMainGroup list (not just ones with WorkSection rows) —
            // same fix applied to InspectionTypes/WorkSections earlier, so
            // the filter shows every valid family, not just ones with
            // existing data. Scoped the same way as the item list above.
            var allAcMainGroups = await _uow.AcMainGroups.GetAllAsync();
            var visibleAcMainGroups = allAcMainGroups.Where(g => g.IsActive);
            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                visibleAcMainGroups = visibleAcMainGroups.Where(g => scope.AllowedAcMainGroupIds.Contains(g.Id));
            }
            ViewBag.AllAcMainGroupLabels = visibleAcMainGroups
                .OrderBy(g => g.Code)
                .Select(g => $"{g.Code} — {g.Name}")
                .ToList();

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
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(WorkSectionFormViewModel vm)
        {
            // Defense in depth — dropdown only offers in-scope AcMainGroups,
            // but AcMainGroupId is still a posted value and can be tampered with.
            if (!IsAcMainGroupInScope(vm.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

            if (await _uow.WorkSections.ExistsByCodeAsync(vm.AcMainGroupId, vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour cette famille d'aéronefs.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new WorkSection
            {
                AcMainGroupId = vm.AcMainGroupId,
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

            if (!IsAcMainGroupInScope(entity.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

            var vm = new WorkSectionFormViewModel
            {
                Id = entity.Id,
                AcMainGroupId = entity.AcMainGroupId,
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
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, WorkSectionFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!IsAcMainGroupInScope(vm.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

            if (await _uow.WorkSections.ExistsByCodeAsync(vm.AcMainGroupId, vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour cette famille d'aéronefs.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.AcMainGroupId = vm.AcMainGroupId;
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

            if (!IsAcMainGroupInScope(entity.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

            var acMainGroup = await _uow.AcMainGroups.GetByIdAsync(entity.AcMainGroupId);

            var vm = new WorkSectionListItemViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                AcMainGroupId = entity.AcMainGroupId,
                AcMainGroupLabel = acMainGroup != null ? $"{acMainGroup.Code} — {acMainGroup.Name}" : "—",
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkSections/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!IsAcMainGroupInScope(entity.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

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
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.WorkSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!IsAcMainGroupInScope(entity.AcMainGroupId, await _userScopeService.GetScopeAsync(User, ModuleCode)))
                return Forbid();

            entity.IsActive = !entity.IsActive;
            _uow.WorkSections.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Section réactivée." : "Section désactivée.";
            return RedirectToAction(nameof(Index));
        }

        // Now a direct, synchronous check on scope.AllowedAcMainGroupIds —
        // no DB round trip needed, since WorkSection carries AcMainGroupId
        // itself rather than requiring an AcType -> AcMainGroup lookup.
        private static bool IsAcMainGroupInScope(int acMainGroupId, UserScope scope)
        {
            return scope.IsUnrestricted
                || !scope.AllowedAcMainGroupIds.Any()
                || scope.AllowedAcMainGroupIds.Contains(acMainGroupId);
        }

        private async Task PopulateDropdownsAsync(WorkSectionFormViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var acMainGroups = await _uow.AcMainGroups.GetAllAsync();
            var visibleAcMainGroups = acMainGroups.Where(g => g.IsActive);

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                visibleAcMainGroups = visibleAcMainGroups.Where(g => scope.AllowedAcMainGroupIds.Contains(g.Id));
            }

            vm.AcMainGroups = visibleAcMainGroups
                .OrderBy(g => g.Code)
                .Select(g => new AcMainGroupLookupViewModel
                {
                    Id = g.Id,
                    Code = g.Code ?? string.Empty,
                    Name = g.Name
                })
                .ToList();
        }
    }
}
