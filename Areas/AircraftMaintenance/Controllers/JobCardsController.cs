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
                AtaId = x.AtaId,
                AtaLabel = x.Ata != null ? $"{x.Ata.Code} — {x.Ata.Name}" : null,
                Specialty = x.Specialty,
                AllocatedTimeMinutes = x.AllocatedTimeMinutes,
                ToReference = x.ToReference,
                DocReference = x.DocReference,
                Edition = x.Edition,
                ChangeNo = x.ChangeNo,
                ChangeDate = x.ChangeDate,
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
                AtaId = vm.AtaId,
                Specialty = vm.Specialty,
                AllocatedTimeMinutes = vm.AllocatedTimeMinutes,
                WorkAreas = vm.WorkAreas,
                MechNo = vm.MechNo,
                ElectricalPowerRequired = vm.ElectricalPowerRequired,
                FigureRef = vm.FigureRef,
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
                AtaId = entity.AtaId,
                Specialty = entity.Specialty,
                AllocatedTimeMinutes = entity.AllocatedTimeMinutes,
                WorkAreas = entity.WorkAreas,
                MechNo = entity.MechNo,
                ElectricalPowerRequired = entity.ElectricalPowerRequired,
                FigureRef = entity.FigureRef,
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
            entity.AtaId = vm.AtaId;
            entity.Specialty = vm.Specialty;
            entity.AllocatedTimeMinutes = vm.AllocatedTimeMinutes;
            entity.WorkAreas = vm.WorkAreas;
            entity.MechNo = vm.MechNo;
            entity.ElectricalPowerRequired = vm.ElectricalPowerRequired;
            entity.FigureRef = vm.FigureRef;
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
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.JobCards.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = MapToDetailsVm(entity);
            return View(vm);
        }

        // POST: AircraftMaintenance/JobCards/DeleteConfirmed/5
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

        // POST: AircraftMaintenance/JobCards/ToggleActive/5
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
            AtaId = x.AtaId,
            AtaLabel = x.Ata != null ? $"{x.Ata.Code} — {x.Ata.Name}" : null,
            Specialty = x.Specialty,
            AllocatedTimeMinutes = x.AllocatedTimeMinutes,
            WorkAreas = x.WorkAreas,
            MechNo = x.MechNo,
            ElectricalPowerRequired = x.ElectricalPowerRequired,
            FigureRef = x.FigureRef,
            ToReference = x.ToReference,
            DocReference = x.DocReference,
            Edition = x.Edition,
            ChangeNo = x.ChangeNo,
            ChangeDate = x.ChangeDate,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc
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

            var ataChapters = await _uow.Ata.GetAllAsync();
            vm.AtaChapters = ataChapters
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder).ThenBy(a => a.Code)
                .Select(a => new AtaLookupViewModel
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name
                })
                .ToList();
        }
    }
}