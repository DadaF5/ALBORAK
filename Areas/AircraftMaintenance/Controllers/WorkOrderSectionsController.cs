using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class WorkOrderSectionsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;
        // ⚠ Same note as WorkOrdersController — adjust ApplicationUser
        // above if your real type differs.

        public WorkOrderSectionsController(
            IUnitOfWork uow, UserManager<ApplicationUser> userManager, IUserScopeService userScopeService)
        {
            _uow = uow;
            _userManager = userManager;
            _userScopeService = userScopeService;
        }

        // GET: AircraftMaintenance/WorkOrderSections/Index/5  (5 = WorkOrderId)
        public async Task<IActionResult> Index(int id)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(id);
            if (workOrder == null) return NotFound();

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

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

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

            var vm = new WorkOrderSectionFormViewModel { WorkOrderId = id };
            await PopulateDropdownsAsync(vm, workOrder.Aircraft?.AcTypeId ?? 0);
            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(WorkOrderSectionFormViewModel vm)
        {
            var workOrder = await _uow.WorkOrders.GetByIdWithDetailsAsync(vm.WorkOrderId);
            if (workOrder == null) return NotFound();

            if (!await IsAircraftInScopeAsync(workOrder.AircraftId))
                return Forbid();

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

            if (!await IsSectionInScopeAsync(entity))
                return Forbid();

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
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, WorkOrderSectionFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionInScopeAsync(entity))
                return Forbid();

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

            if (!await IsSectionInScopeAsync(entity))
                return Forbid();

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

            if (!await IsSectionInScopeAsync(entity))
                return Forbid();

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
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            if (!await IsSectionInScopeAsync(entity))
                return Forbid();

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

        // ── Helpers ──────────────────────────────────────────────────────

        // Wraps IsAircraftInScopeAsync for cases where entity.WorkOrder might
        // not have loaded — fails CLOSED for scoped users if we can't verify
        // the aircraft (Admin/unrestricted still passes immediately).
        private async Task<bool> IsSectionInScopeAsync(WorkOrderSection entity)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;
            if (entity.WorkOrder == null) return false;
            return await IsAircraftInScopeAsync(entity.WorkOrder.AircraftId);
        }

        private async Task<bool> IsAircraftInScopeAsync(int aircraftId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;

            var aircraft = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null || !aircraft.BaseId.HasValue ||
                !scope.AllowedBaseIds.Contains(aircraft.BaseId.Value))
                return false;

            if (!scope.AllowedAcMainGroupIds.Any())
                return true;

            var acType = await _uow.AcTypes.GetByIdAsync(aircraft.AcTypeId);
            return acType != null &&
                   scope.AllowedAcMainGroupIds.Contains(acType.AcMainGroupId);
        }

        // NOTE: takes the aircraft's AcTypeId (unchanged call sites), but
        // WorkSection now keys on AcMainGroupId, not AcTypeId directly
        // (see WorkSection.cs — F16C/F16D and F5E/F5F share sections at
        // the family level). Resolves AcType -> AcMainGroupId once here
        // rather than changing every caller.
        private async Task PopulateDropdownsAsync(WorkOrderSectionFormViewModel vm, int acTypeId)
        {
            var acType = await _uow.AcTypes.GetByIdAsync(acTypeId);
            if (acType == null)
            {
                vm.AvailableSections = [];
                return;
            }

            var sections = await _uow.WorkSections.GetAllWithDetailsAsync();
            vm.AvailableSections = sections
                .Where(s => s.AcMainGroupId == acType.AcMainGroupId && s.IsActive)
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
