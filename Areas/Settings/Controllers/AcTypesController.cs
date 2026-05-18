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
    [Authorize(Roles = "Admin")]
    public class AcTypesController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 10;

        public AcTypesController(IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchCode           = null,
            string? searchName           = null,
            int?    searchAcMainGroupId  = null,
            int?    searchManufacturerId = null,
            bool?   searchActive         = null,
            string  sortColumn           = "SortOrder",
            string  sortDirection        = "asc",
            int     pageNumber           = 1,
            int     pageSize             = DefaultPageSize)
        {
            var result = await _uow.AcTypes.GetPagedAsync(

                filter: x =>
                    (string.IsNullOrWhiteSpace(searchCode)
                        || x.Code.Contains(searchCode)) &&
                    (string.IsNullOrWhiteSpace(searchName)
                        || x.Name.Contains(searchName)) &&
                    (searchAcMainGroupId == null
                        || x.AcMainGroupId == searchAcMainGroupId) &&
                    (searchManufacturerId == null
                        || x.AircraftManufacturerId == searchManufacturerId) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "Code"         => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Code)
                                        : q => q.OrderBy(x => x.Code),
                    "Name"         => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.Name)
                                        : q => q.OrderBy(x => x.Name),
                    "AcMainGroup"  => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.AcMainGroupId)
                                        : q => q.OrderBy(x => x.AcMainGroupId),
                    "Manufacturer" => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.AircraftManufacturerId)
                                        : q => q.OrderBy(x => x.AircraftManufacturerId),
                    "IsActive"     => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.IsActive)
                                        : q => q.OrderBy(x => x.IsActive),
                    _              => sortDirection == "desc"
                                        ? q => q.OrderByDescending(x => x.SortOrder)
                                        : q => q.OrderBy(x => x.SortOrder)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            // Load FK names for display
            var groups  = await _uow.AcMainGroups
                .GetWhereAsync(g => g.IsActive);
            var mfrs    = await _uow.AircraftManufacturers
                .GetWhereAsync(m => m.IsActive);

            var groupMap = groups.ToDictionary(g => g.Id, g => g.Name);
            var mfrMap   = mfrs.ToDictionary(m => m.Id, m => m.Name);

            // Version count per AcType
            var typeIds = result.Items.Select(x => x.Id).ToList();
            var versions = await _uow.AircraftVersions
                .GetWhereAsync(v => typeIds.Contains(v.AcTypeId));
            var versionCount = versions
                .GroupBy(v => v.AcTypeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var vm = new AcTypeIndexVm
            {
                Items = result.Items.Select(x => new AcTypeListVm
                {
                    Id                    = x.Id,
                    Code                  = x.Code,
                    Name                  = x.Name,
                    Description           = x.Description,
                    AcMainGroupId         = x.AcMainGroupId,
                    AcMainGroupName       = groupMap.TryGetValue(x.AcMainGroupId, out var gn)
                                                ? gn : null,
                    AircraftManufacturerId = x.AircraftManufacturerId,
                    ManufacturerName      = x.AircraftManufacturerId.HasValue &&
                                           mfrMap.TryGetValue(x.AircraftManufacturerId.Value, out var mn)
                                                ? mn : null,
                    MaxGrossWeight        = x.MaxGrossWeight,
                    MaxEngines            = x.MaxEngines,
                    SeatCount             = x.SeatCount,
                    MaxPassengers         = x.MaxPassengers,
                    SortOrder             = x.SortOrder,
                    IsActive              = x.IsActive,
                    VersionCount          = versionCount.TryGetValue(x.Id, out var vc)
                                                ? vc : 0
                }).ToList(),

                TotalCount           = result.TotalCount,
                TotalPages           = result.TotalPages,
                SearchCode           = searchCode,
                SearchName           = searchName,
                SearchAcMainGroupId  = searchAcMainGroupId,
                SearchManufacturerId = searchManufacturerId,
                SearchActive         = searchActive,
                SortColumn           = sortColumn,
                SortDirection        = sortDirection,
                PageNumber           = pageNumber,
                PageSize             = pageSize,
                AcMainGroupOptions   = BuildGroupOptions(groups, searchAcMainGroupId),
                ManufacturerOptions  = BuildMfrOptions(mfrs, searchManufacturerId)
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            var dto = new AcTypeFormDto
            {
                IsActive     = true,
                SortOrder    = 99,
                MaxEngines   = 1,
                SeatCount    = 1,
                MaxPassengers = 0
            };
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcTypeFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                // Uniqueness scoped to same AcMainGroup
                await _validator.CheckUniqueAsync<AcType>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<AcType>(
                        x => x.Code == code &&
                             x.AcMainGroupId == dto.AcMainGroupId,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise dans ce groupe."),
                    new UniqueField<AcType>(
                        x => x.Name == name &&
                             x.AcMainGroupId == dto.AcMainGroupId,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise dans ce groupe.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = MapToEntity(dto, new AcType());
            _uow.AcTypes.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Type '{entity.Name}' cree avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.AcTypes.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            var dto = MapToDto(entity);
            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcTypeFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var code = dto.Code.Trim().ToUpper();
                var name = dto.Name.Trim();

                await _validator.CheckUniqueAsync<AcType>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<AcType>(
                        x => x.Code == code &&
                             x.AcMainGroupId == dto.AcMainGroupId,
                        nameof(dto.Code),
                        $"Le code '{code}' est deja utilise dans ce groupe."),
                    new UniqueField<AcType>(
                        x => x.Name == name &&
                             x.AcMainGroupId == dto.AcMainGroupId,
                        nameof(dto.Name),
                        $"Le nom '{name}' est deja utilise dans ce groupe.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = await _uow.AcTypes.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            _uow.AcTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Type '{entity.Name}' modifie avec succes.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AcTypes.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Type introuvable." });

            if (!entity.IsActive)
                return Json(new { success = true,
                    message = "Deja desactive." });

            // Warn if versions exist
            var hasVersions = await _uow.AircraftVersions
                .AnyAsync(v => v.AcTypeId == id && v.IsActive);

            if (hasVersions)
                return Json(new { success = false,
                    message = "Ce type possede des versions actives. " +
                              "Desactivez-les d'abord." });

            entity.IsActive = false;
            _uow.AcTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Type '{entity.Name}' desactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AcTypes.GetByIdAsync(id);

            if (entity == null)
                return Json(new { success = false,
                    message = "Type introuvable." });

            entity.IsActive = true;
            _uow.AcTypes.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] = $"Type '{entity.Name}' reactive.";
            return Json(new { success = true,
                message = TempData["SuccessMessage"] });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        private static AcTypeFormDto MapToDto(AcType entity) =>
            new()
            {
                Id                    = entity.Id,
                AcMainGroupId         = entity.AcMainGroupId,
                AircraftManufacturerId = entity.AircraftManufacturerId,
                Code                  = entity.Code,
                Name                  = entity.Name,
                Description           = entity.Description,
                SortOrder             = entity.SortOrder,   // byte → int
                IsActive              = entity.IsActive,
                MaxGrossWeight        = entity.MaxGrossWeight,
                MaxEngines            = entity.MaxEngines,
                SeatCount             = entity.SeatCount,
                MaxPassengers         = entity.MaxPassengers
            };

        private static AcType MapToEntity(AcTypeFormDto dto, AcType entity)
        {
            entity.AcMainGroupId          = dto.AcMainGroupId!.Value;
            entity.AircraftManufacturerId = dto.AircraftManufacturerId;
            entity.Code                   = dto.Code.Trim().ToUpper();
            entity.Name                   = dto.Name.Trim();
            entity.Description            = dto.Description?.Trim();
            entity.SortOrder              = (byte)dto.SortOrder;
            entity.IsActive               = dto.IsActive;
            entity.MaxGrossWeight         = dto.MaxGrossWeight;
            entity.MaxEngines             = dto.MaxEngines;
            entity.SeatCount              = dto.SeatCount;
            entity.MaxPassengers          = dto.MaxPassengers;
            return entity;
        }

        private async Task PopulateDropdowns(AcTypeFormDto dto)
        {
            var groups = await _uow.AcMainGroups
                .GetWhereAsync(g => g.IsActive);
            var mfrs   = await _uow.AircraftManufacturers
                .GetWhereAsync(m => m.IsActive);

            dto.AcMainGroupOptions   = BuildGroupOptions(groups, dto.AcMainGroupId);
            dto.ManufacturerOptions  = BuildMfrOptions(mfrs, dto.AircraftManufacturerId);
        }

        private static IEnumerable<SelectListItem> BuildGroupOptions(
            IEnumerable<AcMainGroup> groups, int? selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "- Tous les groupes -",
                        Selected = !selectedId.HasValue }
            };
            items.AddRange(
                groups.OrderBy(g => g.SortOrder).ThenBy(g => g.Name)
                    .Select(g => new SelectListItem
                    {
                        Value    = g.Id.ToString(),
                        Text     = g.DisplayLabel,
                        Selected = selectedId.HasValue && selectedId.Value == g.Id
                    }));
            return items;
        }

        private static IEnumerable<SelectListItem> BuildMfrOptions(
            IEnumerable<AircraftManufacturer> mfrs, int? selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "- Tous les constructeurs -",
                        Selected = !selectedId.HasValue }
            };
            items.AddRange(
                mfrs.OrderBy(m => m.Name)
                    .Select(m => new SelectListItem
                    {
                        Value    = m.Id.ToString(),
                        Text     = m.DisplayLabel,
                        Selected = selectedId.HasValue && selectedId.Value == m.Id
                    }));
            return items;
        }
    }
}
