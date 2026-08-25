using System.Security.Claims;
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
        // NEW (Derogation implementation pass) — same dedicated-service
        // convention as _profiles above.
        private readonly IComponentDerogationService _derogations;
        // NEW — sub-assembly slot management talks to IUnitOfWork directly
        // (no dedicated service class), same lightweight-CRUD convention
        // ComponentPositionsController already uses for its own lookup entity.
        private readonly IUnitOfWork _uow;

        public ComponentTypesController(
            IComponentTypeService service,
            IComponentLifeLimitProfileService profiles,
            IComponentDerogationService derogations,
            IUnitOfWork uow)
        {
            _service = service;
            _profiles = profiles;
            _derogations = derogations;
            _uow = uow;
        }

        // NEW (Derogation implementation pass) — same "standard Identity
        // claim, no UserManager DI needed" convention as ComponentsController.
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            // CHANGED — used to land on Edit so the "configure life limits
            // now" alert was immediately visible. That alert (and the two
            // others like it) moved off Edit onto the new Details hub page
            // (Dadda's "too many processes crammed into one view" feedback),
            // so this now lands there instead — same immediate visibility,
            // less crowded landing page.
            return RedirectToAction(nameof(Details), new { id = result.Id });
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
            // CHANGED — was Index (the full PN list). Landing back on this
            // PN's own Details hub is more useful after editing it — same
            // "return to where you came from" reasoning as the redirect
            // above.
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }

        /// <summary>
        /// NEW — read-first "hub" page for one PN, same role
        /// Components/Details.cshtml plays for a physical S/N. Replaces the
        /// three alert-box links that used to be stacked inside the Edit
        /// form (ManageLifeLimits/ManageSubAssemblySlots/ManageDerogations)
        /// — Edit is now a lean data-entry form again, and this page is
        /// where a tech actually lands to see the PN's state and jump into
        /// whichever sub-area they need. CHANGED (follow-up pass) — Positions
        /// éligibles (formerly a picker embedded in Edit) is now a fourth
        /// linked sub-area here too, via ManagePositions below, for the same
        /// "everything sub-area-shaped lives off Details, not Edit" reason.
        /// </summary>
        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _service.GetDetailsAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        // --- NEW (Details-hub-page pass, follow-up): Positions éligibles ---
        // Moved off the Create/Edit catalog form, following the exact same
        // "dedicated Manage page, linked from Details" process already used
        // for Life Limits/Sub-assembly Slots/Derogations above.

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ManagePositions(int id)
        {
            if (!await PopulateComponentTypeHeaderAsync(id)) return NotFound();
            await PopulatePositionOptionsAsync();

            var selected = await _uow.ComponentTypes.GetPositionIdsAsync(id);
            return View(new ComponentTypePositionsFormDto
            {
                ComponentTypeId = id,
                SelectedPositionIds = selected.ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ManagePositions(ComponentTypePositionsFormDto dto)
        {
            var result = await _service.UpdatePositionsAsync(dto.ComponentTypeId, dto.SelectedPositionIds);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = dto.ComponentTypeId });
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

            // CHANGED (Details-hub-page pass, follow-up) — Positions éligibles
            // moved off Create/Edit entirely, so PopulatePositionOptionsAsync
            // is no longer called from here. It's still used, just only by
            // the new ManagePositions action below.
        }

        /// <summary>
        /// CHANGED (Details-hub-page pass) — was a flat, unfiltered
        /// SelectListItem list (every AcType's positions interleaved with no
        /// way to narrow down — Dadda's live-test feedback: "the select list
        /// is not filtered (AcMainGroup, AcType)"). Now populates
        /// ViewBag.PositionOptions as the richer
        /// List&lt;ComponentPositionOptionDto&gt; so _Form.cshtml can
        /// group/filter client-side by Famille aéronef (AcMainGroup) and
        /// Type avion (AcType) — filtering is pure client-side narrowing,
        /// defaulting to "show all," so nothing is ever silently hidden the
        /// way the empty picker used to be before the original fix.
        ///
        /// AcMainGroupId/Label are resolved via a SEPARATE
        /// _uow.AcMainGroups.GetAllAsync() lookup rather than assuming an
        /// AcType.AcMainGroup navigation property — GetAllActiveWithAcTypeAsync's
        /// Include is only confirmed to load AcType itself (with its own
        /// AcMainGroupId scalar, confirmed non-nullable int — see
        /// ComponentPositionsController's IsAcTypeInScopeAsync comment), not
        /// an AcType.AcMainGroup nav chain.
        /// ASSUMPTION: AcMainGroup follows the same LookupBase convention as
        /// Ata/AircraftManufacturer (Id, Code, Name, SortOrder, IsActive) —
        /// not directly confirmed against your real AcMainGroup.cs. If this
        /// doesn't compile, paste that file and I'll fix the property names.
        /// </summary>
        private async Task PopulatePositionOptionsAsync()
        {
            var positions = await _uow.ComponentPositions.GetAllActiveWithAcTypeAsync();
            var mainGroups = await _uow.AcMainGroups.GetAllAsync();
            var mainGroupLabels = mainGroups.ToDictionary(g => g.Id, g => $"{g.Code} — {g.Name}");

            ViewBag.PositionOptions = positions
                .OrderBy(p => p.AcType?.Code).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Select(p => new ComponentPositionOptionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    AcTypeId = p.AcTypeId,
                    AcTypeLabel = p.AcType?.Code ?? "?",
                    AcMainGroupId = p.AcType?.AcMainGroupId ?? 0,
                    AcMainGroupLabel = p.AcType != null && mainGroupLabels.TryGetValue(p.AcType.AcMainGroupId, out var lbl)
                        ? lbl
                        : "—"
                })
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

        /// <summary>
        /// NEW (Revision 13) — every active ComponentLifeLimitDimensionType,
        /// for the stage editor's "add a dimension" picker. Populated on
        /// every action that renders CreateProfile/EditProfile's view
        /// (initial GET and a failed POST re-render alike) — forgetting this
        /// on any of the 4 makes the picker render empty, not error, so it's
        /// easy to miss; kept as one shared helper for that reason.
        ///
        /// CHANGED — now takes componentTypeId and filters to dimensions
        /// this PN's aircraft family can plausibly use: AcMainGroupId ==
        /// null (universal — FH/Cycles/CalendarDays/landings) UNION every
        /// AcMainGroupId GetApplicableAcMainGroupIdsAsync resolves for this
        /// PN. A ComponentType with no positions configured yet (so no
        /// AcMainGroup resolves at all) still sees every universal
        /// dimension — it just can't see family-scoped ones until at least
        /// one eligible position is set, which is the same "PN not fully
        /// configured yet" state that already limits other pickers.
        ///
        /// Also NEW — populates ReferenceBasisOptions (every active
        /// ComponentReferenceBasis) for the per-dimension-row "référence de
        /// calcul" picker. Unlike DimensionTypeOptions this is NOT scoped —
        /// all 4 basis codes are unit-agnostic and valid for any dimension
        /// (see ComponentReferenceBasis.cs).
        /// </summary>
        private async Task PopulateDimensionTypeOptionsAsync(int componentTypeId)
        {
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            var applicableAcMainGroupIds = (await _uow.ComponentTypes.GetApplicableAcMainGroupIdsAsync(componentTypeId)).ToHashSet();

            ViewBag.DimensionTypeOptions = dimensionTypes
                .Where(d => d.IsActive)
                .Where(d => !d.AcMainGroupId.HasValue || applicableAcMainGroupIds.Contains(d.AcMainGroupId.Value))
                // NEW (Derogation implementation pass) — CALENDAR_MONTHS/
                // CALENDAR_YEARS are deliberately excluded from the life-limit
                // PROFILE picker: ComponentLifeStatusCalculator still evaluates
                // every calendar-based dimension as a raw cumulative DAY count
                // (no real AddMonths/AddYears math yet — see
                // ComponentLifeLimitDimensionUnit.Months/Years doc comment).
                // Using either one on a profile stage today would silently
                // misinterpret an Interval/BandEnd entered "in months" as a
                // day count. They ARE available on the Derogation form (see
                // PopulateDerogationDimensionTypeOptionsAsync below), whose
                // Value is only stored/displayed, not run through that
                // calculator in this revision.
                .Where(d => d.Code != "CALENDAR_MONTHS" && d.Code != "CALENDAR_YEARS")
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

            var referenceBases = await _uow.ComponentReferenceBases.GetAllAsync();
            ViewBag.ReferenceBasisOptions = referenceBases
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .Select(b => new ComponentReferenceBasisOptionDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name
                })
                .ToList();
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateProfile(int componentTypeId)
        {
            var dto = new ComponentLifeLimitProfileFormDto { ComponentTypeId = componentTypeId };
            dto.Stages.Add(new ComponentLifeLimitStageFormDto { SequenceOrder = 1 });
            await PopulateDimensionTypeOptionsAsync(componentTypeId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateProfile(ComponentLifeLimitProfileFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDimensionTypeOptionsAsync(dto.ComponentTypeId);
                return View(dto);
            }

            var result = await _profiles.SaveAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDimensionTypeOptionsAsync(dto.ComponentTypeId);
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
            await PopulateDimensionTypeOptionsAsync(dto.ComponentTypeId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> EditProfile(ComponentLifeLimitProfileFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDimensionTypeOptionsAsync(dto.ComponentTypeId);
                return View(dto);
            }

            var result = await _profiles.SaveAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDimensionTypeOptionsAsync(dto.ComponentTypeId);
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

        // --- NEW (Derogation implementation pass): life-limit derogations ---
        // Same dedicated-page "Manage" pattern as ManageLifeLimits/CreateProfile
        // above. Append-only — there is deliberately no EditDerogation action;
        // corrections are a new derogation, optionally pointing back at the one
        // it corrects via SupersedesDerogationId (see ComponentDerogation.cs).

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> ManageDerogations(int id)
        {
            var componentType = await _service.GetForEditAsync(id);
            if (componentType == null) return NotFound();

            ViewBag.ComponentTypeId = id;
            ViewBag.ComponentTypePartNumber = componentType.PartNumber;
            return View(await _derogations.GetByComponentTypeAsync(id));
        }

        /// <summary>
        /// NEW — every active ComponentLifeLimitDimensionType (family-scoped,
        /// same AcMainGroup logic as PopulateDimensionTypeOptionsAsync) for
        /// the derogation form's "dimension concernée" picker. UNLIKE the
        /// profile-stage picker, this one INCLUDES CALENDAR_MONTHS/
        /// CALENDAR_YEARS — a derogation's Value is only stored/displayed in
        /// this revision, never run through ComponentLifeStatusCalculator's
        /// checkpoint-grid math, so the "raw day count" trap that picker is
        /// protecting against doesn't apply here.
        /// </summary>
        private async Task PopulateDerogationDimensionTypeOptionsAsync(int componentTypeId)
        {
            var dimensionTypes = await _uow.ComponentLifeLimitDimensionTypes.GetAllAsync();
            var applicableAcMainGroupIds = (await _uow.ComponentTypes.GetApplicableAcMainGroupIdsAsync(componentTypeId)).ToHashSet();

            ViewBag.DerogationDimensionTypeOptions = dimensionTypes
                .Where(d => d.IsActive)
                .Where(d => !d.AcMainGroupId.HasValue || applicableAcMainGroupIds.Contains(d.AcMainGroupId.Value))
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
        public async Task<IActionResult> CreateDerogation(int componentTypeId)
        {
            if (!await PopulateComponentTypeHeaderAsync(componentTypeId)) return NotFound();
            await PopulateDerogationDimensionTypeOptionsAsync(componentTypeId);
            return View(new ComponentDerogationFormDto { ComponentTypeId = componentTypeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> CreateDerogation(ComponentDerogationFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                if (!await PopulateComponentTypeHeaderAsync(dto.ComponentTypeId)) return NotFound();
                await PopulateDerogationDimensionTypeOptionsAsync(dto.ComponentTypeId);
                return View(dto);
            }

            var result = await _derogations.CreateAsync(dto, CurrentUserId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                if (!await PopulateComponentTypeHeaderAsync(dto.ComponentTypeId)) return NotFound();
                await PopulateDerogationDimensionTypeOptionsAsync(dto.ComponentTypeId);
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(ManageDerogations), new { id = dto.ComponentTypeId });
        }

        /// <summary>
        /// NEW — fixes a real UX gap Dadda hit live-testing: CreateDerogation
        /// tracked ComponentTypeId correctly under the hood (hidden field,
        /// posts back fine — the derogation WAS always linked to the right
        /// PN) but never displayed WHICH PN on the page itself, so the title
        /// just read "Nouvelle dérogation" with no context — easy to read as
        /// "this isn't linked to a PN at all." Mirrors the
        /// ViewBag.ComponentTypePartNumber convention ManageDerogations
        /// already used. Returns false (caller should NotFound()) if the
        /// ComponentTypeId doesn't resolve — same defensive check
        /// ManageDerogations does inline.
        /// </summary>
        private async Task<bool> PopulateComponentTypeHeaderAsync(int componentTypeId)
        {
            var componentType = await _service.GetForEditAsync(componentTypeId);
            if (componentType == null) return false;
            ViewBag.ComponentTypePartNumber = componentType.PartNumber;
            return true;
        }

        /// <summary>
        /// NEW — Void action. Deliberately NOT a generic Edit/Delete — the
        /// only thing this can change is IsActive/VoidReason/VoidedAtUtc/
        /// VoidedByUserId (see ComponentDerogationService.VoidAsync). Posted
        /// from a small inline reason field on ManageDerogations.cshtml, not
        /// a separate page — this is a one-field action, not a form worth a
        /// dedicated view.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> VoidDerogation(ComponentDerogationVoidDto dto)
        {
            var result = await _derogations.VoidAsync(dto, CurrentUserId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(ManageDerogations), new { id = dto.ComponentTypeId });
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
