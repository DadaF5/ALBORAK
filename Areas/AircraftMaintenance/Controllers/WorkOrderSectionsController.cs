using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class WorkOrderSectionsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        // ⚠ Same note as WorkOrdersController — adjust ApplicationUser
        // above if your real type differs.

        public WorkOrderSectionsController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // GET: AircraftMaintenance/WorkOrderSections/Index/5  (5 = WorkOrderId)
        public async Task<IActionResult> Index(int id)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (workOrder == null) return NotFound();

            var sections = await _uow.WorkOrderSections.GetByWorkOrderIdWithDetailsAsync(id);

            ViewBag.WorkOrderId = id;
            ViewBag.WONumber = workOrder.WONumber;

            var vm = sections.Select(x => new WorkOrderSectionListItemViewModel
            {
                Id = x.Id,
                SectionCode = x.WorkSection?.Code ?? "—",
                SectionName = x.WorkSection?.Name ?? "—",
                FormNumber = x.FormNumber,
                TypeTravail = x.TypeTravail,
                DateDebut = x.DateDebut,
                DateFin = x.DateFin,
                Status = x.Status
            }).ToList();

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrderSections/Create/5  (5 = WorkOrderId)
        public async Task<IActionResult> Create(int id)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (workOrder == null) return NotFound();

            var vm = new WorkOrderSectionFormViewModel { WorkOrderId = id };
            await PopulateDropdownsAsync(vm, workOrder.Aircraft?.AcTypeId ?? 0);
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderSectionFormViewModel vm)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(vm.WorkOrderId);
            if (workOrder == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm, workOrder.Aircraft?.AcTypeId ?? 0);
                return View(vm);
            }

            var entity = new WorkOrderSection
            {
                WorkOrderId = vm.WorkOrderId,
                WorkSectionId = vm.WorkSectionId,
                FormNumber = vm.FormNumber,
                OrganismeResponsable = vm.OrganismeResponsable,
                TypeTravail = vm.TypeTravail,
                DateDebut = vm.DateDebut,
                DateFin = vm.DateFin,
                TempsAlloueMinutes = vm.TempsAlloueMinutes,
                TempsPasseSystematiqueMinutes = vm.TempsPasseSystematiqueMinutes,
                TempsPasseRetoucheMinutes = vm.TempsPasseRetoucheMinutes,
                VieillissementHours = vm.VieillissementHours,
                Directives = vm.Directives,
                TechnicalOrderReference = vm.TechnicalOrderReference,
                DirectiveIssuedByName = vm.DirectiveIssuedByName,
                DirectiveIssuedAtUtc = string.IsNullOrEmpty(vm.DirectiveIssuedByName) ? null : DateTime.UtcNow,
                Status = vm.Status,
                OpenedByUserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _uow.WorkOrderSections.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Section (Formule 13) créée avec succès.";
            return RedirectToAction(nameof(Index), new { id = vm.WorkOrderId });
        }

        // GET: AircraftMaintenance/WorkOrderSections/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = new WorkOrderSectionFormViewModel
            {
                Id = entity.Id,
                WorkOrderId = entity.WorkOrderId,
                WorkSectionId = entity.WorkSectionId,
                FormNumber = entity.FormNumber,
                OrganismeResponsable = entity.OrganismeResponsable,
                TypeTravail = entity.TypeTravail,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                TempsAlloueMinutes = entity.TempsAlloueMinutes,
                TempsPasseSystematiqueMinutes = entity.TempsPasseSystematiqueMinutes,
                TempsPasseRetoucheMinutes = entity.TempsPasseRetoucheMinutes,
                VieillissementHours = entity.VieillissementHours,
                Directives = entity.Directives,
                TechnicalOrderReference = entity.TechnicalOrderReference,
                DirectiveIssuedByName = entity.DirectiveIssuedByName,
                Status = entity.Status
            };

            await PopulateDropdownsAsync(vm, entity.WorkOrder?.Aircraft?.AcTypeId ?? 0);
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkOrderSectionFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm, entity.WorkOrder?.Aircraft?.AcTypeId ?? 0);
                return View(vm);
            }

            var directiveChanged = entity.DirectiveIssuedByName != vm.DirectiveIssuedByName;

            entity.WorkSectionId = vm.WorkSectionId;
            entity.FormNumber = vm.FormNumber;
            entity.OrganismeResponsable = vm.OrganismeResponsable;
            entity.TypeTravail = vm.TypeTravail;
            entity.DateDebut = vm.DateDebut;
            entity.DateFin = vm.DateFin;
            entity.TempsAlloueMinutes = vm.TempsAlloueMinutes;
            entity.TempsPasseSystematiqueMinutes = vm.TempsPasseSystematiqueMinutes;
            entity.TempsPasseRetoucheMinutes = vm.TempsPasseRetoucheMinutes;
            entity.VieillissementHours = vm.VieillissementHours;
            entity.Directives = vm.Directives;
            entity.TechnicalOrderReference = vm.TechnicalOrderReference;
            entity.DirectiveIssuedByName = vm.DirectiveIssuedByName;
            if (directiveChanged && !string.IsNullOrEmpty(vm.DirectiveIssuedByName))
                entity.DirectiveIssuedAtUtc = DateTime.UtcNow;
            entity.Status = vm.Status;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            _uow.WorkOrderSections.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Section modifiée avec succès.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderId });
        }

        // GET: AircraftMaintenance/WorkOrderSections/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = new WorkOrderSectionDetailsViewModel
            {
                Id = entity.Id,
                WorkOrderId = entity.WorkOrderId,
                WONumber = entity.WorkOrder?.WONumber ?? "—",
                AircraftLabel = entity.WorkOrder?.Aircraft?.Registration ?? "—",
                SectionCode = entity.WorkSection?.Code ?? "—",
                SectionName = entity.WorkSection?.Name ?? "—",
                FormNumber = entity.FormNumber,
                OrganismeResponsable = entity.OrganismeResponsable,
                TypeTravail = entity.TypeTravail,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                TempsAlloueMinutes = entity.TempsAlloueMinutes,
                TempsPasseSystematiqueMinutes = entity.TempsPasseSystematiqueMinutes,
                TempsPasseRetoucheMinutes = entity.TempsPasseRetoucheMinutes,
                VieillissementHours = entity.VieillissementHours,
                Directives = entity.Directives,
                TechnicalOrderReference = entity.TechnicalOrderReference,
                DirectiveIssuedByName = entity.DirectiveIssuedByName,
                DirectiveIssuedAtUtc = entity.DirectiveIssuedAtUtc,
                Status = entity.Status,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };

            return View(vm);
        }

        // GET: AircraftMaintenance/WorkOrderSections/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            var vm = new WorkOrderSectionDetailsViewModel
            {
                Id = entity.Id,
                WorkOrderId = entity.WorkOrderId,
                WONumber = entity.WorkOrder?.WONumber ?? "—",
                AircraftLabel = entity.WorkOrder?.Aircraft?.Registration ?? "—",
                SectionCode = entity.WorkSection?.Code ?? "—",
                SectionName = entity.WorkSection?.Name ?? "—",
                FormNumber = entity.FormNumber,
                Status = entity.Status
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSections/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.WorkOrderSections.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var workOrderId = entity.WorkOrderId;

            try
            {
                _uow.WorkOrderSections.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Section supprimée.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Impossible de supprimer — des données liées existent.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index), new { id = workOrderId });
        }

        private async Task PopulateDropdownsAsync(WorkOrderSectionFormViewModel vm, int acTypeId)
        {
            var sections = await _uow.WorkSections.GetAllWithDetailsAsync();
            vm.AvailableSections = sections
                .Where(s => s.AcTypeId == acTypeId && s.IsActive)
                .OrderBy(s => s.Code)
                .Select(s => new WorkSectionLookupViewModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                })
                .ToList();
        }
    }
}