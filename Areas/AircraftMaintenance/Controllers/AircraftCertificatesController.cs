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
    [Authorize(Roles = "Admin")]
    public class AircraftCertificatesController : Controller
    {
        private readonly IUnitOfWork        _uow;
        private readonly IValidationService _validator;

        private const string UploadRoot =
            @"D:\2BAFRA\Uploads\Certificates\";

        private const int DefaultPageSize = 15;

        public AircraftCertificatesController(
            IUnitOfWork uow, IValidationService validator)
        {
            _uow       = uow;
            _validator = validator;
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

            var result = await _uow.AircraftCertificates.GetPagedAsync(

                filter: c =>
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

            // Aircraft + type name map
            var aircraftList = await _uow.Aircraft
                .GetWhereAsync(a => a.IsActive);
            var acTypes = await _uow.AcTypes
                .GetWhereAsync(t => t.IsActive);
            var typeMap = acTypes.ToDictionary(t => t.Id, t => t.Name);

            var acMap = aircraftList.ToDictionary(
                a => a.Id,
                a => (dynamic)new
                {
                    a.Registration,
                    Tail     = a.TailNumber.ToString(),
                    TypeName = typeMap.TryGetValue(a.AcTypeId, out var tn)
                                   ? tn : null
                });

            // Summary counts — all active certs
            var allActive = await _uow.AircraftCertificates
                .GetWhereAsync(c => c.IsActive);

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

                // Aircraft filter dropdown
                AircraftOptions = BuildAircraftOptions(
                    aircraftList, acMap, searchAircraftId)
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        // Optional: pre-select aircraft via ?aircraftId=X
        public async Task<IActionResult> Create(int? aircraftId = null)
        {
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
        public async Task<IActionResult> Create(AircraftCertificateFormDto dto)
        {
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

            var dto = MapToDto(entity);
            await FillAircraftInfo(dto, entity.AircraftId);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, AircraftCertificateFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

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
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.AircraftCertificates.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false,
                    message = "Certificat introuvable." });

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
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.AircraftCertificates.GetByIdAsync(id);
            if (entity == null)
                return Json(new { success = false,
                    message = "Certificat introuvable." });

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
