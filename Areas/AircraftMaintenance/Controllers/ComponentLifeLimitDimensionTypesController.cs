using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    /// <summary>
    /// NEW — CRUD for ComponentLifeLimitDimensionType, prompted by Dadda's
    /// question while testing Receipt: "what's the link to access and CRUD
    /// this list" (the Heures de vol/Cycles/Atterrissages T&amp;G/Atterrissages
    /// complets rows on the Réception composant form's "Pièce usagée" table).
    /// There was no controller for this lookup at all before this pass —
    /// it's only ever been read (ComponentTypesController's
    /// PopulateDimensionTypeOptionsAsync / PopulateDerogationDimensionTypeOptionsAsync)
    /// or written by ComponentLifeLimitDimensionTypeSeeder at startup.
    ///
    /// Deliberately NOT a hard-Delete anywhere — same soft-delete-via-
    /// IsActive convention as every other catalog entity in this module.
    /// This one especially: a dimension already referenced by a
    /// ComponentLifeLimitStageDimension/ComponentEventReading/
    /// ComponentInitialReadingValue row can't be safely removed, and Code
    /// is a stable contract the calculator switches on (see this
    /// controller's Edit action and the ViewModel's doc comment).
    /// </summary>
    [Area("AircraftMaintenance")]
    public class ComponentLifeLimitDimensionTypesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ComponentLifeLimitDimensionTypesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Index()
        {
            var all = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            var mainGroups = await _uow.AcMainGroups.GetAllAsync();
            var mgLabels = mainGroups.ToDictionary(g => g.Id, g => $"{g.Code} — {g.Name}");
            ViewBag.AcMainGroupLabels = mgLabels;

            return View(all.OrderBy(d => d.SortOrder).ThenBy(d => d.Code).ToList());
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsAsync();
            return View(new ComponentLifeLimitDimensionTypeFormDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(ComponentLifeLimitDimensionTypeFormDto dto)
        {
            dto.Code = (dto.Code ?? string.Empty).Trim().ToUpperInvariant();

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(dto);
            }

            var existing = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            if (existing.Any(d => d.Code == dto.Code))
            {
                ModelState.AddModelError(nameof(dto.Code), "Ce code existe déjà.");
                await PopulateLookupsAsync();
                return View(dto);
            }

            _uow.ComponentLifeLimitDimensionTypes.Add(new ComponentLifeLimitDimensionType
            {
                Code = dto.Code,
                Name = dto.Name,
                Unit = dto.Unit,
                IsCalendarBased = dto.IsCalendarBased,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                AcMainGroupId = dto.AcMainGroupId
            });
            await _uow.CompleteAsync();

            TempData["Success"] = "Dimension créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.ComponentLifeLimitDimensionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            await PopulateLookupsAsync();
            return View(new ComponentLifeLimitDimensionTypeFormDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Unit = entity.Unit,
                IsCalendarBased = entity.IsCalendarBased,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                AcMainGroupId = entity.AcMainGroupId
            });
        }

        /// <summary>
        /// CHANGED — dto.Code is deliberately never applied here, whatever
        /// was posted (the field is rendered read-only in _Form.cshtml, but
        /// that's a UI nicety, not a guarantee — a modified/replayed POST
        /// could still carry a different value). Code is a stable contract
        /// once a row exists (see the ViewModel's doc comment) — only
        /// Name/Unit/IsCalendarBased/IsActive/SortOrder/AcMainGroupId are
        /// ever written back.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(ComponentLifeLimitDimensionTypeFormDto dto)
        {
            if (dto.Id is null) return NotFound();
            var entity = await _uow.ComponentLifeLimitDimensionTypes.GetByIdAsync(dto.Id.Value);
            if (entity == null) return NotFound();

            // Code is immutable post-creation — see doc comment above.
            dto.Code = entity.Code;
            ModelState.Remove(nameof(dto.Code));

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(dto);
            }

            entity.Name = dto.Name;
            entity.Unit = dto.Unit;
            entity.IsCalendarBased = dto.IsCalendarBased;
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            entity.AcMainGroupId = dto.AcMainGroupId;
            _uow.ComponentLifeLimitDimensionTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Dimension mise à jour avec succès.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// NEW — quick activate/deactivate from Index, same pattern as
        /// ComponentPositionsController.ToggleActive. Never a hard Delete —
        /// see class doc comment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _uow.ComponentLifeLimitDimensionTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            _uow.ComponentLifeLimitDimensionTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Dimension réactivée." : "Dimension désactivée.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLookupsAsync()
        {
            var mainGroups = await _uow.AcMainGroups.GetAllAsync();
            ViewBag.AcMainGroupOptions = mainGroups
                .OrderBy(g => g.Code)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = $"{g.Code} — {g.Name}" })
                .ToList();

            // Built here (not Html.GetEnumSelectList in the view) — same
            // controller-populates-ViewBag convention every other dropdown
            // in this module already follows.
            ViewBag.UnitOptions = Enum.GetValues<ComponentLifeLimitDimensionUnit>()
                .Select(u => new SelectListItem { Value = ((int)u).ToString(), Text = u.ToString() })
                .ToList();
        }
    }
}
