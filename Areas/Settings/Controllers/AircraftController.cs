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
    public class AircraftController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidationService _validator;

        private const int DefaultPageSize = 15;

        public AircraftController(IUnitOfWork uow, IValidationService validator)
        {
            _uow = uow;
            _validator = validator;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? searchRegistration = null,
            int? searchAcTypeId = null,
            int? searchStatusId = null,
            int? searchBaseId = null,
            bool? searchActive = null,
            string sortColumn = "Registration",
            string sortDirection = "asc",
            int pageNumber = 1,
            int pageSize = DefaultPageSize)
        {
            var result = await _uow.Aircraft.GetPagedAsync(
                filter: x =>
                    (string.IsNullOrWhiteSpace(searchRegistration)
                        || x.Registration.Contains(searchRegistration)) &&
                    (searchAcTypeId == null || x.AcTypeId == searchAcTypeId) &&
                    (searchStatusId == null || x.AcStatusTypeId == searchStatusId) &&
                    (searchBaseId == null || (x.BaseId.HasValue && x.BaseId == searchBaseId)) &&
                    (searchActive == null || x.IsActive == searchActive),

                orderBy: sortColumn switch
                {
                    "TailNumber" => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.TailNo)
                        : q => q.OrderBy(x => x.TailNo),

                    "AcType" => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.AcTypeId)
                        : q => q.OrderBy(x => x.AcTypeId),

                    "Status" => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.AcStatusTypeId)
                        : q => q.OrderBy(x => x.AcStatusTypeId),

                    "Base" => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.BaseId)
                        : q => q.OrderBy(x => x.BaseId),

                    "FlightTime" => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.TotalFlightMinutes)
                        : q => q.OrderBy(x => x.TotalFlightMinutes),

                    _ => sortDirection == "desc"
                        ? q => q.OrderByDescending(x => x.Registration)
                        : q => q.OrderBy(x => x.Registration)
                },

                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var acTypes = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var statuses = await _uow.AcStatusTypes.GetWhereAsync(s => s.IsActive);
            var bases = await _uow.Bases.GetWhereAsync(b => b.IsActive);
            var roles = await _uow.MissionRoles.GetWhereAsync(r => r.IsActive);
            var versions = await _uow.AircraftVersions.GetWhereAsync(v => v.IsActive);

            var typeMap = acTypes.ToDictionary(t => t.Id, t => t.Name);
            var statusMap = statuses.ToDictionary(s => s.Id, s => new { s.Code, s.Name });
            var baseMap = bases.ToDictionary(b => b.Id, b => b.BaseName);
            var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);
            var versionMap = versions.ToDictionary(v => v.Id, v => v.Name);

            var allAircraft = await _uow.Aircraft.GetWhereAsync(x => x.IsActive);

            var vm = new AircraftIndexVm
            {
                Items = result.Items.Select(x =>
                {
                    var st = x.AcStatusTypeId > 0 &&
                             statusMap.TryGetValue(x.AcStatusTypeId, out var s)
                        ? s
                        : null;

                    return new AircraftListVm
                    {
                        Id = x.Id,
                        Registration = x.Registration,
                        TailNo = x.TailNo,
                        SerialNumber = x.SerialNumber,
                        AcTypeName = typeMap.TryGetValue(x.AcTypeId, out var tn) ? tn : null,
                        VersionName = x.AircraftVersionId.HasValue &&
                                      versionMap.TryGetValue(x.AircraftVersionId.Value, out var vn)
                            ? vn
                            : null,
                        StatusCode = st?.Code,
                        StatusName = st?.Name,
                        BaseName = x.BaseId.HasValue &&
                                   baseMap.TryGetValue(x.BaseId.Value, out var bn)
                            ? bn
                            : null,
                        MissionRoleName = x.MissionRoleId.HasValue &&
                                          roleMap.TryGetValue(x.MissionRoleId.Value, out var rn)
                            ? rn
                            : null,
                        TotalFlightMinutes = x.TotalFlightMinutes,
                        TotalCycles = x.TotalCycles,
                        TotalLandings = x.TotalLandings,
                        ServiceEntryDate = x.ServiceEntryDate,
                        RegistrationDate = x.RegistrationDate,
                        IsActive = x.IsActive
                    };
                }).ToList(),

                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                SearchRegistration = searchRegistration,
                SearchAcTypeId = searchAcTypeId,
                SearchStatusId = searchStatusId,
                SearchBaseId = searchBaseId,
                SearchActive = searchActive,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                PageNumber = pageNumber,
                PageSize = pageSize,

                TotalAircraft = allAircraft.Count(),
                TotalOpr = allAircraft.Count(x =>
                    statusMap.TryGetValue(x.AcStatusTypeId, out var s) && s.Code == "OPR"),
                TotalMnt = allAircraft.Count(x =>
                    statusMap.TryGetValue(x.AcStatusTypeId, out var s) && s.Code == "MNT"),
                TotalAog = allAircraft.Count(x =>
                    statusMap.TryGetValue(x.AcStatusTypeId, out var s) && s.Code == "AOG"),

                AcTypeOptions = BuildOptions(acTypes, x => x.Id, x => x.Name,
                    searchAcTypeId, "— Tous les types —"),
                StatusOptions = BuildOptions(statuses, x => x.Id, x => x.Name,
                    searchStatusId, "— Tous les statuts —"),
                BaseOptions = BuildOptions(bases, x => x.Id, x => x.BaseName,
                    searchBaseId, "— Toutes les bases —")
            };

            return View(vm);
        }

        // ── CREATE GET ───────────────────────────────────────────────────
        public async Task<IActionResult> Create(int? dossierId = null)
        {
            var dto = new AircraftFormDto
            {
                IsActive = true,
                SortOrder = 99
            };

            if (dossierId.HasValue)
                await PreFillFromDossier(dto, dossierId.Value);

            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── CREATE POST ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftFormDto dto)
        {
            if (ModelState.IsValid)
            {
                var registration = dto.Registration.Trim().ToUpper();

                await _validator.CheckUniqueAsync<Aircraft>(
                    ModelState,
                    excludeId: null,
                    new UniqueField<Aircraft>(
                        x => x.Registration == registration,
                        nameof(dto.Registration),
                        $"La marque '{registration}' est deja attribuee."),
                    new UniqueField<Aircraft>(
                        x => x.TailNo == dto.TailNo,
                        nameof(dto.TailNo),
                        $"Le numero de queue '{dto.TailNo}' est deja attribue.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = MapToEntity(dto, new Aircraft());
            entity.CreatedAt = DateTime.UtcNow;

            _uow.Aircraft.Add(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Aeronef '{entity.Registration}' enregistre avec succes.";

            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.Aircraft.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            var dto = MapToDto(entity);

            if (entity.DossierId.HasValue)
            {
                var dossier = await _uow.Dossiers.GetByIdAsync(entity.DossierId.Value);
                dto.DossierNumber = dossier?.DossierNumber;
            }

            await PopulateDropdowns(dto);
            return View(dto);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftFormDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var registration = dto.Registration.Trim().ToUpper();

                await _validator.CheckUniqueAsync<Aircraft>(
                    ModelState,
                    excludeId: id,
                    new UniqueField<Aircraft>(
                        x => x.Registration == registration,
                        nameof(dto.Registration),
                        $"La marque '{registration}' est deja attribuee."),
                    new UniqueField<Aircraft>(
                        x => x.TailNo == dto.TailNo,
                        nameof(dto.TailNo),
                        $"Le numero de queue '{dto.TailNo}' est deja attribue.")
                );
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto);
                return View(dto);
            }

            var entity = await _uow.Aircraft.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MapToEntity(dto, entity);
            entity.LastModifiedAt = DateTime.UtcNow;

            _uow.Aircraft.Update(entity);
            await _uow.CompleteAsync();

            TempData["SuccessMessage"] =
                $"Aeronef '{entity.Registration}' modifie avec succes.";

            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — soft ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _uow.Aircraft.GetByIdAsync(id);

            if (entity == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Aeronef introuvable."
                });
            }

            entity.IsActive = false;
            entity.Active = false;
            entity.LastModifiedAt = DateTime.UtcNow;

            _uow.Aircraft.Update(entity);
            await _uow.CompleteAsync();

            return Json(new
            {
                success = true,
                message = $"Aeronef '{entity.Registration}' desactive."
            });
        }

        // ── ACTIVATE ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var entity = await _uow.Aircraft.GetByIdAsync(id);

            if (entity == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Aeronef introuvable."
                });
            }

            entity.IsActive = true;
            entity.Active = true;
            entity.LastModifiedAt = DateTime.UtcNow;

            _uow.Aircraft.Update(entity);
            await _uow.CompleteAsync();

            return Json(new
            {
                success = true,
                message = $"Aeronef '{entity.Registration}' reactive."
            });
        }

        // ── PRINT — opens standalone A4 fiche ────────────────────────────
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _uow.Aircraft.GetByIdAsync(id.Value);
            if (entity == null) return NotFound();

            if (entity.AcTypeId > 0)
                entity.AcType = await _uow.AcTypes.GetByIdAsync(entity.AcTypeId);

            if (entity.AircraftVersionId.HasValue)
                entity.AircraftVersion = await _uow.AircraftVersions
                    .GetByIdAsync(entity.AircraftVersionId.Value);

            if (entity.AcStatusTypeId > 0)
                entity.AcStatusType = await _uow.AcStatusTypes
                    .GetByIdAsync(entity.AcStatusTypeId);

            if (entity.MissionRoleId.HasValue)
                entity.MissionRole = await _uow.MissionRoles
                    .GetByIdAsync(entity.MissionRoleId.Value);

            if (entity.ManufacturerId.HasValue)
                entity.AircraftManufacturerNav = await _uow.AircraftManufacturers
                    .GetByIdAsync(entity.ManufacturerId.Value);

            if (entity.OriginCountryId.HasValue)
                entity.OriginCountry = await _uow.Countries
                    .GetByIdAsync(entity.OriginCountryId.Value);

            if (entity.BaseId.HasValue)
                entity.Base = await _uow.Bases
                    .GetByIdAsync(entity.BaseId.Value);

            if (entity.DossierId.HasValue)
                entity.Dossier = await _uow.Dossiers
                    .GetByIdAsync(entity.DossierId.Value);

            return View("Print", entity);
        }

        // ── AJAX — GetVersions ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetVersions(int? acTypeId)
        {
            if (!acTypeId.HasValue)
                return Json(Array.Empty<object>());

            var versions = await _uow.AircraftVersions
                .GetWhereAsync(v => v.IsActive && v.AcTypeId == acTypeId.Value);

            return Json(versions
                .OrderBy(v => v.SortOrder)
                .Select(v => new { value = v.Id, text = v.DisplayLabel }));
        }

        // ── AJAX — GetMissionRoles (cascade from AcType → AcCategory) ────
        [HttpGet]
        public async Task<IActionResult> GetMissionRoles(int? acTypeId)
        {
            if (!acTypeId.HasValue)
                return Json(Array.Empty<object>());

            var acType = await _uow.AcTypes.GetByIdAsync(acTypeId.Value);
            if (acType == null) return Json(Array.Empty<object>());

            var group = await _uow.AcMainGroups.GetByIdAsync(acType.AcMainGroupId);

            var acCategoryId = group != null ? group.AcCategoryId : (int?)null;

            var roles = await _uow.MissionRoles.GetWhereAsync(r =>
                r.IsActive &&
                (r.AcCategoryId == null || r.AcCategoryId == acCategoryId));

            return Json(roles
                .OrderBy(r => r.SortOrder)
                .Select(r => new { value = r.Id, text = r.Name }));
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════════

        // ── Pre-fill Create form from approved Dossier ────────────────────
        private async Task PreFillFromDossier(AircraftFormDto dto, int dossierId)
        {
            var dossier = await _uow.Dossiers.GetByIdAsync(dossierId);
            if (dossier == null) return;

            var aircraft = await _uow.DossierAircrafts.GetByIdAsync(dossierId);
            if (aircraft == null) return;

            dto.DossierId = dossierId;
            dto.DossierNumber = dossier.DossierNumber;
            dto.Registration = aircraft.FullImmatriculation ?? string.Empty;
            dto.SerialNumber = aircraft.SerialNumber;
            dto.AcTypeId = aircraft.AcTypeId;
            dto.AircraftVersionId = aircraft.AircraftVersionId;
            dto.ManufacturerId = aircraft.ManufacturerId;
            dto.OriginCountryId = aircraft.OriginCountryId;
            dto.ManufactureDate = aircraft.ManufactureDate.HasValue
                ? aircraft.ManufactureDate.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;
            dto.ServiceEntryDate = aircraft.ServiceEntryDate;
            dto.BaseId = aircraft.PortAttacheId;
            dto.MissionRoleId = aircraft.MissionRoleId;
            dto.RegistrationDate = DateOnly.FromDateTime(DateTime.Today);

            if (aircraft.AcTypeId.HasValue)
            {
                var acType = await _uow.AcTypes.GetByIdAsync(aircraft.AcTypeId.Value);
                if (acType != null)
                {
                    dto.Obs = $"{acType.Name} / {dto.Registration}";
                }
            }
        }

        // ── Entity → DTO ──────────────────────────────────────────────────
        private static AircraftFormDto MapToDto(Aircraft entity) =>
            new()
            {
                Id = entity.Id,
                Registration = entity.Registration,
                Obs = entity.Obs,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                TailNo = entity.TailNo,
                SerialNumber = entity.SerialNumber,
                AcTypeId = entity.AcTypeId,
                AircraftVersionId = entity.AircraftVersionId,
                AcStatusTypeId = entity.AcStatusTypeId,
                MissionRoleId = entity.MissionRoleId,
                ManufacturerId = entity.ManufacturerId,
                OriginCountryId = entity.OriginCountryId,
                BaseId = entity.BaseId,
                ManufactureDate = entity.ManufactureDate,
                ServiceEntryDate = entity.ServiceEntryDate,
                RegistrationDate = entity.RegistrationDate,
                TotalFlightMinutes = entity.TotalFlightMinutes,
                TotalCycles = entity.TotalCycles,
                TotalLandings = entity.TotalLandings,
                DossierId = entity.DossierId
            };

        // ── DTO → Entity ──────────────────────────────────────────────────
        private static Aircraft MapToEntity(AircraftFormDto dto, Aircraft entity)
        {
            entity.Registration = dto.Registration.Trim().ToUpper();
            entity.Obs = dto.Obs?.Trim();
            entity.SortOrder = (byte)dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.Active = dto.IsActive;
            entity.TailNo = dto.TailNo;
            entity.SerialNumber = dto.SerialNumber?.Trim();
            entity.AcTypeId = dto.AcTypeId!.Value;
            entity.AircraftVersionId = dto.AircraftVersionId;
            entity.AcStatusTypeId = dto.AcStatusTypeId!.Value;
            entity.MissionRoleId = dto.MissionRoleId;
            entity.ManufacturerId = dto.ManufacturerId;
            entity.OriginCountryId = dto.OriginCountryId;
            entity.BaseId = dto.BaseId;
            entity.ManufactureDate = dto.ManufactureDate;
            entity.ServiceEntryDate = dto.ServiceEntryDate;
            entity.RegistrationDate = dto.RegistrationDate;
            entity.TotalFlightMinutes = dto.TotalFlightMinutes;
            entity.TotalCycles = dto.TotalCycles;
            entity.TotalLandings = dto.TotalLandings;
            entity.DossierId = dto.DossierId;

            return entity;
        }

        // ── Populate all dropdowns ────────────────────────────────────────
        private async Task PopulateDropdowns(AircraftFormDto dto)
        {
            var acTypes = await _uow.AcTypes.GetWhereAsync(t => t.IsActive);
            var statuses = await _uow.AcStatusTypes.GetWhereAsync(s => s.IsActive);
            var bases = await _uow.Bases.GetWhereAsync(b => b.IsActive);
            var roles = await _uow.MissionRoles.GetWhereAsync(r => r.IsActive);
            var manufacturers = await _uow.AircraftManufacturers.GetWhereAsync(m => m.IsActive);
            var countries = await _uow.Countries.GetWhereAsync(c => c.IsActive);

            var versions = dto.AcTypeId.HasValue
                ? await _uow.AircraftVersions.GetWhereAsync(
                    v => v.IsActive && v.AcTypeId == dto.AcTypeId.Value)
                : [];

            dto.AcTypeOptions = BuildOptions(
                acTypes,
                x => x.Id,
                x => x.DisplayLabel,
                dto.AcTypeId,
                "— Sélectionner le type —");

            dto.VersionOptions = BuildOptions(
                versions,
                x => x.Id,
                x => x.DisplayLabel,
                dto.AircraftVersionId,
                "— Version (optionnel) —");

            dto.StatusOptions = BuildOptions(
                statuses,
                x => x.Id,
                x => $"{x.Code} — {x.Name}",
                dto.AcStatusTypeId,
                "— Sélectionner le statut —");

            dto.BaseOptions = BuildOptions(
                bases,
                x => x.Id,
                x => x.BaseName,
                dto.BaseId,
                "— Sélectionner la base —");

            dto.MissionRoleOptions = BuildOptions(
                roles,
                x => x.Id,
                x => x.Name,
                dto.MissionRoleId,
                "— Rôle (optionnel) —");

            dto.ManufacturerOptions = BuildOptions(
                manufacturers,
                x => x.Id,
                x => x.DisplayLabel,
                dto.ManufacturerId,
                "— Constructeur (optionnel) —");

            dto.CountryOptions = BuildOptions(
                countries,
                x => x.Id,
                x => x.DisplayLabel,
                dto.OriginCountryId,
                "— Pays (optionnel) —");
        }

        // ── Generic SelectList builder ────────────────────────────────────
        private static IEnumerable<SelectListItem> BuildOptions<T>(
            IEnumerable<T> items,
            Func<T, int> valueSelector,
            Func<T, string> textSelector,
            int? selectedId,
            string placeholder)
        {
            var list = new List<SelectListItem>
            {
                new()
                {
                    Value = "",
                    Text = placeholder,
                    Selected = !selectedId.HasValue
                }
            };

            list.AddRange(items.Select(item => new SelectListItem
            {
                Value = valueSelector(item).ToString(),
                Text = textSelector(item),
                Selected = selectedId.HasValue &&
                           selectedId.Value == valueSelector(item)
            }));

            return list;
        }
    }
}