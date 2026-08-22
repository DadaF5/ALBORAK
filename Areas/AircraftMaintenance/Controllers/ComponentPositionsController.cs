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

        [Authorize(Policy = "MaintenanceRead")]
        public async Task<IActionResult> Index(int? acTypeId, bool includeInactive = false)
        {
            var all = await _uow.ComponentPositions.GetAllAsync();
            var scoped = new List<Models.ComponentPosition>();
            foreach (var p in all)
            {
                if (!includeInactive && !p.IsActive) continue;
                if (acTypeId.HasValue && p.AcTypeId != acTypeId.Value) continue;
                if (!await IsAcTypeInScopeAsync(p.AcTypeId)) continue;
                scoped.Add(p);
            }

            ViewBag.IncludeInactive = includeInactive;
            ViewBag.AcTypeId = acTypeId;
            return View(scoped);
        }

        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsAsync();
            return View(new ComponentPositionFormDto());
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
