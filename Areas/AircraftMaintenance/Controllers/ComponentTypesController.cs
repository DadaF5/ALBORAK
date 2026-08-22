using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    // ComponentType is catalog/setup data (no aircraft instance) — Admin/setup-level,
    // same convention as JobCard/InspectionType/MaintenanceProgram. Read is opened to
    // MaintenanceRead so any scoped tech can look up part data; Write stays tighter.
    // Position-eligibility isn't AcType-scoped itself (a PN can span AcTypes), so this
    // controller doesn't apply IsAcTypeInScopeAsync per-row — flag if that's wrong for
    // your real access model.
    [Area("AircraftMaintenance")]
    public class ComponentTypesController : Controller
    {
        private readonly IComponentTypeService _service;
        private readonly IComponentLifeLimitProfileService _profiles;
        // NEW — sub-assembly slot management talks to IUnitOfWork directly
        // (no dedicated service class), same lightweight-CRUD convention
        // ComponentPositionsController already uses for its own lookup entity.
        private readonly IUnitOfWork _uow;

        public ComponentTypesController(IComponentTypeService service, IComponentLifeLimitProfileService profiles, IUnitOfWork uow)
        {
            _service = service;
            _profiles = profiles;
            _uow = uow;
        }

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            ViewBag.IncludeInactive = includeInactive;
            return View(await _service.GetAllAsync(includeInactive));
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create()
        {
            await PopulateAtaOptionsAsync();
            return View(new ComponentTypeFormDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(ComponentTypeFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAtaOptionsAsync();
                return View(dto);
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateAtaOptionsAsync();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            // Land on Edit (not Index) so the "configure life limits now" link
            // in _Form.cshtml is immediately visible for HardTime types.
            return RedirectToAction(nameof(Edit), new { id = result.Id });
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.GetForEditAsync(id);
            if (dto == null) return NotFound();
            await PopulateAtaOptionsAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(ComponentTypeFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAtaOptionsAsync();
                return View(dto);
            }

            var result = await _service.UpdateAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateAtaOptionsAsync();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // --- Lookup dropdowns ---
        // ATA: CONFIRMED this session against the real JobCardsController.cs —
        // _uow.Ata.GetAllAsync() returns Ata : LookupBase { Id, Code, Name,
        // SortOrder, IsActive } — same repo/shape JobCards' own ATA dropdown uses.
        // AircraftManufacturer: repo property (_uow.AircraftManufacturers,
        // IGenericRepository<AircraftManufacturer>) is confirmed to exist from
        // your real IUnitOfWork.cs, but Code/Name below are INFERRED from the
        // "Standard LookupBase" convention noted on AtaCategory (Id, Code, Name,
        // SortOrder, IsActive) — not directly confirmed. If this doesn't
        // compile, paste AircraftManufacturer.cs and I'll fix the property names.
        private async Task PopulateAtaOptionsAsync()
        {
            var ata = await _uow.Ata.GetAllAsync();
            ViewBag.AtaOptions = ata
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder).ThenBy(a => a.Code)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Code} — {a.Name}" })
                .ToList();

            var manufacturers = await _uow.AircraftManufacturers.GetAllAsync();
            ViewBag.AircraftManufacturerOptions = manufacturers
                .Where(m => m.IsActive)
                .OrderBy(m => m.SortOrder).ThenBy(m => m.Code)
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = $"{m.Code} — {m.Name}" })
                .ToList();
        }

        // --- Life-limit profiles (staged, S/N-resolvable schedules) ---
        // Dedicated page, same pattern as InspectionType.ManagePrograms — a
        // full staged schedule doesn't fit the basic Create/Edit form above.

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> ManageLifeLimits(int id)
        {
            var componentType = await _service.GetForEditAsync(id);
            if (componentType == null) return NotFound();

            ViewBag.ComponentTypeId = id;
            ViewBag.ComponentTypePartNumber = componentType.PartNumber;
            return View(await _profiles.GetByComponentTypeAsync(id));
        }

        /// <summary>NEW (Revision 13) — every active ComponentLifeLimitDimensionType, for the stage editor's "add a dimension" picker. Populated on every action that renders CreateProfile/EditProfile's view (initial GET and a failed POST re-render alike) — forgetting this on any of the 4 makes the picker render empty, not error, so it's easy to miss; kept as one shared helper for that reason.</summary>
        private async Task PopulateDimensionTypeOptionsAsync()
        {
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            ViewBag.DimensionTypeOptions = dimensionTypes
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .Select(d => new ComponentLifeLimitDimensionTypeOptionDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.Name,
                    Unit = d.Unit,
                    IsCalendarBased = d.IsCalendarBased
                })
                .ToList();
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateProfile(int componentTypeId)
        {
            var dto = new ComponentLifeLimitProfileFormDto { ComponentTypeId = componentTypeId };
            dto.Stages.Add(new ComponentLifeLimitStageFormDto { SequenceOrder = 1 });
            await PopulateDimensionTypeOptionsAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateProfile(ComponentLifeLimitProfileFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDimensionTypeOptionsAsync();
                return View(dto);
            }

            var result = await _profiles.SaveAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDimensionTypeOptionsAsync();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(ManageLifeLimits), new { id = dto.ComponentTypeId });
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> EditProfile(int id)
        {
            var dto = await _profiles.GetForEditAsync(id);
            if (dto == null) return NotFound();
            await PopulateDimensionTypeOptionsAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> EditProfile(ComponentLifeLimitProfileFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDimensionTypeOptionsAsync();
                return View(dto);
            }

            var result = await _profiles.SaveAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDimensionTypeOptionsAsync();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(ManageLifeLimits), new { id = dto.ComponentTypeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteProfile(int id, int componentTypeId)
        {
            var result = await _profiles.DeleteAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ManageLifeLimits), new { id = componentTypeId });
        }

        // --- NEW: Sub-assembly slots (design doc §2, "Component Installation & Hierarchical Tree") ---
        // Same dedicated-page "Manage" pattern as ManageLifeLimits above — one
        // ComponentType (as parent/host) can define several named, capacity-
        // limited slots; each slot then has its own eligible-PN list managed
        // on a nested page (ManageSlotEligibility) — split the same way
        // ManageLifeLimits/CreateProfile split "the profile" from "its stages".

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> ManageSubAssemblySlots(int id)
        {
            var componentType = await _service.GetForEditAsync(id);
            if (componentType == null) return NotFound();

            ViewBag.ComponentTypeId = id;
            ViewBag.ComponentTypePartNumber = componentType.PartNumber;
            return View(await _uow.ComponentTypeSlots.GetByParentComponentTypeAsync(id, includeInactive: true));
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public IActionResult CreateSlot(int parentComponentTypeId) =>
            View(new ComponentTypeSlotFormDto { ParentComponentTypeId = parentComponentTypeId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateSlot(ComponentTypeSlotFormDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var slotCode = dto.SlotCode.Trim().ToUpperInvariant();
            if (await _uow.ComponentTypeSlots.ExistsAsync(dto.ParentComponentTypeId, slotCode))
            {
                ModelState.AddModelError("", "Ce code d'emplacement existe déjà pour ce composant parent.");
                return View(dto);
            }

            var slot = new ComponentTypeSlot
            {
                ParentComponentTypeId = dto.ParentComponentTypeId,
                SlotCode = slotCode,
                SlotName = dto.SlotName,
                MaxCount = dto.MaxCount,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            };
            _uow.ComponentTypeSlots.Add(slot);
            await _uow.CompleteAsync(); // slot.Id populated after this

            TempData["Success"] = "Emplacement créé avec succès. Ajoutez maintenant au moins un numéro de pièce éligible.";
            return RedirectToAction(nameof(ManageSlotEligibility), new { id = slot.Id });
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> EditSlot(int id)
        {
            var entity = await _uow.ComponentTypeSlots.GetByIdAsync(id);
            if (entity == null) return NotFound();

            return View(new ComponentTypeSlotFormDto
            {
                Id = entity.Id,
                ParentComponentTypeId = entity.ParentComponentTypeId,
                SlotCode = entity.SlotCode,
                SlotName = entity.SlotName,
                MaxCount = entity.MaxCount,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> EditSlot(ComponentTypeSlotFormDto dto)
        {
            if (dto.Id is null) return NotFound();
            var entity = await _uow.ComponentTypeSlots.GetByIdAsync(dto.Id.Value);
            if (entity == null) return NotFound();

            if (!ModelState.IsValid) return View(dto);

            var slotCode = dto.SlotCode.Trim().ToUpperInvariant();
            if (await _uow.ComponentTypeSlots.ExistsAsync(dto.ParentComponentTypeId, slotCode, dto.Id))
            {
                ModelState.AddModelError("", "Ce code d'emplacement existe déjà pour ce composant parent.");
                return View(dto);
            }

            entity.SlotCode = slotCode;
            entity.SlotName = dto.SlotName;
            entity.MaxCount = dto.MaxCount;
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            _uow.ComponentTypeSlots.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Emplacement mis à jour avec succès.";
            return RedirectToAction(nameof(ManageSubAssemblySlots), new { id = entity.ParentComponentTypeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> DeleteSlot(int id, int parentComponentTypeId)
        {
            // Deleting the slot DEFINITION cascades to its eligible-PN rows at
            // the DB level (ComponentTypeSubAssemblySlotConfiguration —
            // Cascade from Slot) — no manual cleanup needed here. Does NOT
            // touch any Component already attached under it —
            // Component.CurrentSlotCode is a free-text projection, not an FK
            // to this table. Only future attaches are affected.
            var entity = await _uow.ComponentTypeSlots.GetByIdAsync(id);
            if (entity != null)
            {
                _uow.ComponentTypeSlots.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Emplacement supprimé.";
            }
            return RedirectToAction(nameof(ManageSubAssemblySlots), new { id = parentComponentTypeId });
        }

        // --- NEW: Eligible parts for one slot (which interchangeable PN(s) fit it) ---

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> ManageSlotEligibility(int id)
        {
            var slot = await _uow.ComponentTypeSlots.GetWithEligibilityAsync(id);
            if (slot == null) return NotFound();

            ViewBag.SlotId = id;
            ViewBag.SlotCode = slot.SlotCode;
            ViewBag.SlotName = slot.SlotName;
            ViewBag.MaxCount = slot.MaxCount;
            ViewBag.ParentComponentTypeId = slot.ParentComponentTypeId;
            ViewBag.ParentComponentTypePartNumber = slot.ParentComponentType?.PartNumber;
            ViewBag.ComponentTypeOptions = await _service.GetAllAsync();
            return View(slot.EligibleChildren.OrderBy(e => e.SortOrder).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> AddEligiblePart(ComponentTypeSlotEligibilityFormDto dto)
        {
            if (!ModelState.IsValid || await _uow.ComponentTypeSubAssemblySlots.ExistsAsync(dto.SlotId, dto.ChildComponentTypeId))
            {
                TempData["Error"] = "Ce numéro de pièce est déjà éligible pour cet emplacement (ou la sélection est invalide).";
                return RedirectToAction(nameof(ManageSlotEligibility), new { id = dto.SlotId });
            }

            _uow.ComponentTypeSubAssemblySlots.Add(new ComponentTypeSubAssemblySlot
            {
                SlotId = dto.SlotId,
                ChildComponentTypeId = dto.ChildComponentTypeId,
                IsActive = true
            });
            await _uow.CompleteAsync();

            TempData["Success"] = "Numéro de pièce ajouté à l'emplacement.";
            return RedirectToAction(nameof(ManageSlotEligibility), new { id = dto.SlotId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> RemoveEligiblePart(int id, int slotId)
        {
            // Same "removing the rule doesn't touch already-attached
            // Components" reasoning as DeleteSlot above.
            var entity = await _uow.ComponentTypeSubAssemblySlots.GetByIdAsync(id);
            if (entity != null)
            {
                _uow.ComponentTypeSubAssemblySlots.Delete(entity);
                await _uow.CompleteAsync();
                TempData["Success"] = "Numéro de pièce retiré de l'emplacement.";
            }
            return RedirectToAction(nameof(ManageSlotEligibility), new { id = slotId });
        }
    }
}
