using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services; // IUserScopeService

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class ComponentPositionsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserScopeService _userScope;

        public ComponentPositionsController(IUnitOfWork uow, IUserScopeService userScope)
        {
            _uow = uow;
            _userScope = userScope;
        }

        /// <summary>
        /// STOPGAP — the real IUserScopeService has no IsAcTypeInScopeAsync
        /// (confirmed this session: it exposes only GetScopeAsync, returning
        /// UserScope{IsUnrestricted, AllowedBaseIds, AllowedAcMainGroupIds,
        /// AllowedWingIds}). Reproducing "is this AcType in scope" from that
        /// needs AcType's real FK path to Base/AcMainGroup, which hasn't been
        /// shared yet. Until then this fails CLOSED: unrestricted (Admin/
        /// Base-Admin-everywhere) users pass, everyone else is denied — safe
        /// default, but means restricted users currently can't manage
        /// ComponentPositions at all. Fix by resolving AcType -> AcMainGroup
        /// -> Base (or AcType -> Base directly, whichever your schema uses)
        /// and checking scope.AllowedBaseIds/AllowedAcMainGroupIds, same
        /// pattern as ComponentScopeHelper.IsComponentInScopeAsync.
        /// </summary>
        private async Task<bool> IsAcTypeInScopeAsync(int acTypeId)
        {
            var scope = await _userScope.GetScopeAsync(User, "MAINTENANCE");
            return scope.IsUnrestricted;
        }

        /// <summary>
        /// CHANGED — fixed a real bug confirmed live via Dadda's screenshot:
        /// the "Type d'aéronef" column rendered blank for every row. Was
        /// fetching via the base GetAllAsync() (no Include), and this app
        /// doesn't use lazy-loading proxies, so p.AcType was always null
        /// here. Now uses GetAllWithAcTypeAndAtaAsync() (also fixes the
        /// same latent gap in the ATA column, and in Tree.cshtml below).
        ///
        /// CHANGED (follow-up) — per Dadda's explicit request ("still needs
        /// to be filtered by Acmaingroup then Actype as we did in the
        /// previous model"), Index.cshtml now gets the exact same
        /// client-side Famille aéronef / Type avion filter already used on
        /// the ComponentType Positions picker (_Form.cshtml/
        /// ManagePositions.cshtml): two <select>s, no page reload, plain JS
        /// narrowing which rows are visible. That means no acMainGroupId/
        /// acTypeId query params here — the full scoped/includeInactive-
        /// filtered list is still sent to the view every time (same as
        /// before this change), and AcMainGroup/AcType narrowing happens
        /// entirely in the browser. AcMainGroupLabels below is the one bit
        /// the view can't derive from Model alone (ComponentPosition ->
        /// AcType carries AcMainGroupId as a plain FK, not a loaded
        /// AcMainGroup nav/label) — same "no virtual, explicit Include
        /// only" rule as AcType/Ata already worked around above.
        /// </summary>
        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            var all = await _uow.ComponentPositions.GetAllWithAcTypeAndAtaAsync();
            var scoped = new List<Models.ComponentPosition>();
            foreach (var p in all)
            {
                if (!includeInactive && !p.IsActive) continue;
                if (!await IsAcTypeInScopeAsync(p.AcTypeId)) continue;
                scoped.Add(p);
            }

            var mainGroups = await _uow.AcMainGroups.GetAllAsync();
            ViewBag.AcMainGroupLabels = mainGroups.ToDictionary(g => g.Id, g => $"{g.Code} — {g.Name}");
            ViewBag.IncludeInactive = includeInactive;
            return View(scoped);
        }

        /// <summary>
        /// NEW — "how do I define the aircraft tree" answer: AcMainGroup
        /// (famille) and AcType (type avion) are NOT managed here — confirmed
        /// with Dadda they already have their own CRUD in the Réglages
        /// module, so this deliberately does not duplicate that. This page
        /// only reads the existing AcMainGroup -> AcType structure to use as
        /// navigation, and gives full CRUD (add/edit/deactivate) at the
        /// Position leaf level — same underlying Create/Edit actions and
        /// ComponentPosition entity Index.cshtml already uses, just grouped
        /// as a tree instead of one flat table so "which positions exist for
        /// this type" reads as one glance instead of a filter/scroll.
        ///
        /// Same AcType-level RBAC stopgap as Index (IsAcTypeInScopeAsync) —
        /// an AcMainGroup with zero in-scope AcTypes is skipped entirely
        /// rather than shown empty.
        /// </summary>
        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Tree(bool includeInactive = false)
        {
            var mainGroups = await _uow.AcMainGroups.GetAllAsync();
            var acTypes = await _uow.AcTypes.GetAllAsync();
            // CHANGED — same AcType/Ata blank-column bug as Index, fixed the
            // same way (see GetAllWithAcTypeAndAtaAsync's doc comment).
            var positions = await _uow.ComponentPositions.GetAllWithAcTypeAndAtaAsync();

            var tree = new List<AcMainGroupTreeNodeDto>();
            foreach (var mg in mainGroups.OrderBy(g => g.Code))
            {
                var typeNodes = new List<AcTypePositionsNodeDto>();
                foreach (var at in acTypes.Where(a => a.AcMainGroupId == mg.Id).OrderBy(a => a.Code))
                {
                    if (!await IsAcTypeInScopeAsync(at.Id)) continue;

                    var typePositions = positions
                        .Where(p => p.AcTypeId == at.Id)
                        .Where(p => includeInactive || p.IsActive)
                        .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
                        .ToList();

                    typeNodes.Add(new AcTypePositionsNodeDto
                    {
                        AcTypeId = at.Id,
                        AcTypeLabel = $"{at.Code} — {at.Name}",
                        Positions = typePositions
                    });
                }

                if (typeNodes.Any())
                {
                    tree.Add(new AcMainGroupTreeNodeDto
                    {
                        AcMainGroupId = mg.Id,
                        AcMainGroupLabel = $"{mg.Code} — {mg.Name}",
                        AcTypes = typeNodes
                    });
                }
            }

            ViewBag.IncludeInactive = includeInactive;
            return View(tree);
        }

        /// <summary>
        /// NEW — quick activate/deactivate straight from the tree, without a
        /// full Edit round trip. Deliberately NOT a hard Delete: same
        /// "referenced by history, soft-delete via IsActive" convention as
        /// every other catalog entity in this module (ComponentType,
        /// ComponentTypeSlot eligibility, etc.) — a Position that already has
        /// Components installed against it, or is referenced by a
        /// ComponentType's EligiblePositions, can't be safely hard-deleted.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> ToggleActive(int id, bool includeInactive)
        {
            var entity = await _uow.ComponentPositions.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (!await IsAcTypeInScopeAsync(entity.AcTypeId)) return Forbid();

            entity.IsActive = !entity.IsActive;
            _uow.ComponentPositions.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = entity.IsActive ? "Position réactivée." : "Position désactivée.";
            return RedirectToAction(nameof(Tree), new { includeInactive });
        }

        /// <summary>
        /// CHANGED — optional acTypeId query param so the new Tree view's
        /// "+ Ajouter une position" button (under a given AcType node) can
        /// land here with that AcType already selected, same convenience-
        /// prefill pattern as Components/Receipt's componentTypeId param.
        /// </summary>
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(int? acTypeId = null)
        {
            await PopulateLookupsAsync();
            return View(new ComponentPositionFormDto { AcTypeId = acTypeId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(ComponentPositionFormDto dto)
        {
            if (!await IsAcTypeInScopeAsync(dto.AcTypeId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(dto);
            }

            if (await _uow.ComponentPositions.ExistsByCodeAsync(dto.AcTypeId, dto.Code))
            {
                ModelState.AddModelError("", "Ce code existe déjà pour ce type d'aéronef.");
                await PopulateLookupsAsync();
                return View(dto);
            }

            _uow.ComponentPositions.Add(new Models.ComponentPosition
            {
                AcTypeId = dto.AcTypeId,
                Code = dto.Code.ToUpperInvariant(),
                Name = dto.Name,
                AtaId = dto.AtaId,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            });
            await _uow.CompleteAsync();

            TempData["Success"] = "Position créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _uow.ComponentPositions.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (!await IsAcTypeInScopeAsync(entity.AcTypeId)) return Forbid();

            await PopulateLookupsAsync();
            return View(new ComponentPositionFormDto
            {
                Id = entity.Id,
                AcTypeId = entity.AcTypeId,
                Code = entity.Code,
                Name = entity.Name,
                AtaId = entity.AtaId,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(ComponentPositionFormDto dto)
        {
            if (dto.Id is null) return NotFound();
            var entity = await _uow.ComponentPositions.GetByIdAsync(dto.Id.Value);
            if (entity == null) return NotFound();
            if (!await IsAcTypeInScopeAsync(dto.AcTypeId)) return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(dto);
            }

            if (await _uow.ComponentPositions.ExistsByCodeAsync(dto.AcTypeId, dto.Code, dto.Id))
            {
                ModelState.AddModelError("", "Ce code existe déjà pour ce type d'aéronef.");
                await PopulateLookupsAsync();
                return View(dto);
            }

            entity.AcTypeId = dto.AcTypeId;
            entity.Code = dto.Code.ToUpperInvariant();
            entity.Name = dto.Name;
            entity.AtaId = dto.AtaId;
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            _uow.ComponentPositions.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Position mise à jour avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // --- Lookup dropdowns ---
        // CONFIRMED this session against the real JobCardsController.cs:
        // _uow.AcTypes.GetAllAsync() (Code, Name, AcMainGroupId) and
        // _uow.Ata.GetAllAsync() (Ata : LookupBase{Id,Code,Name,SortOrder,IsActive})
        // — same repos/shapes JobCards' own dropdowns use. Not scope-filtered
        // here (unlike JobCards' AcType list) — ComponentPositions' RBAC
        // scoping is still the documented Revision-4 stopgap (fails closed at
        // POST time via IsAcTypeInScopeAsync above; this list just shows every
        // AcType so an unrestricted/Admin user can actually pick one).
        private async Task PopulateLookupsAsync()
        {
            var acTypes = await _uow.AcTypes.GetAllAsync();
            ViewBag.AcTypeOptions = acTypes
                .OrderBy(a => a.Code)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Code} — {a.Name}" })
                .ToList();

            var ata = await _uow.Ata.GetAllAsync();
            ViewBag.AtaOptions = ata
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder).ThenBy(a => a.Code)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Code} — {a.Name}" })
                .ToList();
        }
    }
}
