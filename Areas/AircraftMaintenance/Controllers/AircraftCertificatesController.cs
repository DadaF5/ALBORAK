using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.ViewModels;
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
    public class AircraftCertificatesController : Controller
    {
        private const string ModuleCode = "MAINTENANCE";

        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;
        private readonly IUserScopeService  _userScopeService;

        private const string UploadRoot =
            @"D:\2BAFRA\Uploads\Certificates\";

        private const int DefaultPageSize = 15;

        public AircraftCertificatesController(
            IUnitOfWork uow, IValidationService validator, IUserScopeService userScopeService)
        {
            _uow              = uow;
            _validator        = validator;
            _userScopeService = userScopeService;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int?   searchAircraftId  = null,
            string? searchCertType   = null,
            bool?   searchActive     = null,
            bool    searchExpiringSoon = false,
            string  sortColumn       = "ExpiryDate",
            string  sortDirection    = "asc",
            int     pageNumber       = 1,
            int     pageSize         = DefaultPageSize)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Aircraft + type name map — scoped first, since it's reused both
            // for the dropdown and for restricting which certificates can
            // even appear in the paged result below.
            var aircraftList = await _uow.Aircraft.GetWhereAsync(a => a.IsActive);
            var acTypes = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var typeMap = acTypes.ToDictionary(t => t.Id, t => t.Name);
            var acTypesById = acTypes.ToDictionary(t => t.Id);

            if (!scope.IsUnrestricted)
            {
                aircraftList = aircraftList.Where(a =>
                    a.BaseId.HasValue && scope.AllowedBaseIds.Contains(a.BaseId.Value)
                    && (!scope.AllowedAcMainGroupIds.Any()
                        || (acTypesById.TryGetValue(a.AcTypeId, out var t)
                            && scope.AllowedAcMainGroupIds.Contains(t.AcMainGroupId))));
            }

            // null = unrestricted (no extra filter applied below)
            var allowedAircraftIds = scope.IsUnrestricted
                ? null
                : aircraftList.Select(a => a.Id).ToHashSet();

            var result = await _uow.AircraftCertificates.GetPagedAsync(

                filter: c =>
                    (allowedAircraftIds == null || allowedAircraftIds.Contains(c.AircraftId)) &&
                    (searchAircraftId == null
                        || c.AircraftId == searchAircraftId) &&
                    (string.IsNullOrEmpty(searchCertType)
                        || c.CertType == searchCertType) &&
                    (searchActive == null || c.IsActive == searchActive) &&
                    (!searchExpiringSoon ||
                        (c.ExpiryDate.HasValue &&
                         c.ExpiryDate.Value >= today &&
                         c.ExpiryDate.Value <= today.AddDays(30))),

                orderBy: sortColumn switch
                {
                    "CertType"   => sortDirection == "desc"
                                    ? q => q.OrderByDescending(c => c.CertType)
                                    : q => q.OrderBy(c => c.CertType),
                    "Reference"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(c => c.Reference)
                                    : q => q.OrderBy(c => c.Reference),
                    "IssueDate"  => sortDirection == "desc"
                                    ? q => q.OrderByDescending(c => c.IssueDate)
                                    : q => q.OrderBy(c => c.IssueDate),
                    _            => sortDirection == "desc"
                                    ? q => q.OrderByDescending(c => c.ExpiryDate)
                                    : q => q.OrderBy(c => c.ExpiryDate)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var acMap = aircraftList.ToDictionary(
                a => a.Id,
                a => (dynamic)new
                {
                    a.Registration,
                    Tail     = a.TailNumber.ToString(),
                    TypeName = typeMap.TryGetValue(a.AcTypeId, out var tn)
                                   ? tn : null
                });

            // Summary counts — all active certs the user is allowed to see
            var allActive = await _uow.AircraftCertificates
                .GetWhereAsync(c => c.IsActive &&
                    (allowedAircraftIds == null || allowedAircraftIds.Contains(c.AircraftId)));

            var vm = new AircraftCertificateIndexVm
            {
                Items = result.Items.Select(c =>
                {
                    acMap.TryGetValue(c.AircraftId, out var ac);
                    return new AircraftCertificateListVm
                    {
                        Id               = c.Id,
                        AircraftId       = c.AircraftId,
                        AircraftCode     = ac?.Registration     ?? "—",
                        AircraftTail     = ac?.Tail     ?? "—",
                        AircraftTypeName = ac?.TypeName,
                        CertType         = c.CertType,
                        CertTypeLabel    = c.CertTypeLabel,
                        Reference        = c.Reference,
                        IssuingAuthority = c.IssuingAuthority,
                        IssueDate        = c.IssueDate,
                        ExpiryDate       = c.ExpiryDate,
                        HasDocument      = !string.IsNullOrEmpty(c.DocumentPath),
                        IsActive         = c.IsActive,
                        DaysRemaining    = c.DaysRemaining,
                        StatusLabel      = c.StatusLabel,
                        StatusClass      = c.StatusClass
                    };
                }).ToList(),

                TotalCount        = result.TotalCount,
                TotalPages        = result.TotalPages,
                SearchAircraftId  = searchAircraftId,
                SearchCertType    = searchCertType,
                SearchActive      = searchActive,
                SearchExpiringSoon = searchExpiringSoon,
                SortColumn        = sortColumn,
                SortDirection     = sortDirection,
                PageNumber        = pageNumber,
                PageSize          = pageSize,

                // Summary counters
                CountExpired     = allActive.Count(c =>
                    c.ExpiryDate.HasValue && c.DaysRemaining < 0),
                CountExpiringSoon = allActive.Count(c =>
                    c.ExpiryDate.HasValue &&
                    c.DaysRemaining >= 0 && c.DaysRemaining <= 30),
                CountValid       = allActive.Count(c =>
                    !c.ExpiryDate.HasValue || c.DaysRemaining > 30),

                // Aircraft filter dropdown — already scoped via aircraftList
                AircraftOptions = BuildAircraftOptions(
                    aircraftList, acMap, searchAircraftId)
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        // Optional: pre-select aircraft via ?aircraftId=X
        public async Task<IActionResult> Create(int? aircraftId = null)
        {
            if (aircraftId.HasValue && !await IsAircraftInScopeAsync(aircraftId.Value))
                return Forbid();

            var dto = new AircraftCertificateFormDto
            {
                AircraftId = aircraftId ?? 0,
                IsActive   = true
            };

            if (aircraftId.HasValue)
                await FillAircraftInfo(dto, aircraftId.Value);

            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Create(AircraftCertificateFormDto dto)
        {
            // Defense in depth — dropdown only offers in-scope aircraft, but
            // AircraftId is still a posted value and can be tampered with.
            if (!await IsAircraftInScopeAsync(dto.AircraftId))
                return Forbid();

            if (ModelState.IsValid)
            {
                // Check uniqueness: one active cert per type per aircraft
                await _validator.CheckUniqueAsync<AircraftCertificate>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<AircraftCertificate>(
                        c => c.AircraftId == dto.AircraftId &&
                             c.CertType   == dto.CertType   &&
                             c.IsActive,
                        nameof(dto.CertType),
                        $"Un certificat '{dto.CertType}' actif existe deja " +
                        $"pour cet aeronef. Desactivez l'ancien avant d'en creer un nouveau.")
                );
            }

            if (!ModelState.IsValid)
            {
                await FillAircraftInfo(dto, dto.AircraftId);
                return View(dto);
            }

            var entity = MapToEntity(dto, new AircraftCertificate
                { CreatedAt = DateTime.UtcNow,
                  CreatedByUserId = User.Identity?.Name });

            // Handle document upload
            if (dto.DocumentFile?.Length > 0)
                await SaveDocument(entity, dto.DocumentFile);

            _uow.AircraftCertificates.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Certificat {entity.CertType} '{entity.Reference}' cree.";
            return RedirectToAction(nameof(Index),
                new { searchAircraftId = entity.AircraftId });
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.AircraftCertificates.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            var dto = MapToDto(entity);
            await FillAircraftInfo(dto, entity.AircraftId);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Edit(
            int id, AircraftCertificateFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!await IsAircraftInScopeAsync(dto.AircraftId))
                return Forbid();

            if (ModelState.IsValid)
            {
                await _validator.CheckUniqueAsync<AircraftCertificate>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<AircraftCertificate>(
                        c => c.AircraftId == dto.AircraftId &&
                             c.CertType   == dto.CertType   &&
                             c.IsActive,
                        nameof(dto.CertType),
                        $"Un certificat '{dto.CertType}' actif existe deja " +
                        $"pour cet aeronef.")
                );
            }

            if (!ModelState.IsValid)
            {
                await FillAircraftInfo(dto, dto.AircraftId);
                return View(dto);
            }

            var entity = await _uow.AircraftCertificates.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            entity.LastModifiedAt = DateTime.UtcNow;

            if (dto.DocumentFile?.Length > 0)
                await SaveDocument(entity, dto.DocumentFile);

            _uow.AircraftCertificates.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Certificat '{entity.Reference}' modifie.";
            return RedirectToAction(nameof(Index),
                new { searchAircraftId = entity.AircraftId });
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftCertificates.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false,
                    message = "Certificat introuvable." });

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            entity.IsActive       = false;
            entity.LastModifiedAt = DateTime.UtcNow;
            _uow.AircraftCertificates.Update(entity);
            await _uow.CompleteAsync();

            return Json(new { success = true,
                message = $"Certificat '{entity.Reference}' desactive." });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "MaintenanceWrite")]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftCertificates.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false,
                    message = "Certificat introuvable." });

            if (!await IsAircraftInScopeAsync(entity.AircraftId))
                return Forbid();

            entity.IsActive       = true;
            entity.LastModifiedAt = DateTime.UtcNow;
            _uow.AircraftCertificates.Update(entity);
            await _uow.CompleteAsync();

            return Json(new { success = true,
                message = $"Certificat '{entity.Reference}' reactive." });
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

        private static AircraftCertificateFormDto MapToDto(
            AircraftCertificate entity) =>
            new()
            {
                Id               = entity.Id,
                AircraftId       = entity.AircraftId,
                CertType         = entity.CertType,
                Reference        = entity.Reference,
                IssuingAuthority = entity.IssuingAuthority,
                IssueDate        = entity.IssueDate,
                ExpiryDate       = entity.ExpiryDate,
                Notes            = entity.Notes,
                IsActive         = entity.IsActive,
                DocumentPath     = entity.DocumentPath,
                DocumentName     = entity.DocumentName
            };

        private static AircraftCertificate MapToEntity(
            AircraftCertificateFormDto dto,
            AircraftCertificate        entity)
        {
            entity.AircraftId       = dto.AircraftId;
            entity.CertType         = dto.CertType!;
            entity.Reference        = dto.Reference.Trim();
            entity.IssuingAuthority = dto.IssuingAuthority?.Trim();
            entity.IssueDate        = dto.IssueDate;
            entity.ExpiryDate       = dto.ExpiryDate;
            entity.Notes            = dto.Notes?.Trim();
            entity.IsActive         = dto.IsActive;
            return entity;
        }

        private async Task FillAircraftInfo(
            AircraftCertificateFormDto dto, int aircraftId)
        {
            var aircraft = await _uow.Aircraft.GetByIdAsync(aircraftId);
            if (aircraft == null) return;

            dto.AircraftCode = aircraft.Registration;
            dto.AircraftTail = aircraft.TailNumber.ToString();

            var acType = await _uow.AcTypes.GetByIdAsync(aircraft.AcTypeId);
            dto.AircraftTypeName = acType?.Name;
        }

        private async Task SaveDocument(
            AircraftCertificate entity, IFormFile file)
        {
            var folder = Path.Combine(
                UploadRoot, entity.AircraftId.ToString());
            Directory.CreateDirectory(folder);

            var safeName =
                $"{entity.CertType}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folder, safeName);

            await using var stream =
                new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            entity.DocumentPath = filePath;
            entity.DocumentName = file.FileName;
        }

        private static IEnumerable<SelectListItem> BuildAircraftOptions(
            IEnumerable<FRAProject.Areas.Settings.Models.Aircraft> list,
            Dictionary<int, dynamic> acMap,
            int? selectedId)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "— Tous les aéronefs —",
                        Selected = !selectedId.HasValue }
            };
            items.AddRange(
                list.OrderBy(a => a.Registration)
                    .Select(a =>
                    {
                        acMap.TryGetValue(a.Id, out var info);
                        return new SelectListItem
                        {
                            Value    = a.Id.ToString(),
                            Text     = $"{a.Registration} · {info?.TypeName ?? "?"}" +
                                       $" · Queue {a.TailNumber}",
                            Selected = selectedId.HasValue &&
                                       selectedId.Value == a.Id
                        };
                    }));
            return items;
        }
    }
}
