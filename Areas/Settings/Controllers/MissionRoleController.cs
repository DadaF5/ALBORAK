using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class MissionRoleController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 15;

        public MissionRoleController(IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode       = null,
            string? searchName       = null,
            int?    searchCategoryId = null,
            bool?   searchActive     = null,
            string  sortColumn       = "SortOrder",
            string  sortDirection    = "asc",
            int     pageNumber       = 1,
            int     pageSize         = DefaultPageSize)
        {
            // ── Fetch paged data with AcCategory join ────────────────────
            // GetPagedAsync returns entities — we project to VM below.
            // The join to AcCategory.Name is done in memory after paging
            // because the generic repository doesn't support joins.
            // With only 11 seed rows this is fine. For large datasets,
            // consider a custom repository method with Include().
            var result = await _uow.MissionRoles.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode)
                        || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (searchCategoryId == null
                        || x.AcCategoryId == searchCategoryId) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "Code"        => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Code)
                                        : q => q.OrderBy(x => x.Code),
                    "Name"        => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Name)
                                        : q => q.OrderBy(x => x.Name),
                    "AcCategory"  => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.AcCategoryId)
                                        : q => q.OrderBy(x => x.AcCategoryId),
                    "IsActive"    => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.IsActive)
                                        : q => q.OrderBy(x => x.IsActive),
                    _             => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.SortOrder)
                                        : q => q.OrderBy(x => x.SortOrder)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            // Fetch active categories for name lookup + search dropdown
            var categories = await _uow.AcCategories.GetWhereAsync(
                c => c.IsActive);

            var categoryMap = categories.ToDictionary(
                c => c.Id, c => c.Name);

            var vm = new MissionRoleIndexVm
            {
                Items = result.Items.Select(x => new MissionRoleListVm
                {
                    Id             = x.Id,
                    Code           = x.Code,
                    Name           = x.Name,
                    AcCategoryId   = x.AcCategoryId,
                    AcCategoryName = x.AcCategoryId.HasValue &&
                                     categoryMap.TryGetValue(x.AcCategoryId.Value, out var cn)
                                         ? cn : null,
                    SortOrder      = x.SortOrder,
                    IsActive       = x.IsActive
                }).ToList(),

                TotalCount      = result.TotalCount,
                TotalPages      = result.TotalPages,
                SearchCode      = searchCode,
                SearchName      = searchName,
                SearchCategoryId = searchCategoryId,
                SearchActive    = searchActive,
                SortColumn      = sortColumn,
                SortDirection   = sortDirection,
                PageNumber      = pageNumber,
                PageSize        = pageSize,

                // Category filter dropdown
                AcCategoryOptions = BuildCategoryOptions(categories, searchCategoryId)
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var dto = new MissionRoleFormDto { IsActive = true };
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MissionRoleFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<MissionRole>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<MissionRole>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<MissionRole>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = MapToEntity(dto, new MissionRole());
            _uow.MissionRoles.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Role '{entity.Name}' cree avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.MissionRoles.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            var dto = MapToDto(entity);
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MissionRoleFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<MissionRole>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<MissionRole>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<MissionRole>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = await _uow.MissionRoles.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.MissionRoles.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Role '{entity.Name}' modifie avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.MissionRoles.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Role introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactive." });

            entity.IsActive = false;
            _uow.MissionRoles.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Role '{entity.Name}' desactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.MissionRoles.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Role introuvable." });

            entity.IsActive = true;
            _uow.MissionRoles.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Role '{entity.Name}' reactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static MissionRoleFormDto MapToDto(MissionRole entity) =>
            new()
            {
                Id           = entity.Id,
                Code         = entity.Code,
                Name         = entity.Name,
                AcCategoryId = entity.AcCategoryId,
                SortOrder    = entity.SortOrder,
                IsActive     = entity.IsActive
            };

        private static MissionRole MapToEntity(MissionRoleFormDto dto, MissionRole entity)
        {
            entity.Code         = dto.Code.Trim().ToUpper();
            entity.Name         = dto.Name.Trim();
            entity.AcCategoryId = dto.AcCategoryId;
            entity.SortOrder    = dto.SortOrder;
            entity.IsActive     = dto.IsActive;
            return entity;
        }

        // ── PopulateDropdowns ────────────────────────────────────────────
        // Fills AcCategoryOptions on the FormDto.
        // Called on every GET and on POST validation failure.
        // "Toutes catégories" option (value="") allows null FK.
        private async Task PopulateDropdowns(MissionRoleFormDto dto)
        {
            var categories = await _uow.AcCategories
                .GetWhereAsync(c => c.IsActive);

            dto.AcCategoryOptions = BuildCategoryOptions(
                categories, dto.AcCategoryId);
        }

        // Shared builder — used by both Index and PopulateDropdowns
        private static IEnumerable<SelectListItem> BuildCategoryOptions(
            IEnumerable<AcCategory> categories,
            int?                    selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "",   Text = "— Toutes catégories —",
                        Selected = !selectedId.HasValue }
            };

            items.AddRange(
                categories
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new SelectListItem
                    {
                        Value    = c.Id.ToString(),
                        Text     = c.DisplayLabel,
                        Selected = selectedId.HasValue &&
                                   selectedId.Value == c.Id
                    })
            );

            return items;
        }
    }
}
