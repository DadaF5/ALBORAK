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
    public class EmployingAuthorityController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public EmployingAuthorityController(IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode    = null,
            string? searchName    = null,
            bool?   searchActive  = null,
            string  sortColumn    = "SortOrder",   // default: respect intended display order
            string  sortDirection = "asc",
            int     pageNumber    = 1,
            int     pageSize      = DefaultPageSize)
        {
            var result = await _uow.EmployingAuthorities.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode)
                        || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "Code"      => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Code)
                                    : q => q.OrderBy(x => x.Code),
                    "Name"      => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Name)
                                    : q => q.OrderBy(x => x.Name),
                    "IsActive"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.IsActive)
                                    : q => q.OrderBy(x => x.IsActive),
                    _           => sortDirection == "desc"   // default: SortOrder
                                    ? q => q.OrderByDescending(x => x.SortOrder)
                                    : q => q.OrderBy(x => x.SortOrder)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var vm = new EmployingAuthorityIndexVm
            {
                Items = result.Items.Select(x => new EmployingAuthorityListVm
                {
                    Id        = x.Id,
                    Code      = x.Code,
                    Name      = x.Name,
                    SortOrder = x.SortOrder,
                    IsActive  = x.IsActive
                }).ToList(),

                TotalCount    = result.TotalCount,
                TotalPages    = result.TotalPages,
                SearchCode    = searchCode,
                SearchName    = searchName,
                SearchActive  = searchActive,
                SortColumn    = sortColumn,
                SortDirection = sortDirection,
                PageNumber    = pageNumber,
                PageSize      = pageSize
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public IActionResult Create() =>
            View(new EmployingAuthorityFormDto { IsActive = true });

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployingAuthorityFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<EmployingAuthority>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<EmployingAuthority>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<EmployingAuthority>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = MapToEntity(dto, new EmployingAuthority());
            _uow.EmployingAuthorities.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Autorite '{entity.Name}' creée avec succés.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.EmployingAuthorities.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            return View(MapToDto(entity));
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployingAuthorityFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<EmployingAuthority>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<EmployingAuthority>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<EmployingAuthority>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = await _uow.EmployingAuthorities.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.EmployingAuthorities.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Autorite '{entity.Name}' modifiee avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft (IsActive = false) ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.EmployingAuthorities.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Autorite introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactivee." });

            entity.IsActive = false;
            _uow.EmployingAuthorities.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Autorite '{entity.Name}' desactivee.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE — reverse soft delete ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.EmployingAuthorities.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Autorite introuvable." });

            entity.IsActive = true;
            _uow.EmployingAuthorities.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Autorite '{entity.Name}' reactivee.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static EmployingAuthorityFormDto MapToDto(EmployingAuthority entity) =>
            new()
            {
                Id        = entity.Id,
                Code      = entity.Code,
                Name      = entity.Name,
                SortOrder = entity.SortOrder,
                IsActive  = entity.IsActive
            };

        private static EmployingAuthority MapToEntity(
            EmployingAuthorityFormDto dto,
            EmployingAuthority        entity)
        {
            entity.Code      = dto.Code.Trim().ToUpper();
            entity.Name      = dto.Name.Trim();
            entity.SortOrder = dto.SortOrder;
            entity.IsActive  = dto.IsActive;
            return entity;
        }
    }
}
