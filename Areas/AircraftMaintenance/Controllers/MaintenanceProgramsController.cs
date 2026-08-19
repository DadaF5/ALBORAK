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
    public class MaintenanceProgramsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public MaintenanceProgramsController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/MaintenancePrograms
        // NOTE: MaintenanceProgram is AcType-level setup data (no Aircraft/Base
        // of its own), so scoping here is by AcMainGroup only — unlike
        // AircraftCertificates/AircraftRestrictions/Snags, there's no Base
        // dimension to filter on for this entity.
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var items = await _uow.MaintenancePrograms.GetAllWithDetailsAsync();

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                items = items.Where(x =>
                    x.AcType != null &&
                    scope.AllowedAcMainGroupIds.Contains(x.AcType.AcMainGroupId)).ToList();
            }

            var vm = items.Select(x => new MaintenanceProgramListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                AcTypeId = x.AcTypeId,
                AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
                DocReference = x.DocReference,
                Edition = x.Edition,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/MaintenancePrograms/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.MaintenancePrograms.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // GET: AircraftMaintenance/MaintenancePrograms/Create
        public async Task<IActionResult> Create()
        {
            var vm = new MaintenanceProgramFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/MaintenancePrograms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(MaintenanceProgramFormViewModel vm)
        {
            // Defense in depth — dropdown only offers in-scope AcTypes, but
            // AcTypeId is still a posted value and can be tampered with.
            if (!await IsAcTypeInScopeAsync(vm.AcTypeId))
                return Forbid();

            if (await _uow.MaintenancePrograms.ExistsByCodeAsync(vm.AcTypeId, vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new MaintenanceProgram
            {
                AcTypeId = vm.AcTypeId,
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Description = vm.Description,
                DocReference = vm.DocReference,
                Edition = vm.Edition,
                ChangeNo = vm.ChangeNo,
                ChangeDate = vm.ChangeDate,
                SortOrder = (byte)vm.SortOrder,
                IsActive = vm.IsActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.MaintenancePrograms.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Programme d'entretien créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/MaintenancePrograms/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.MaintenancePrograms.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = new MaintenanceProgramFormViewModel
            {
                Id = entity.Id,
                AcTypeId = entity.AcTypeId,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                DocReference = entity.DocReference,
                Edition = entity.Edition,
                ChangeNo = entity.ChangeNo,
                ChangeDate = entity.ChangeDate,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/MaintenancePrograms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, MaintenanceProgramFormViewModel vm)
        {
            if (id != vm.Id)
            {
                return BadRequest();
            }

            if (!await IsAcTypeInScopeAsync(vm.AcTypeId))
                return Forbid();

            if (await _uow.MaintenancePrograms.ExistsByCodeAsync(vm.AcTypeId, vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = await _uow.MaintenancePrograms.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.AcTypeId = vm.AcTypeId;
            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Description = vm.Description;
            entity.DocReference = vm.DocReference;
            entity.Edition = vm.Edition;
            entity.ChangeNo = vm.ChangeNo;
            entity.ChangeDate = vm.ChangeDate;
            entity.SortOrder = (byte)vm.SortOrder;
            entity.IsActive = vm.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.MaintenancePrograms.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Programme d'entretien modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/MaintenancePrograms/Delete/5
        // Confirmation page — offers Deactivate (soft, recommended) or Delete (hard).
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.MaintenancePrograms.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // POST: AircraftMaintenance/MaintenancePrograms/DeleteConfirmed/5 (hard delete)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.MaintenancePrograms.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            try
            {
                _uow.MaintenancePrograms.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Programme d'entretien supprimé définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Impossible de supprimer — utilisé par d'autres données (types d'inspection, job cards...). Désactivez-le plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AircraftMaintenance/MaintenancePrograms/ToggleActive/5 (soft delete / reactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.MaintenancePrograms.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.MaintenancePrograms.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive
                ? "Programme d'entretien réactivé."
                : "Programme d'entretien désactivé.";

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task<bool> IsAcTypeInScopeAsync(int acTypeId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted || !scope.AllowedAcMainGroupIds.Any()) return true;

            var acType = await _uow.AcTypes.GetByIdAsync(acTypeId);
            return acType != null &&
                   scope.AllowedAcMainGroupIds.Contains(acType.AcMainGroupId);
        }

        private static MaintenanceProgramDetailsViewModel MapToDetailsVm(MaintenanceProgram x) => new()
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            AcTypeId = x.AcTypeId,
            AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
            DocReference = x.DocReference,
            Edition = x.Edition,
            ChangeNo = x.ChangeNo,
            ChangeDate = x.ChangeDate,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc
            // InspectionTypes intentionally left empty here — the
            // InspectionTypeProgram junction doesn't have a controller yet.
        };

        private async Task PopulateDropdownsAsync(MaintenanceProgramFormViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var acTypes = await _uow.AcTypes.GetAllAsync();

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                acTypes = acTypes.Where(a =>
                    scope.AllowedAcMainGroupIds.Contains(a.AcMainGroupId));
            }

            vm.AcTypes = acTypes
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
