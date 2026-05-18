using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class ImmatriculationDocTypeController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public ImmatriculationDocTypeController(
            IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode       = null,
            string? searchName       = null,
            bool?   searchIsRequired = null,
            bool?   searchActive     = null,
            string  sortColumn       = "SortOrder",
            string  sortDirection    = "asc",
            int     pageNumber       = 1,
            int     pageSize         = DefaultPageSize)
        {
            var result = await _uow.ImmatriculationDocTypes.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode)
                        || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (searchIsRequired == null
                        || x.IsRequired == searchIsRequired) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "Code"       => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Code)
                                        : q => q.OrderBy(x => x.Code),
                    "Name"       => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Name)
                                        : q => q.OrderBy(x => x.Name),
                    "IsRequired" => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.IsRequired)
                                        : q => q.OrderBy(x => x.IsRequired),
                    "IsActive"   => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.IsActive)
                                        : q => q.OrderBy(x => x.IsActive),
                    _            => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.SortOrder)
                                        : q => q.OrderBy(x => x.SortOrder)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var vm = new ImmatriculationDocTypeIndexVm
            {
                Items = result.Items.Select(x => new ImmatriculationDocTypeListVm
                {
                    Id               = x.Id,
                    Code             = x.Code,
                    Name             = x.Name,
                    ArticleReference = x.ArticleReference,
                    IsRequired       = x.IsRequired,
                    AcceptedFormats  = x.AcceptedFormats,
                    MaxFileSizeMb    = x.MaxFileSizeMb,
                    SortOrder        = x.SortOrder,
                    IsActive         = x.IsActive
                }).ToList(),

                TotalCount       = result.TotalCount,
                TotalPages       = result.TotalPages,
                SearchCode       = searchCode,
                SearchName       = searchName,
                SearchIsRequired = searchIsRequired,
                SearchActive     = searchActive,
                SortColumn       = sortColumn,
                SortDirection    = sortDirection,
                PageNumber       = pageNumber,
                PageSize         = pageSize
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public IActionResult Create() =>
            View(new ImmatriculationDocTypeFormDto
            {
                IsActive      = true,
                IsRequired    = false,
                MaxFileSizeMb = 10
            });

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImmatriculationDocTypeFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<ImmatriculationDocType>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<ImmatriculationDocType>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<ImmatriculationDocType>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = MapToEntity(dto, new ImmatriculationDocType());
            _uow.ImmatriculationDocTypes.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Type de document '{entity.Code}' cree avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.ImmatriculationDocTypes.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            return View(MapToDto(entity));
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ImmatriculationDocTypeFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<ImmatriculationDocType>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<ImmatriculationDocType>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<ImmatriculationDocType>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = await _uow.ImmatriculationDocTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.ImmatriculationDocTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Type de document '{entity.Code}' modifie avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        // Note: These rows are legally mandated by DAM regulation.
        // Soft delete only — never hard delete.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.ImmatriculationDocTypes.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Type introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactive." });

            entity.IsActive = false;
            _uow.ImmatriculationDocTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Type '{entity.Code}' desactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.ImmatriculationDocTypes.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Type introuvable." });

            entity.IsActive = true;
            _uow.ImmatriculationDocTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Type '{entity.Code}' reactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static ImmatriculationDocTypeFormDto MapToDto(
            ImmatriculationDocType entity) =>
            new()
            {
                Id               = entity.Id,
                Code             = entity.Code,
                Name             = entity.Name,
                ArticleReference = entity.ArticleReference,
                IsRequired       = entity.IsRequired,
                AcceptedFormats  = entity.AcceptedFormats,
                MaxFileSizeMb    = entity.MaxFileSizeMb,
                SortOrder        = entity.SortOrder,
                IsActive         = entity.IsActive
            };

        private static ImmatriculationDocType MapToEntity(
            ImmatriculationDocTypeFormDto dto,
            ImmatriculationDocType        entity)
        {
            entity.Code             = dto.Code.Trim().ToUpper();
            entity.Name             = dto.Name.Trim();
            entity.ArticleReference = dto.ArticleReference?.Trim();
            entity.IsRequired       = dto.IsRequired;
            entity.AcceptedFormats  = dto.AcceptedFormats?.Trim().ToUpper();
            entity.MaxFileSizeMb    = dto.MaxFileSizeMb;
            entity.SortOrder        = dto.SortOrder;
            entity.IsActive         = dto.IsActive;
            return entity;
        }
    }
}
