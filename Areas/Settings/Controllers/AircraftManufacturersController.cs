using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class AircraftManufacturersController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public AircraftManufacturersController(
            IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode    = null,
            string? searchName    = null,
            bool?   searchActive  = null,
            string  sortColumn    = "Name",
            string  sortDirection = "asc",
            int     pageNumber    = 1,
            int     pageSize      = DefaultPageSize)
        {
            var result = await _uow.AircraftManufacturers.GetPagedAsync(

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
                    "SortOrder" => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.SortOrder)
                                    : q => q.OrderBy(x => x.SortOrder),
                    "IsActive"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.IsActive)
                                    : q => q.OrderBy(x => x.IsActive),
                    _           => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Name)
                                    : q => q.OrderBy(x => x.Name)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var vm = new AircraftManufacturerIndexVm
            {
                Items = result.Items.Select(x => new AircraftManufacturerListVm
                {
                    Id          = x.Id,
                    Code        = x.Code,
                    Name        = x.Name,
                    Description = x.Description,
                    SortOrder   = x.SortOrder,
                    IsActive    = x.IsActive
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
            View(new AircraftManufacturerFormDto { IsActive = true, SortOrder = 99 });

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftManufacturerFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<AircraftManufacturer>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<AircraftManufacturer>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<AircraftManufacturer>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = MapToEntity(dto, new AircraftManufacturer());
            _uow.AircraftManufacturers.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Constructeur '{entity.Name}' cree avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.AircraftManufacturers.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            return View(MapToDto(entity));
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftManufacturerFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<AircraftManufacturer>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<AircraftManufacturer>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise."),
                    new UniqueField<AircraftManufacturer>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = await _uow.AircraftManufacturers.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.AircraftManufacturers.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Constructeur '{entity.Name}' modifie avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft (IsActive = false) ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftManufacturers.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Constructeur introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactive." });

            entity.IsActive = false;
            _uow.AircraftManufacturers.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Constructeur '{entity.Name}' desactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE — reverse soft delete ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftManufacturers.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Constructeur introuvable." });

            entity.IsActive = true;
            _uow.AircraftManufacturers.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Constructeur '{entity.Name}' reactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static AircraftManufacturerFormDto MapToDto(
            AircraftManufacturer entity) =>
            new()
            {
                Id          = entity.Id,
                Code        = entity.Code,
                Name        = entity.Name,
                Description = entity.Description,
                SortOrder   = entity.SortOrder,   // byte → int (safe)
                IsActive    = entity.IsActive
            };

        private static AircraftManufacturer MapToEntity(
            AircraftManufacturerFormDto dto, AircraftManufacturer entity)
        {
            entity.Code        = dto.Code.Trim().ToUpper();
            entity.Name        = dto.Name.Trim();
            entity.Description = dto.Description?.Trim();
            entity.SortOrder   = (byte)dto.SortOrder;   // int → byte (0–255 validated)
            entity.IsActive    = dto.IsActive;
            return entity;
        }
    }
}
