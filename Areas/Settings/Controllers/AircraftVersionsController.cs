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
    public class AircraftVersionsController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        public AircraftVersionsController(IUnitOfWork uow, IValidationService validator)
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
            int     pageSize      = 10)
        {
            var result = await _uow.AircraftVersions.GetPagedAsync(
                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode) || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName) || x.Name.Contains(searchName)) &&
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

            var vm = new AircraftVersionIndexVm
            {
                Items = result.Items.Select(x => new AircraftVersionListVm
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
            View(new AircraftVersionFormDto { IsActive = true });

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftVersionFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                // ── Reusable duplicate check ─────────────────────────────
                // Works exactly like your WebForms helper:
                //   - pass the fields you want checked
                //   - each gets its own ModelState error
                //   - excludeId = null means Create (check all rows)
                await _validator.CheckUniqueAsync<AircraftVersion>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<AircraftVersion>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code «{code}» est déjà utilisé."),
                    new UniqueField<AircraftVersion>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom «{name}» est déjà utilisé.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = new AircraftVersion
            {
                Code      = dto.Code.Trim().ToUpper(),
                Name      = dto.Name.Trim(),
                SortOrder = (byte)dto.SortOrder,
                IsActive  = dto.IsActive
            };

            _uow.AircraftVersions.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version «{entity.Name}» créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.AircraftVersions.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            return View(new AircraftVersionFormDto
            {
                Id        = entity.Id,
                Code      = entity.Code,
                Name      = entity.Name,
                SortOrder = entity.SortOrder,
                IsActive  = entity.IsActive
            });
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

                // ── Reusable duplicate check ─────────────────────────────
                // Identical call to Create — only excludeId differs.
                // The service automatically appends "AND Id != excludeId"
                // to each predicate in SQL — no extra code needed here.
                await _validator.CheckUniqueAsync<AircraftVersion>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<AircraftVersion>(
                        x => x.Code == code,
                        nameof(dto.Code),
                        $"Le code «{code}» est déjà utilisé."),
                    new UniqueField<AircraftVersion>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom «{name}» est déjà utilisé.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = await _uow.AircraftVersions.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.Code      = dto.Code.Trim().ToUpper();
            entity.Name      = dto.Name.Trim();
            entity.SortOrder = (byte)dto.SortOrder;
            entity.IsActive  = dto.IsActive;

            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Version «{entity.Name}» modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE (soft) ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftVersions.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false, message = "Version introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true, message = "Déjà désactivée." });

            entity.IsActive = false;
            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            return Json(new { success = true,
                message = $"Version «{entity.Name}» désactivée." });
        }

        // ── ACTIVATE ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftVersions.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false, message = "Version introuvable." });

            entity.IsActive = true;
            _uow.AircraftVersions.Update(entity);
            await _uow.CompleteAsync();

            return Json(new { success = true,
                message = $"Version «{entity.Name}» réactivée." });
        }
    }
}
