
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]   // "Administrators" — platform convention
    public class AircraftVersionsController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public AircraftVersionsController(IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode     = null,
            string? searchName     = null,
            int?    searchAcTypeId = null,
            bool?   searchActive   = null,
            string  sortColumn     = "AcType",
            string  sortDirection  = "asc",
            int     pageNumber     = 1,
            int     pageSize       = DefaultPageSize)
        {
            var result = await _uow.AircraftVersions.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode)
                        || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (searchAcTypeId == null
                        || x.AcTypeId == searchAcTypeId) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "Code"      => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Code)
                                    : q => q.OrderBy(x => x.Code),
                    "Name"      => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Name)
                                    : q => q.OrderBy(x => x.Name),
                    "SortOrder" => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.SortOrder)
                                    : q => q.OrderBy(x => x.SortOrder),
                    "IsActive"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.IsActive)
                                    : q => q.OrderBy(x => x.IsActive),
                    _           => sortDirection == "desc"   // default: AcTypeId
                                    ? q => q.OrderByDescending(x => x.AcTypeId)
                                    : q => q.OrderBy(x => x.AcTypeId)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            // Fetch active AcTypes for name lookup + search dropdown
            var acTypes    = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var acTypeMap  = acTypes.ToDictionary(t => t.Id, t => t.Name);

            // Build AcType filter DDL for index page
            var acTypeOptions = BuildAcTypeOptions(acTypes, searchAcTypeId);

            var vm = new AircraftVersionIndexVm
            {
                Items = result.Items.Select(x => new AircraftVersionListVm
                {
                    Id          = x.Id,
                    Code        = x.Code,
                    Name        = x.Name,
                    Description = x.Description,
                    AcTypeId    = x.AcTypeId,
                    AcTypeName  = acTypeMap.TryGetValue(x.AcTypeId, out var tn)
                                    ? tn : null,
                    SortOrder   = x.SortOrder,
                    IsActive    = x.IsActive
                }).ToList(),

                TotalCount     = result.TotalCount,
                TotalPages     = result.TotalPages,
                SearchCode     = searchCode,
                SearchName     = searchName,
                SearchAcTypeId = searchAcTypeId,
                SearchActive   = searchActive,
                SortColumn     = sortColumn,
                SortDirection  = sortDirection,
                PageNumber     = pageNumber,
                PageSize       = pageSize,
                AcTypeOptions  = acTypeOptions
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var dto = new AircraftVersionFormDto { IsActive = true, SortOrder = 99 };
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftVersionFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                // Uniqueness scoped to the same AcType:
                // same Code/Name is allowed across different AcTypes.
                await _validator.CheckUniqueAsync<AircraftVersion>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<AircraftVersion>(
                        x => x.Code == code && x.AcTypeId == dto.AcTypeId,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise pour ce type."),
                    new UniqueField<AircraftVersion>(
                        x => x.Name == name && x.AcTypeId == dto.AcTypeId,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise pour ce type.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = MapToEntity(dto, new AircraftVersion());
            _uow.AircraftVersions.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version '{entity.Name}' creee avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.AircraftVersions.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            var dto = MapToDto(entity);
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftVersionFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                // Exclude current record — scoped to same AcType
                await _validator.CheckUniqueAsync<AircraftVersion>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<AircraftVersion>(
                        x => x.Code == code && x.AcTypeId == dto.AcTypeId,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise pour ce type."),
                    new UniqueField<AircraftVersion>(
                        x => x.Name == name && x.AcTypeId == dto.AcTypeId,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise pour ce type.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = await _uow.AircraftVersions.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version '{entity.Name}' modifiee avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftVersions.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Version introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactivee." });

            entity.IsActive = false;
            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version '{entity.Name}' desactivee.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftVersions.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Version introuvable." });

            entity.IsActive = true;
            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version '{entity.Name}' reactivee.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        // ── Entity → FormDto ─────────────────────────────────────────────
        private static AircraftVersionFormDto MapToDto(AircraftVersion entity) =>
            new()
            {
                Id          = entity.Id,
                AcTypeId    = entity.AcTypeId,
                Code        = entity.Code,
                Name        = entity.Name,
                Description = entity.Description,
                SortOrder   = entity.SortOrder,   // byte → int (widening, safe)
                IsActive    = entity.IsActive
            };

        // ── FormDto → Entity ─────────────────────────────────────────────
        private static AircraftVersion MapToEntity(
            AircraftVersionFormDto dto, AircraftVersion entity)
        {
            entity.AcTypeId     = dto.AcTypeId!.Value;
            entity.Code         = dto.Code.Trim().ToUpper();
            entity.Name         = dto.Name.Trim();
            entity.Description  = dto.Description?.Trim();
            entity.SortOrder    = (byte)dto.SortOrder;   // int → byte (0–255 validated)
            entity.IsActive     = dto.IsActive;
            return entity;
        }

        // ── Populate AcType dropdown on FormDto ──────────────────────────
        private async Task PopulateDropdowns(AircraftVersionFormDto dto)
        {
            var acTypes = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            dto.AcTypeOptions = BuildAcTypeOptions(acTypes, dto.AcTypeId);
        }

        // ── Shared DDL builder — used by Index and PopulateDropdowns ──────
        private static IEnumerable<SelectListItem> BuildAcTypeOptions(
            IEnumerable<AcType> acTypes, int? selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "— Tous les types —",
                        Selected = !selectedId.HasValue }
            };

            items.AddRange(
                acTypes
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value    = t.Id.ToString(),
                        Text     = t.Name,
                        Selected = selectedId.HasValue &&
                                   selectedId.Value == t.Id
                    })
            );

            return items;
        }
    }
}
