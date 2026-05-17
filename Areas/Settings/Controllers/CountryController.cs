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
    public class CountryController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public CountryController(IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX — list with search, sort, paging ───────────────────────
        //
        // All parameters come from the query string automatically:
        // /Settings/Country?searchName=Maroc&sortColumn=IsoCode&pageNumber=2
        //
        public async Task<IActionResult> Index(
            string? searchIsoCode   = null,
            string? searchName      = null,
            string? searchContinent = null,
            bool?   searchActive    = null,
            string  sortColumn      = "Name",
            string  sortDirection   = "asc",
            int     pageNumber      = 1,
            int     pageSize        = DefaultPageSize)
        {
            // ── Build filter ─────────────────────────────────────────────
            var result = await _uow.Countries.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchIsoCode)
                        || x.IsoCode.Contains(searchIsoCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (string.IsNullOrWhiteSpace(searchContinent)
                        || (x.Continent != null && x.Continent.Contains(searchContinent))) &&
                    (searchActive == null || x.IsActive == searchActive),

                // ── Build ORDER BY ───────────────────────────────────────
                orderBy: sortColumn switch
                {
                    "IsoCode"   => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.IsoCode)
                                    : q => q.OrderBy(x => x.IsoCode),
                    "Continent" => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.Continent)
                                    : q => q.OrderBy(x => x.Continent),
                    "SortOrder" => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.SortOrder)
                                    : q => q.OrderBy(x => x.SortOrder),
                    "IsActive"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(x => x.IsActive)
                                    : q => q.OrderBy(x => x.IsActive),
                    _           => sortDirection == "desc"   // default: Name
                                    ? q => q.OrderByDescending(x => x.Name)
                                    : q => q.OrderBy(x => x.Name)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            // ── Project to ViewModel ─────────────────────────────────────
            var vm = new CountryIndexVm
            {
                Items = result.Items.Select(x => new CountryListVm
                {
                    Id        = x.Id,
                    IsoCode   = x.IsoCode,
                    Name      = x.Name,
                    Continent = x.Continent,
                    SortOrder = x.SortOrder,
                    IsActive  = x.IsActive
                }).ToList(),

                TotalCount      = result.TotalCount,
                TotalPages      = result.TotalPages,

                // Echo back state so view can rebuild links
                SearchIsoCode   = searchIsoCode,
                SearchName      = searchName,
                SearchContinent = searchContinent,
                SearchActive    = searchActive,
                SortColumn      = sortColumn,
                SortDirection   = sortDirection,
                PageNumber      = pageNumber,
                PageSize        = pageSize
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public IActionResult Create() =>
            View(new CountryFormDto { IsActive = true });

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountryFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var isoCode = dto.IsoCode.Trim().ToUpper();
                var name    = dto.Name.Trim();

                // ── Duplicate check ──────────────────────────────────────
                await _validator.CheckUniqueAsync<Country>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<Country>(
                        x => x.IsoCode == isoCode,
                        nameof(dto.IsoCode),
                        $"Le code ISO '{isoCode}' est deja utilise."),
                    new UniqueField<Country>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = MapToEntity(dto, new Country());

            _uow.Countries.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Pays '{entity.Name}' cree avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.Countries.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            return View(MapToDto(entity));
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountryFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var isoCode = dto.IsoCode.Trim().ToUpper();
                var name    = dto.Name.Trim();

                // ── Duplicate check (excludes current record) ────────────
                await _validator.CheckUniqueAsync<Country>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<Country>(
                        x => x.IsoCode == isoCode,
                        nameof(dto.IsoCode),
                        $"Le code ISO '{isoCode}' est deja utilise."),
                    new UniqueField<Country>(
                        x => x.Name == name,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise.")
                );
            }

            if (!ModelState.IsValid) return View(dto);

            var entity = await _uow.Countries.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);

            _uow.Countries.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Pays '{entity.Name}' modifie avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft (IsActive = false) ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.Countries.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Pays introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Pays deja desactive." });

            entity.IsActive = false;
            _uow.Countries.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Pays '{entity.Name}' desactive.";

            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE — reverse soft delete ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.Countries.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Pays introuvable." });

            entity.IsActive = true;
            _uow.Countries.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Pays '{entity.Name}' reactive.";

            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        // ── Entity → FormDto (used in Edit GET) ──────────────────────────
        private static CountryFormDto MapToDto(Country entity) =>
            new()
            {
                Id        = entity.Id,
                IsoCode   = entity.IsoCode,
                Name      = entity.Name,
                Continent = entity.Continent,
                SortOrder = entity.SortOrder,
                IsActive  = entity.IsActive
            };

        // ── FormDto → Entity (used in Create POST + Edit POST) ───────────
        // Returns the entity so it can be chained.
        // Always sanitises strings — never trust raw form input.
        private static Country MapToEntity(CountryFormDto dto, Country entity)
        {
            entity.IsoCode   = dto.IsoCode.Trim().ToUpper();
            entity.Name      = dto.Name.Trim();
            entity.Continent = dto.Continent?.Trim();
            entity.SortOrder = dto.SortOrder;
            entity.IsActive  = dto.IsActive;
            return entity;
        }
    }
}
