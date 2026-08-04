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
    public class JobCardsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public JobCardsController(IUnitOfWork uow)
        {
            _uow = uow;
        }
       
        // GET: AircraftMaintenance/JobCards
        public async Task<IActionResult> Index()
        {
            var items = await _uow.JobCards.GetAllWithDetailsAsync();

            var vm = items.Select(x => new JobCardListItemViewModel
            {
                Id = x.Id,
                CardCode = x.CardCode,
                Title = x.Title,
                AcTypeId = x.AcTypeId,
                AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
                AtaCode = x.AtaCode,
                Specialty = x.Specialty,
                AllocatedTimeMinutes = x.AllocatedTimeMinutes,
                ToReference = x.ToReference,           // ← ADD
                DocReference = x.DocReference,         // ← ADD
                Edition = x.Edition,                   // ← ADD
                ChangeNo = x.ChangeNo,                 // ← ADD
                ChangeDate = x.ChangeDate,              // ← ADD
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/JobCards/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.JobCards.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // GET: AircraftMaintenance/JobCards/Create
        public async Task<IActionResult> Create()
        {
            var vm = new JobCardFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: AircraftMaintenance/JobCards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCardFormViewModel vm)
        {
            if (await _uow.JobCards.ExistsByCodeAsync(vm.AcTypeId, vm.CardCode))
            {
                ModelState.AddModelError(nameof(vm.CardCode),
                    "Ce code de job card existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = new JobCard
            {
                AcTypeId = vm.AcTypeId,
                CardCode = vm.CardCode.Trim().ToUpper(),
                Title = vm.Title.Trim(),
                Description = vm.Description,
                AtaCode = vm.AtaCode,
                Specialty = vm.Specialty,
                AllocatedTimeMinutes = vm.AllocatedTimeMinutes,
                WorkAreas = vm.WorkAreas,                                   // ← ADD
                MechNo = vm.MechNo,                                         // ← ADD
                ElectricalPowerRequired = vm.ElectricalPowerRequired,       // ← ADD
                FigureRef = vm.FigureRef,                                   // ← ADD
                ToReference = vm.ToReference,
                DocReference = vm.DocReference,
                Edition = vm.Edition,
                ChangeNo = vm.ChangeNo,
                ChangeDate = vm.ChangeDate,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.JobCards.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Job card créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/JobCards/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.JobCards.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = new JobCardFormViewModel
            {
                Id = entity.Id,
                AcTypeId = entity.AcTypeId,
                CardCode = entity.CardCode,
                Title = entity.Title,
                Description = entity.Description,
                AtaCode = entity.AtaCode,
                Specialty = entity.Specialty,
                AllocatedTimeMinutes = entity.AllocatedTimeMinutes,
                WorkAreas = entity.WorkAreas,                               // ← ADD
                MechNo = entity.MechNo,                                     // ← ADD
                ElectricalPowerRequired = entity.ElectricalPowerRequired,   // ← ADD
                FigureRef = entity.FigureRef,                               // ← ADD
                ToReference = entity.ToReference,
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

        // POST: AircraftMaintenance/JobCards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobCardFormViewModel vm)
        {
            if (id != vm.Id)
            {
                return BadRequest();
            }

            if (await _uow.JobCards.ExistsByCodeAsync(vm.AcTypeId, vm.CardCode, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.CardCode),
                    "Ce code de job card existe déjà pour ce type d'aéronef.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var entity = await _uow.JobCards.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.AcTypeId = vm.AcTypeId;
            entity.CardCode = vm.CardCode.Trim().ToUpper();
            entity.Title = vm.Title.Trim();
            entity.Description = vm.Description;
            entity.AtaCode = vm.AtaCode;
            entity.Specialty = vm.Specialty;
            entity.AllocatedTimeMinutes = vm.AllocatedTimeMinutes;
            entity.WorkAreas = vm.WorkAreas;                                // ← ADD
            entity.MechNo = vm.MechNo;                                      // ← ADD
            entity.ElectricalPowerRequired = vm.ElectricalPowerRequired;    // ← ADD
            entity.FigureRef = vm.FigureRef;                                // ← ADD
            entity.ToReference = vm.ToReference;
            entity.DocReference = vm.DocReference;
            entity.Edition = vm.Edition;
            entity.ChangeNo = vm.ChangeNo;
            entity.ChangeDate = vm.ChangeDate;
            entity.SortOrder = vm.SortOrder;
            entity.IsActive = vm.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.JobCards.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Job card modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/JobCards/Delete/5
        // Confirmation page — offers Deactivate (soft, recommended) or Delete (hard).
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.JobCards.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // POST: AircraftMaintenance/JobCards/DeleteConfirmed/5 (hard delete)
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.JobCards.GetByIdAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _uow.JobCards.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Job card supprimée définitivement.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Impossible de supprimer — utilisée par d'autres données (programmes, work orders...). Désactivez-la plutôt.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AircraftMaintenance/JobCards/ToggleActive/5 (soft delete / reactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.JobCards.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.JobCards.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive
                ? "Job card réactivée."
                : "Job card désactivée.";

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static JobCardDetailsViewModel MapToDetailsVm(JobCard x) => new()
        {
            Id = x.Id,
            CardCode = x.CardCode,
            Title = x.Title,
            Description = x.Description,
            AcTypeId = x.AcTypeId,
            AcTypeLabel = x.AcType != null ? $"{x.AcType.Code} — {x.AcType.Name}" : "—",
            AtaCode = x.AtaCode,
            Specialty = x.Specialty,
            AllocatedTimeMinutes = x.AllocatedTimeMinutes,
            WorkAreas = x.WorkAreas,                                        // ← ADD
            MechNo = x.MechNo,                                              // ← ADD
            ElectricalPowerRequired = x.ElectricalPowerRequired,            // ← ADD
            FigureRef = x.FigureRef,                                        // ← ADD
            ToReference = x.ToReference,
            DocReference = x.DocReference,
            Edition = x.Edition,
            ChangeNo = x.ChangeNo,
            ChangeDate = x.ChangeDate,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc
            // MaintenancePrograms intentionally left empty here — the
            // ProgramJobCard junction doesn't have a controller yet.
        };

        private async Task PopulateDropdownsAsync(JobCardFormViewModel vm)
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
        }
    }
}