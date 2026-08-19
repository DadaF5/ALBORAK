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
    public class InspectionTypesController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScopeService;

        public InspectionTypesController(IUnitOfWork uow, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/InspectionTypes
        // AcType-level setup data (no Aircraft/Base of its own) — scoped by
        // AcMainGroup only, same as JobCards/MaintenancePrograms.
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var items = await _uow.InspectionTypes.GetAllWithDetailsAsync();

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                items = items.Where(x =>
                    x.AcType != null &&
                    scope.AllowedAcMainGroupIds.Contains(x.AcType.AcMainGroupId)).ToList();
            }

            // Single batched query for all InspectionTypeProgram links,
            // not one query per row (avoids N+1).
            var allIds = items.Select(x => x.Id).ToList();
            var allLinks = await _uow.InspectionTypePrograms.GetByInspectionTypeIdsAsync(allIds);
            var codesByInspectionTypeId = allLinks
                .Where(l => l.MaintenanceProgram != null)
                .GroupBy(l => l.InspectionTypeId)
                .ToDictionary(g => g.Key, g => g.Select(l => l.MaintenanceProgram!.Code).OrderBy(c => c).ToList());

            var vm = items.Select(x => new InspectionTypeListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Kind = x.Kind,
                AcTypeId = x.AcTypeId,
                AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
                IntervalHours = x.IntervalHours,
                IntervalCycles = x.IntervalCycles,
                CalendarValue = x.CalendarValue,
                CalendarUnit = x.CalendarUnit,
                ProgramCodes = codesByInspectionTypeId.GetValueOrDefault(x.Id, []),
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            // Full AcType list (not just ones with InspectionType rows) —
            // so the filter dropdown shows every valid aircraft type,
            // including ones with zero InspectionTypes seeded yet. Scoped
            // the same way as the item list above.
            var allAcTypes = await _uow.AcTypes.GetAllAsync();
            var visibleAcTypes = allAcTypes.Where(t => t.IsActive);
            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                visibleAcTypes = visibleAcTypes.Where(t => scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId));
            }
            ViewBag.AllAcTypeLabels = visibleAcTypes
                .OrderBy(t => t.Code)
                .Select(t => $"{t.Code} — {t.Name}")
                .ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/InspectionTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = await MapToDetailsVmAsync(entity);
            return View(vm);
        }

        // GET: AircraftMaintenance/InspectionTypes/Create
        public async Task<IActionResult> Create()
        {
            var vm = new InspectionTypeFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(InspectionTypeFormViewModel vm)
        {
            // Defense in depth — dropdown only offers in-scope AcTypes, but
            // AcTypeId is still a posted value and can be tampered with.
            if (!await IsAcTypeInScopeAsync(vm.AcTypeId))
                return Forbid();

            if (await _uow.InspectionTypes.ExistsByCodeAsync(vm.AcTypeId, vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new InspectionType
            {
                AcTypeId = vm.AcTypeId,
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Kind = vm.Kind,
                IntervalHours = vm.IntervalHours,
                IntervalCycles = vm.IntervalCycles,
                CalendarValue = vm.CalendarValue,
                CalendarUnit = vm.CalendarUnit,
                ToleranceHours = vm.ToleranceHours,
                ToleranceCycles = vm.ToleranceCycles,
                ToleranceCalendarValue = vm.ToleranceCalendarValue,
                ToleranceCalendarUnit = vm.ToleranceCalendarUnit,
                NextInspectionTypeId = vm.NextInspectionTypeId,
                SortOrder = (byte)vm.SortOrder,
                IsActive = vm.IsActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.InspectionTypes.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Type d'inspection créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/InspectionTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = new InspectionTypeFormViewModel
            {
                Id = entity.Id,
                AcTypeId = entity.AcTypeId,
                Code = entity.Code,
                Name = entity.Name,
                Kind = entity.Kind,
                IntervalHours = entity.IntervalHours,
                IntervalCycles = entity.IntervalCycles,
                CalendarValue = entity.CalendarValue,
                CalendarUnit = entity.CalendarUnit,
                ToleranceHours = entity.ToleranceHours,
                ToleranceCycles = entity.ToleranceCycles,
                ToleranceCalendarValue = entity.ToleranceCalendarValue,
                ToleranceCalendarUnit = entity.ToleranceCalendarUnit,
                NextInspectionTypeId = entity.NextInspectionTypeId,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            await PopulateDropdownsAsync(vm, excludeId: id);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, InspectionTypeFormViewModel vm)
        {
            if (id != vm.Id)
            {
                return BadRequest();
            }

            if (!await IsAcTypeInScopeAsync(vm.AcTypeId))
                return Forbid();

            if (await _uow.InspectionTypes.ExistsByCodeAsync(vm.AcTypeId, vm.Code, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Ce code existe déjà pour ce type d'aéronef.");
            }

            if (vm.NextInspectionTypeId == id)
            {
                ModelState.AddModelError(nameof(vm.NextInspectionTypeId),
                    "Un type d'inspection ne peut pas se référencer lui-même.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm, excludeId: id);
                return View(vm);
            }

            var entity = await _uow.InspectionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.AcTypeId = vm.AcTypeId;
            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Kind = vm.Kind;
            entity.IntervalHours = vm.IntervalHours;
            entity.IntervalCycles = vm.IntervalCycles;
            entity.CalendarValue = vm.CalendarValue;
            entity.CalendarUnit = vm.CalendarUnit;
            entity.ToleranceHours = vm.ToleranceHours;
            entity.ToleranceCycles = vm.ToleranceCycles;
            entity.ToleranceCalendarValue = vm.ToleranceCalendarValue;
            entity.ToleranceCalendarUnit = vm.ToleranceCalendarUnit;
            entity.NextInspectionTypeId = vm.NextInspectionTypeId;
            entity.SortOrder = (byte)vm.SortOrder;
            entity.IsActive = vm.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.InspectionTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Type d'inspection modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/InspectionTypes/Delete/5
        // Confirmation page — offers Deactivate (soft, recommended) or Delete (hard).
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            var vm = await MapToDetailsVmAsync(entity);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/DeleteConfirmed/5 (hard delete)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            try
            {
                _uow.InspectionTypes.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Type d'inspection supprimé définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Impossible de supprimer — utilisé par d'autres données (programmes, ordres de travail...). Désactivez-le plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AircraftMaintenance/InspectionTypes/ToggleActive/5 (soft delete / reactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            if (!await IsAcTypeInScopeAsync(entity.AcTypeId))
                return Forbid();

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.InspectionTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive
                ? "Type d'inspection réactivé."
                : "Type d'inspection désactivé.";

            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/InspectionTypes/ManagePrograms/5
        public async Task<IActionResult> ManagePrograms(int id)
        {
            var vm = await BuildManageProgramsVmAsync(id);
            if (vm == null) return NotFound();

            if (!await IsInspectionTypeInScopeAsync(id))
                return Forbid();

            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/AddProgram
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> AddProgram(int inspectionTypeId, int maintenanceProgramId)
        {
            if (!await IsInspectionTypeInScopeAsync(inspectionTypeId))
                return Forbid();

            var existingLinks = await _uow.InspectionTypePrograms.GetByInspectionTypeIdsAsync([inspectionTypeId]);

            if (existingLinks.Any(l => l.MaintenanceProgramId == maintenanceProgramId))
            {
                TempData["Error"] = "Ce programme est déjà associé à ce type d'inspection.";
            }
            else
            {
                await _uow.InspectionTypePrograms.AddAsync(new InspectionTypeProgram
                {
                    InspectionTypeId = inspectionTypeId,
                    MaintenanceProgramId = maintenanceProgramId,
                    SortOrder = 100
                });
                await _uow.CompleteAsync();
                TempData["Success"] = "Programme associé avec succès.";
            }

            return RedirectToAction(nameof(ManagePrograms), new { id = inspectionTypeId });
        }

        // POST: AircraftMaintenance/InspectionTypes/RemoveProgram/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> RemoveProgram(int id)
        {
            var link = await _uow.InspectionTypePrograms.GetByIdAsync(id);
            if (link == null) return NotFound();

            if (!await IsInspectionTypeInScopeAsync(link.InspectionTypeId))
                return Forbid();

            var inspectionTypeId = link.InspectionTypeId;

            _uow.InspectionTypePrograms.Delete(link);
            await _uow.CompleteAsync();

            TempData["Success"] = "Programme retiré.";
            return RedirectToAction(nameof(ManagePrograms), new { id = inspectionTypeId });
        }

        private async Task<InspectionTypeManageProgramsViewModel?> BuildManageProgramsVmAsync(int inspectionTypeId)
        {
            var inspectionType = await _uow.InspectionTypes.GetByIdWithDetailsAsync(inspectionTypeId);
            if (inspectionType == null) return null;

            var links = await _uow.InspectionTypePrograms.GetByInspectionTypeIdsAsync([inspectionTypeId]);
            var linkedProgramIds = links.Select(l => l.MaintenanceProgramId).ToHashSet();

            var allPrograms = await _uow.MaintenancePrograms.GetAllWithDetailsAsync();

            return new InspectionTypeManageProgramsViewModel
            {
                InspectionTypeId = inspectionType.Id,
                InspectionTypeCode = inspectionType.Code,
                InspectionTypeName = inspectionType.Name,
                LinkedPrograms = links
                    .Where(l => l.MaintenanceProgram != null)
                    .Select(l => new LinkedProgramItemViewModel
                    {
                        LinkId = l.Id,
                        MaintenanceProgramId = l.MaintenanceProgramId,
                        ProgramCode = l.MaintenanceProgram!.Code,
                        ProgramName = l.MaintenanceProgram.Name
                    })
                    .OrderBy(p => p.ProgramCode)
                    .ToList(),
                AvailablePrograms = allPrograms
                    .Where(p => p.AcTypeId == inspectionType.AcTypeId && p.IsActive)
                    .Where(p => !linkedProgramIds.Contains(p.Id))
                    .OrderBy(p => p.Code)
                    .Select(p => new MaintenanceProgramLookupViewModel
                    {
                        Id = p.Id,
                        Code = p.Code,
                        Name = p.Name,
                        AcTypeId = p.AcTypeId
                    })
                    .ToList()
            };
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

        private async Task<bool> IsInspectionTypeInScopeAsync(int inspectionTypeId)
        {
            var it = await _uow.InspectionTypes.GetByIdAsync(inspectionTypeId);
            return it != null && await IsAcTypeInScopeAsync(it.AcTypeId);
        }

        // Now async (was a static sync helper) — needs to query the
        // InspectionTypeProgram junction to populate Programs, which
        // wasn't possible when this was first written (no repository
        // existed for that junction yet).
        private async Task<InspectionTypeDetailsViewModel> MapToDetailsVmAsync(InspectionType x)
        {
            var vm = new InspectionTypeDetailsViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Kind = x.Kind,
                AcTypeId = x.AcTypeId,
                AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
                IntervalHours = x.IntervalHours,
                IntervalCycles = x.IntervalCycles,
                CalendarValue = x.CalendarValue,
                CalendarUnit = x.CalendarUnit,
                ToleranceHours = x.ToleranceHours,
                ToleranceCycles = x.ToleranceCycles,
                ToleranceCalendarValue = x.ToleranceCalendarValue,
                ToleranceCalendarUnit = x.ToleranceCalendarUnit,
                NextInspectionTypeId = x.NextInspectionTypeId,
                NextInspectionTypeLabel = x.NextInspectionType?.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            };

            var links = await _uow.InspectionTypePrograms.GetByInspectionTypeIdsAsync([x.Id]);
            vm.Programs = links
                .Where(l => l.MaintenanceProgram != null)
                .Select(l => new MaintenanceProgramListItemViewModel
                {
                    Id = l.MaintenanceProgram!.Id,
                    Code = l.MaintenanceProgram.Code,
                    Name = l.MaintenanceProgram.Name,
                    AcTypeId = l.MaintenanceProgram.AcTypeId,
                    AcTypeLabel = vm.AcTypeLabel,
                    IsActive = l.MaintenanceProgram.IsActive,
                    SortOrder = l.MaintenanceProgram.SortOrder
                })
                .OrderBy(p => p.SortOrder)
                .ToList();

            return vm;
        }

        private async Task PopulateDropdownsAsync(InspectionTypeFormViewModel vm, int? excludeId = null)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var acTypes = await _uow.AcTypes.GetAllAsync();
            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                acTypes = acTypes.Where(a => scope.AllowedAcMainGroupIds.Contains(a.AcMainGroupId));
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

            var inspectionTypes = await _uow.InspectionTypes.GetAllAsync();
            var visibleInspectionTypes = inspectionTypes
                .Where(t => !excludeId.HasValue || t.Id != excludeId.Value);

            if (!scope.IsUnrestricted && scope.AllowedAcMainGroupIds.Any())
            {
                var acTypesById = (await _uow.AcTypes.GetAllAsync()).ToDictionary(t => t.Id);
                visibleInspectionTypes = visibleInspectionTypes.Where(t =>
                    acTypesById.TryGetValue(t.AcTypeId, out var at) &&
                    scope.AllowedAcMainGroupIds.Contains(at.AcMainGroupId));
            }

            vm.NextInspectionTypes = visibleInspectionTypes
                .OrderBy(t => t.Code)
                .Select(t => new LookupOptionViewModel
                {
                    Id = t.Id,
                    Label = $"{t.Code} — {t.Name}"
                })
                .ToList();
        }
    }
}
