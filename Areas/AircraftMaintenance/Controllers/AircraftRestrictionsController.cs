using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Policy = "MaintenanceRead")]
    public class AircraftRestrictionsController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;
        private readonly IUserScopeService  _userScopeService;
        private const int DefaultPageSize = 15;

        public AircraftRestrictionsController(
            IUnitOfWork uow, IValidationService validator, IUserScopeService userScopeService)
        {
            _uow              = uow;
            _validator        = validator;
            _userScopeService = userScopeService;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int?    searchAircraftId = null,
            string? searchType       = null,
            string? searchSeverity   = null,
            bool?   searchActive     = null,
            bool    showActiveOnly   = true,
            string  sortColumn       = "StartDate",
            string  sortDirection    = "desc",
            int     pageNumber       = 1,
            int     pageSize         = DefaultPageSize)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var aircraft  = await _uow.Aircraft.GetWhereAsync(a => a.IsActive);
            var acTypes   = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var acTypesById = acTypes.ToDictionary(t => t.Id);

            if (!scope.IsUnrestricted)
            {
                aircraft = aircraft.Where(a =>
                    a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypesById.TryGetValue(a.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId))));
            }

            // null = unrestricted (no extra filter applied below)
            var allowedAircraftIds = scope.IsUnrestricted
                ? null
                : aircraft.Select(a => a.Id).ToHashSet();

            var result = await _uow.AircraftRestrictions.GetPagedAsync(

                filter: r =>
                    (allowedAircraftIds == null || allowedAircraftIds.Contains(r.AircraftId)) &&
                    (searchAircraftId == null || r.AircraftId == searchAircraftId) &&
                    (string.IsNullOrEmpty(searchType)     || r.RestrictionType == searchType) &&
                    (string.IsNullOrEmpty(searchSeverity) || r.Severity == searchSeverity) &&
                    (!showActiveOnly || r.IsActive),

                orderBy: sortColumn switch
                {
                    "Severity"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(r => r.Severity)
                                    : q => q.OrderBy(r => r.Severity),
                    "ExpiryDate"=> sortDirection == "desc"
                                    ? q => q.OrderByDescending(r => r.ExpiryDate)
                                    : q => q.OrderBy(r => r.ExpiryDate),
                    _           => sortDirection == "desc"
                                    ? q => q.OrderByDescending(r => r.StartDate)
                                    : q => q.OrderBy(r => r.StartDate)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var typeMap   = acTypes.ToDictionary(t => t.Id, t => t.Name);
            var acMap     = aircraft.ToDictionary(a => a.Id,
                a => (dynamic)new
                {
                    a.Registration,
                    TypeName = typeMap.TryGetValue(a.AcTypeId, out var tn) ? tn : null
                });

            var certs = await _uow.AircraftCertificates
                .GetWhereAsync(c => c.IsActive);
            var certMap = certs.ToDictionary(c => c.Id, c => c.Reference);

            var allActive = await _uow.AircraftRestrictions
                .GetWhereAsync(r => r.IsActive &&
                    (allowedAircraftIds == null || allowedAircraftIds.Contains(r.AircraftId)));

            var vm = new AircraftRestrictionIndexVm
            {
                Items = result.Items.Select(r =>
                {
                    acMap.TryGetValue(r.AircraftId, out var ac);
                    return new AircraftRestrictionListVm
                    {
                        Id                   = r.Id,
                        AircraftId           = r.AircraftId,
                        AircraftRegistration = ac?.Registration ?? "—",
                        AircraftTypeName     = ac?.TypeName,
                        RestrictionType      = r.RestrictionType,
                        TypeLabel            = r.TypeLabel,
                        Severity             = r.Severity,
                        Reference            = r.Reference,
                        Description          = r.Description,
                        IssuedBy             = r.IssuedBy,
                        StartDate            = r.StartDate,
                        ExpiryDate           = r.ExpiryDate,
                        DaysRemaining        = r.DaysRemaining,
                        IsExpired            = r.IsExpired,
                        IsActive             = r.IsActive,
                        CertificateReference = r.CertificateId.HasValue &&
                                               certMap.TryGetValue(r.CertificateId.Value, out var cr)
                                                   ? cr : null
                    };
                }).ToList(),

                TotalCount      = result.TotalCount,
                TotalPages      = result.TotalPages,
                SearchAircraftId = searchAircraftId,
                SearchType       = searchType,
                SearchSeverity   = searchSeverity,
                SearchActiveOnly = showActiveOnly,
                SortColumn       = sortColumn,
                SortDirection    = sortDirection,
                PageNumber       = pageNumber,
                PageSize         = pageSize,
                CountCritical    = allActive.Count(r => r.Severity == "CRITICAL"),
                CountHigh        = allActive.Count(r => r.Severity == "HIGH"),
                AircraftOptions  = BuildAircraftOptions(aircraft, acMap, searchAircraftId)
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public async Task<IActionResult> Create(int? aircraftId = null)
        {
            if (aircraftId.HasValue && !await IsAircraftInScopeAsync(aircraftId.Value))
                return Forbid();

            var dto = new AircraftRestrictionFormDto
            {
                AircraftId = aircraftId ?? 0,
                IsActive   = true,
                StartDate  = DateOnly.FromDateTime(DateTime.Today)
            };
            if (aircraftId.HasValue)
                await FillContext(dto, aircraftId.Value);
            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(AircraftRestrictionFormDto dto)
        {
            // Defense in depth — dropdown only offers in-scope aircraft, but
            // AircraftId is still a posted value and can be tampered with.
            if (!await IsAircraftInScopeAsync(dto.AircraftId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await FillContext(dto, dto.AircraftId);
                return View(dto);
            }

            var entity = MapToEntity(dto, new AircraftRestriction
                { CreatedAt = DateTime.UtcNow,
                  CreatedByUserId = User.Identity?.Name });

            _uow.AircraftRestrictions.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Restriction '{entity.Reference}' creee.";
            return RedirectToAction(nameof(Index),
                new { searchAircraftId = entity.AircraftId });
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var entity = await _uow.AircraftRestrictions.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            var dto = MapToDto(entity);
            await FillContext(dto, entity.AircraftId);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(int id, AircraftRestrictionFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!await IsAircraftInScopeAsync(dto.AircraftId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await FillContext(dto, dto.AircraftId);
                return View(dto);
            }

            var entity = await _uow.AircraftRestrictions.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            entity.LastModifiedAt = DateTime.UtcNow;
            _uow.AircraftRestrictions.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Restriction '{entity.Reference}' modifiee.";
            return RedirectToAction(nameof(Index),
                new { searchAircraftId = entity.AircraftId });
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftRestrictions.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Introuvable." });

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            entity.IsActive       = false;
            entity.LastModifiedAt = DateTime.UtcNow;
            _uow.AircraftRestrictions.Update(entity);
            await _uow.CompleteAsync();
            return Json(new { success = true,
                message = $"Restriction '{entity.Reference}' levee." });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftRestrictions.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Introuvable." });

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            entity.IsActive       = true;
            entity.LastModifiedAt = DateTime.UtcNow;
            _uow.AircraftRestrictions.Update(entity);
            await _uow.CompleteAsync();
            return Json(new { success = true,
                message = $"Restriction '{entity.Reference}' reactive." });
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        // Mirrors the Base+AcMainGroup check used inline in SnagsController,
        // but for a single already-known AircraftId (form posts, entity
        // lookups) rather than a list being built for a dropdown.
        private async Task<bool> IsAircraftInScopeAsync(int aircraftId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;

            var aircraft = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null || !aircraft.BaseId.HasValue ||
                !scope.AllowedBaseIds.Contains(aircraft.BaseId.Value))
                return false;

            if (!scope.AllowedAcMainGroupIds.Any())
                return true;

            var acType = await _uow.AcTypes.GetByIdAsync(aircraft.AcTypeId);
            return acType != null &&
                   scope.AllowedAcMainGroupIds.Contains(acType.AcMainGroupId);
        }

        private static AircraftRestrictionFormDto MapToDto(AircraftRestriction e) =>
            new()
            {
                Id              = e.Id,
                AircraftId      = e.AircraftId,
                RestrictionType = e.RestrictionType,
                Severity        = e.Severity,
                Reference       = e.Reference,
                Description     = e.Description,
                IssuedBy        = e.IssuedBy,
                StartDate       = e.StartDate,
                ExpiryDate      = e.ExpiryDate,
                Notes           = e.Notes,
                CertificateId   = e.CertificateId,
                IsActive        = e.IsActive
            };

        private static AircraftRestriction MapToEntity(
            AircraftRestrictionFormDto dto, AircraftRestriction entity)
        {
            entity.AircraftId      = dto.AircraftId;
            entity.RestrictionType = dto.RestrictionType!;
            entity.Severity        = dto.Severity;
            entity.Reference       = dto.Reference.Trim();
            entity.Description     = dto.Description.Trim();
            entity.IssuedBy        = dto.IssuedBy?.Trim();
            entity.StartDate       = dto.StartDate;
            entity.ExpiryDate      = dto.ExpiryDate;
            entity.Notes           = dto.Notes?.Trim();
            entity.CertificateId   = dto.CertificateId;
            entity.IsActive        = dto.IsActive;
            return entity;
        }

        private async Task FillContext(AircraftRestrictionFormDto dto, int aircraftId)
        {
            var a = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (a != null)
            {
                dto.AircraftRegistration = a.Registration;
                var t = await _uow.AcTypes.GetByIdAsync(a.AcTypeId);
                dto.AircraftTypeName = t?.Name;
            }

            // Certificate DDL — only certs for this aircraft
            var certs = await _uow.AircraftCertificates
                .GetWhereAsync(c => c.AircraftId == aircraftId && c.IsActive);

            dto.CertificateOptions = new List<SelectListItem>
                {
                    new() { Value = "", Text = "— Aucun certificat lié —",
                            Selected = !dto.CertificateId.HasValue }
                }
                .Concat(certs.Select(c => new SelectListItem
                {
                    Value    = c.Id.ToString(),
                    Text     = $"{c.CertType} — {c.Reference}",
                    Selected = dto.CertificateId.HasValue &&
                               dto.CertificateId.Value == c.Id
                }));
        }

        private static IEnumerable<SelectListItem> BuildAircraftOptions(
            IEnumerable<Aircraft> list,
            Dictionary<int, dynamic> acMap,
            int? selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "— Tous les aéronefs —",
                        Selected = !selectedId.HasValue }
            };
            items.AddRange(list.OrderBy(a => a.Registration).Select(a =>
            {
                acMap.TryGetValue(a.Id, out var info);
                return new SelectListItem
                {
                    Value    = a.Id.ToString(),
                    Text     = $"{a.Registration} · {info?.TypeName ?? "?"}",
                    Selected = selectedId == a.Id
                };
            }));
            return items;
        }
    }
}
