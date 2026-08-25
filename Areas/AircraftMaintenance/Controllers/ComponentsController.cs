using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class ComponentsController : Controller
    {
        private readonly IComponentService _service;
        private readonly IComponentScopeHelper _scopeHelper;
        // NEW — only needed to populate the ComponentType dropdown on Receipt.
        // Already registered in Program.cs (Revision 10 DI fix), so this is a
        // free addition, no new registration needed.
        private readonly IComponentTypeService _componentTypes;
        // NEW — only needed to populate the Base dropdown on Receipt/Remove
        // (_uow.Bases, confirmed to exist on your real IUnitOfWork.cs).
        private readonly IUnitOfWork _uow;

        public ComponentsController(IComponentService service, IComponentScopeHelper scopeHelper, IComponentTypeService componentTypes, IUnitOfWork uow)
        {
            _service = service;
            _scopeHelper = scopeHelper;
            _componentTypes = componentTypes;
            _uow = uow;
        }

        // Standard Identity claim — no UserManager DI needed. If your app stamps
        // a different claim type for the user id, adjust here only.
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("No authenticated user id claim found.");

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            ViewBag.IncludeInactive = includeInactive;
            return View(await _service.GetScopedListAsync(User, includeInactive));
        }

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Details(int id)
        {
            var component = await _service.GetWithDetailsAsync(id);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            ViewBag.History = await _service.GetHistoryAsync(id);
            ViewBag.SlotStatus = await _service.GetSlotStatusAsync(id); // NEW — per-slot readiness breakdown
            return View(component);
        }

        // --- Receipt (new part into stock) ---

        /// <summary>
        /// CHANGED — optional componentTypeId query param so the new
        /// ComponentTypes/Details hub's "Ajouter un S/N" link can land here
        /// with the PN already selected, instead of the tech having to find
        /// it again in the dropdown. Purely a convenience pre-fill — 0/absent
        /// behaves exactly as before (nothing selected).
        /// </summary>
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Receipt(int? componentTypeId = null)
        {
            await PopulateComponentTypeOptionsAsync();
            await PopulateBaseOptionsAsync();
            var dto = new ComponentReceiptDto
            {
                ComponentTypeId = componentTypeId ?? 0,
                InitialValues = await BuildEmptyInitialValuesAsync()
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Receipt(ComponentReceiptDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateComponentTypeOptionsAsync();
                await PopulateBaseOptionsAsync();
                await PopulateInitialValueDisplayFieldsAsync(dto);
                return View(dto);
            }

            var result = await _service.ReceiptAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateComponentTypeOptionsAsync();
                await PopulateBaseOptionsAsync();
                await PopulateInitialValueDisplayFieldsAsync(dto);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }

        // --- Lookup dropdowns ---
        // ComponentType is this module's own catalog data — no external
        // lookup shape needed, IComponentTypeService.GetAllAsync() was
        // already confirmed/working.
        // Base: repo property (_uow.Bases, IGenericRepository<Base>) confirmed
        // to exist from your real IUnitOfWork.cs, but Code/Name below are
        // INFERRED from the "Standard LookupBase" convention (same caveat as
        // AircraftManufacturer in ComponentTypesController) — not directly
        // confirmed. If this doesn't compile, paste Base.cs and I'll fix it.
        private async Task PopulateComponentTypeOptionsAsync()
        {
            var types = await _componentTypes.GetAllAsync();
            ViewBag.ComponentTypeOptions = types
                .OrderBy(t => t.PartNumber)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.PartNumber} — {t.Nomenclature}" })
                .ToList();
        }

        private async Task PopulateBaseOptionsAsync()
        {
            var bases = await _uow.Bases.GetAllAsync();
            ViewBag.BaseOptions = bases
                .Where(b => b.IsActive)
                // FIXED — real Base has no SortOrder field (confirmed by a live
                // build error; Revision 11 guessed it existed under the
                // "Standard LookupBase" convention along with Code/Name, which
                // was wrong for this specific field). Ordering by Code alone
                // until/unless Base turns out to have a different real
                // display-order field.
                .OrderBy(b => b.BaseCode)
                // FIXED — real Base uses BaseName, not Name (confirmed by
                // Dadda against the real solution — Revision 13a still had
                // this wrong even after the SortOrder fix above).
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = $"{b.BaseCode} — {b.BaseName}" })
                .ToList();
        }

        /// <summary>NEW (Revision 13) — one empty row per active non-calendar ComponentLifeLimitDimensionType, so the Receipt form always shows every dimension the receiving tech might need to fill in (CALENDAR_DAYS deliberately excluded — see ComponentInitialReading's doc comment).</summary>
        private async Task<List<ComponentInitialReadingValueFormDto>> BuildEmptyInitialValuesAsync()
        {
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            return dimensionTypes
                .Where(d => d.IsActive && !d.IsCalendarBased)
                .OrderBy(d => d.SortOrder)
                .Select(d => new ComponentInitialReadingValueFormDto
                {
                    DimensionTypeId = d.Id,
                    DimensionTypeCode = d.Code,
                    DimensionTypeName = d.Name,
                    Unit = d.Unit
                })
                .ToList();
        }

        /// <summary>NEW (Revision 13) — re-attaches DimensionTypeCode/Name/Unit (display-only, never posted — see ComponentInitialReadingValueFormDto) onto a posted dto before re-rendering the Receipt form after a validation failure, so the dimension rows don't show up blank.</summary>
        private async Task PopulateInitialValueDisplayFieldsAsync(ComponentReceiptDto dto)
        {
            var dimensionTypesById = (await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync()).ToDictionary(d => d.Id);
            foreach (var v in dto.InitialValues)
            {
                if (dimensionTypesById.TryGetValue(v.DimensionTypeId, out var d))
                {
                    v.DimensionTypeCode = d.Code;
                    v.DimensionTypeName = d.Name;
                    v.Unit = d.Unit;
                }
            }
        }

        // --- Install ---

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Install(int componentId)
        {
            var component = await _service.GetWithDetailsAsync(componentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            return View(new ComponentInstallDto { ComponentId = componentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Install(ComponentInstallDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            // Scope check on the DESTINATION aircraft, not just the component's
            // current (stock) location — a write to that aircraft must also be
            // in scope for this user.
            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            var result = await _service.InstallAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- Remove ---

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Remove(int componentId)
        {
            var component = await _service.GetWithDetailsAsync(componentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            await PopulateBaseOptionsAsync();
            return View(new ComponentRemoveDto { ComponentId = componentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Remove(ComponentRemoveDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBaseOptionsAsync();
                return View(dto);
            }

            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            var result = await _service.RemoveAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateBaseOptionsAsync();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- NEW: Attach to parent (sub-assembly hierarchy, design doc §2) ---

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> AttachToParent(int componentId)
        {
            var component = await _service.GetWithDetailsAsync(componentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            return View(new ComponentAttachToParentDto { ComponentId = componentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> AttachToParent(ComponentAttachToParentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            // Scope check on the PARENT too — same "check the destination" rule as Install's aircraft check.
            var parent = await _service.GetWithDetailsAsync(dto.ParentComponentId);
            if (parent == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, parent)) return Forbid();

            var result = await _service.AttachToParentAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- NEW: Detach from parent ---

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DetachFromParent(int componentId)
        {
            var component = await _service.GetWithDetailsAsync(componentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            return View(new ComponentDetachFromParentDto { ComponentId = componentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DetachFromParent(ComponentDetachFromParentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            var result = await _service.DetachFromParentAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- Overhaul ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Overhaul(ComponentOverhaulDto dto)
        {
            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            var result = await _service.OverhaulAsync(dto, CurrentUserId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- Scrap ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Scrap(ComponentScrapDto dto)
        {
            var component = await _service.GetWithDetailsAsync(dto.ComponentId);
            if (component == null) return NotFound();
            if (!await _scopeHelper.IsComponentInScopeAsync(User, component)) return Forbid();

            var result = await _service.ScrapAsync(dto, CurrentUserId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentId });
        }

        // --- Due list (until/unless unified into the existing DueList/DamDashboard, see integration guide) ---

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> DueList()
        {
            return View(await _service.GetDueOrOverdueAsync());
        }
    }
}
