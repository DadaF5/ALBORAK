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
    public class InspectionTypesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public InspectionTypesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/InspectionTypes
        public async Task<IActionResult> Index()
        {
            var items = await _uow.InspectionTypes.GetAllWithDetailsAsync();

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
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/InspectionTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToDetailsVm(entity);
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
        public async Task<IActionResult> Create(InspectionTypeFormViewModel vm)
        {
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
        public async Task<IActionResult> Edit(int id, InspectionTypeFormViewModel vm)
        {
            if (id != vm.Id)
            {
                return BadRequest();
            }

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

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/DeleteConfirmed/5 (hard delete)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

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
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.InspectionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.InspectionTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive
                ? "Type d'inspection réactivé."
                : "Type d'inspection désactivé.";

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static InspectionTypeDetailsViewModel MapToDetailsVm(InspectionType x) => new()
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
            // Programs intentionally left empty here — MaintenanceProgram
            // module isn't built yet (Phase 2 of the Inspection Guide).
            // Wire this up once InspectionTypeProgram junction has a controller.
        };

        private async Task PopulateDropdownsAsync(InspectionTypeFormViewModel vm, int? excludeId = null)
        {
            var acTypes = await _uow.AcTypes.GetAllAsync();
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
            vm.NextInspectionTypes = inspectionTypes
                .Where(t => !excludeId.HasValue || t.Id != excludeId.Value)
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
