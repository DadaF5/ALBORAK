using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Services
{
    // ════════════════════════════════════════════════════════════════
    //  INTERFACE
    // ════════════════════════════════════════════════════════════════
    /// <summary>
    /// All dossier business logic — orchestration, DTO building,
    /// dropdown population, mapping, step advancement.
    ///
    /// The controller calls this service exclusively.
    /// It never touches DbContext directly — uses IUnitOfWork.
    ///
    /// Register in Program.cs:
    ///   builder.Services.AddScoped&lt;IDossierService, DossierService&gt;();
    /// </summary>
    public interface IDossierService
    {
        // ── DTO builders ─────────────────────────────────────────────
        Task<DossierStep1Dto>  BuildStep1DtoAsync(int? dossierId = null);
        Task<DossierStep2Dto>  BuildStep2DtoAsync(int dossierId);
        Task<DossierStep3Dto>  BuildStep3DtoAsync(int dossierId);
        Task<DossierStep4Dto>  BuildStep4DtoAsync(int dossierId);
        Task<DossierStep5Dto>  BuildStep5DtoAsync(int dossierId);
        Task<DossierIndexVm>   BuildIndexVmAsync(
            string? searchNumber, string? searchImmat, string? searchStatus,
            string sortColumn, string sortDirection, int pageNumber, int pageSize);

        // ── Repopulate dropdowns after validation failure ─────────────
        Task RepopulateStep1Async(DossierStep1Dto dto);
        Task RepopulateStep2Async(DossierStep2Dto dto);
        Task RepopulateStep3Async(DossierStep3Dto dto);

        // ── Save step data ────────────────────────────────────────────
        Task<ImmatriculationDossier> SaveStep1Async(DossierStep1Dto dto, string? userId);
        Task SaveStep2Async(int dossierId, DossierStep2Dto dto);
        Task SaveStep3Async(int dossierId, DossierStep3Dto dto);
        Task SaveStep4AdvanceAsync(int dossierId);

        // ── Document handling ─────────────────────────────────────────
        Task<(bool Success, string? Error, object? Payload)>
            UploadDocumentAsync(int dossierId, int documentTypeId,
                                IFormFile file, string? userId);
        Task<bool> DeleteDocumentAsync(int dossierId, int documentId);

        // ── Submission ────────────────────────────────────────────────
        Task<string> SubmitAsync(int dossierId, DossierStep5Dto dto);

        // ── Queries ───────────────────────────────────────────────────
        Task<ImmatriculationDossier?> GetEditableDossierAsync(int id);
        Task<bool> AllRequiredDocsUploadedAsync(int dossierId);
        Task<WizardProgressVm> GetProgressVmAsync(int dossierId);
    }

    // ════════════════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ════════════════════════════════════════════════════════════════
    public class DossierService : IDossierService
    {
        private readonly IUnitOfWork       _uow;
        private readonly IFileUploadService _fileService;

        public DossierService(IUnitOfWork uow, IFileUploadService fileService)
        {
            _uow         = uow;
            _fileService = fileService;
        }

        // ════════════════════════════════════════════════════════════
        //  INDEX VM
        // ════════════════════════════════════════════════════════════
        public async Task<DossierIndexVm> BuildIndexVmAsync(
            string? searchNumber, string? searchImmat, string? searchStatus,
            string sortColumn, string sortDirection, int pageNumber, int pageSize)
        {
            var result = await _uow.Dossiers.GetPagedAsync(

                filter: d =>
                    d.IsActive &&
                    (string.IsNullOrWhiteSpace(searchNumber)
                        || (d.DossierNumber != null &&
                            d.DossierNumber.Contains(searchNumber))) &&
                    (string.IsNullOrWhiteSpace(searchStatus)
                        || d.Status == searchStatus),

                orderBy: sortColumn switch
                {
                    "DossierNumber" => sortDirection == "desc"
                        ? q => q.OrderByDescending(d => d.DossierNumber)
                        : q => q.OrderBy(d => d.DossierNumber),
                    "Status"        => sortDirection == "desc"
                        ? q => q.OrderByDescending(d => d.Status)
                        : q => q.OrderBy(d => d.Status),
                    _               => sortDirection == "desc"
                        ? q => q.OrderByDescending(d => d.CreatedAt)
                        : q => q.OrderBy(d => d.CreatedAt)
                },

                pageNumber: pageNumber,
                pageSize:   pageSize
            );

            var dossierIds = result.Items.Select(d => d.Id).ToList();

            var aircraftRows  = await _uow.DossierAircrafts
                .GetWhereAsync(a => dossierIds.Contains(a.DossierId));
            var authorityRows = await _uow.DossierAuthorities
                .GetWhereAsync(a => dossierIds.Contains(a.DossierId));

            var authorityIds = authorityRows
                .Where(a => a.EmployingAuthorityId.HasValue)
                .Select(a => a.EmployingAuthorityId!.Value)
                .Distinct().ToList();

            var authorities = await _uow.EmployingAuthorities
                .GetWhereAsync(e => authorityIds.Contains(e.Id));

            var authorityMap = authorities.ToDictionary(e => e.Id, e => e.Name);

            return new DossierIndexVm
            {
                Items = result.Items.Select(d =>
                {
                    var ac   = aircraftRows.FirstOrDefault(a => a.DossierId == d.Id);
                    var auth = authorityRows.FirstOrDefault(a => a.DossierId == d.Id);
                    var authName = auth?.EmployingAuthorityId.HasValue == true &&
                                   authorityMap.TryGetValue(
                                       auth.EmployingAuthorityId!.Value, out var n)
                                   ? n : null;

                    return new DossierListVm
                    {
                        Id                  = d.Id,
                        DossierNumber       = d.DossierNumber,
                        Status              = d.Status,
                        CurrentStep         = d.CurrentStep,
                        FullImmatriculation = ac?.FullImmatriculation,
                        OgmnNumber          = auth?.OgmnNumber,
                        AuthorityName       = authName,
                        CreatedAt           = d.CreatedAt,
                        SubmittedAt         = d.SubmittedAt,
                        IsEditable          = d.IsEditable
                    };
                }).ToList(),

                TotalCount    = result.TotalCount,
                TotalPages    = result.TotalPages,
                SearchNumber  = searchNumber,
                SearchImmat   = searchImmat,
                SearchStatus  = searchStatus,
                SortColumn    = sortColumn,
                SortDirection = sortDirection,
                PageNumber    = pageNumber,
                PageSize      = pageSize
            };
        }

        // ════════════════════════════════════════════════════════════
        //  DTO BUILDERS
        // ════════════════════════════════════════════════════════════

        public async Task<DossierStep1Dto> BuildStep1DtoAsync(int? dossierId = null)
        {
            var dto = new DossierStep1Dto { DossierId = dossierId ?? 0 };

            if (dossierId.HasValue && dossierId > 0)
            {
                var authority = await _uow.DossierAuthorities
                    .GetByIdAsync(dossierId.Value);

                if (authority != null)
                {
                    dto.EmployingAuthorityId = authority.EmployingAuthorityId;
                    dto.BaseAerienneId       = authority.BaseAerienneId;
                    dto.OgmnNumber           = authority.OgmnNumber;
                    dto.OgmnAggrementDate    = authority.OgmnAggrementDate;
                    dto.OgmnSousPartie       = authority.OgmnSousPartie;
                    dto.OgmnResponsable      = authority.OgmnResponsable;
                    dto.AeAddress            = authority.AeAddress;
                    dto.AePhone              = authority.AePhone;
                    dto.AeEmail              = authority.AeEmail;
                }
            }

            await RepopulateStep1Async(dto);
            return dto;
        }

        public async Task<DossierStep2Dto> BuildStep2DtoAsync(int dossierId)
        {
            var aircraft = await _uow.DossierAircrafts.GetByIdAsync(dossierId);
            var dto = aircraft != null ? MapToStep2Dto(aircraft) : new DossierStep2Dto();
            dto.DossierId = dossierId;
            await RepopulateStep2Async(dto);
            return dto;
        }

        public async Task<DossierStep3Dto> BuildStep3DtoAsync(int dossierId)
        {
            var airworthiness = await _uow.DossierAirworthiness.GetByIdAsync(dossierId);
            var dto = airworthiness != null
                ? MapToStep3Dto(airworthiness)
                : new DossierStep3Dto();
            dto.DossierId = dossierId;
            await RepopulateStep3Async(dto);
            return dto;
        }

        public async Task<DossierStep4Dto> BuildStep4DtoAsync(int dossierId)
        {
            var docTypes = await _uow.ImmatriculationDocTypes
                .GetWhereAsync(t => t.IsActive);
            var uploads  = await _uow.ImmatriculationDocuments
                .GetWhereAsync(d => d.DossierId == dossierId && d.IsActive);

            var uploadMap = uploads.ToDictionary(u => u.DocumentTypeId);

            return new DossierStep4Dto
            {
                DossierId = dossierId,
                Slots = docTypes
                    .OrderBy(t => t.SortOrder)
                    .Select(t =>
                    {
                        uploadMap.TryGetValue(t.Id, out var upload);
                        return new DocumentSlotVm
                        {
                            DocumentTypeId   = t.Id,
                            Code             = t.Code,
                            Name             = t.Name,
                            ArticleReference = t.ArticleReference,
                            IsRequired       = t.IsRequired,
                            AcceptedFormats  = t.AcceptedFormats,
                            MaxFileSizeMb    = t.MaxFileSizeMb,
                            DocumentId       = upload?.Id,
                            FileName         = upload?.FileName,
                            FileSizeDisplay  = upload?.FileSizeDisplay
                        };
                    }).ToList()
            };
        }

        public async Task<DossierStep5Dto> BuildStep5DtoAsync(int dossierId)
        {
            var dossier       = await _uow.Dossiers.GetByIdAsync(dossierId);
            var aircraft      = await _uow.DossierAircrafts.GetByIdAsync(dossierId);
            var authority     = await _uow.DossierAuthorities.GetByIdAsync(dossierId);
            var requiredTypes = await _uow.ImmatriculationDocTypes
                                    .GetWhereAsync(t => t.IsRequired && t.IsActive);
            var uploaded      = await _uow.ImmatriculationDocuments
                                    .GetWhereAsync(d => d.DossierId == dossierId && d.IsActive);

            var uploadedTypeIds = uploaded.Select(d => d.DocumentTypeId).ToHashSet();

            string? authorityName = null;
            if (authority?.EmployingAuthorityId.HasValue == true)
            {
                var ea = await _uow.EmployingAuthorities
                    .GetByIdAsync(authority.EmployingAuthorityId.Value);
                authorityName = ea?.Name;
            }

            return new DossierStep5Dto
            {
                DossierId            = dossierId,
                AttestationCity      = dossier?.AttestationCity,
                AttestationDate      = dossier?.AttestationDate,
                SignatoryName        = dossier?.SignatoryName,
                AttestationConfirmed = dossier?.AttestationConfirmed ?? false,
                FullImmatriculation  = aircraft?.FullImmatriculation,
                AircraftTypeName     = aircraft?.AcTypeId.HasValue == true
                                           ? $"Type #{aircraft.AcTypeId}" : null,
                AuthorityName        = authorityName,
                OgmnNumber           = authority?.OgmnNumber,
                SerialNumber         = aircraft?.SerialNumber,
                RequiredDocCount     = requiredTypes.Count(),
                UploadedDocCount     = uploadedTypeIds
                                           .Intersect(requiredTypes.Select(t => t.Id))
                                           .Count()
            };
        }

        // ════════════════════════════════════════════════════════════
        //  DROPDOWN REPOPULATION
        // ════════════════════════════════════════════════════════════

        public async Task RepopulateStep1Async(DossierStep1Dto dto)
        {
            var authorities = await _uow.EmployingAuthorities
                .GetWhereAsync(a => a.IsActive);
            var bases = await _uow.Bases
                .GetWhereAsync(b => b.IsActive);

            dto.AuthorityOptions = BuildSelectList(
                authorities.OrderBy(a => a.SortOrder),
                a => a.Id.ToString(), a => a.DisplayLabel,
                dto.EmployingAuthorityId?.ToString());

            dto.BaseOptions = BuildSelectList(
                bases.OrderBy(b => b.BaseName),
                b => b.Id.ToString(), b => b.BaseName,
                dto.BaseAerienneId?.ToString());
        }

        public async Task RepopulateStep2Async(DossierStep2Dto dto)
        {
            var categories    = await _uow.AcCategories
                .GetWhereAsync(c => c.IsActive);
            var manufacturers = await _uow.AircraftManufacturers
                .GetWhereAsync(m => m.IsActive);
            var bases         = await _uow.Bases
                .GetWhereAsync(b => b.IsActive);
            var countries     = await _uow.Countries
                .GetWhereAsync(c => c.IsActive);
            var versions      = dto.AcTypeId.HasValue
                ? await _uow.AircraftVersions.GetWhereAsync(v => v.IsActive)
                : [];
            var roles         = await _uow.MissionRoles.GetWhereAsync(r =>
                r.IsActive &&
                (!dto.AircraftCategoryId.HasValue ||
                 r.AcCategoryId == null ||
                 r.AcCategoryId == dto.AircraftCategoryId));

            dto.CategoryOptions     = BuildSelectList(
                categories.OrderBy(c => c.SortOrder),
                c => c.Id.ToString(), c => c.DisplayLabel,
                dto.AircraftCategoryId?.ToString());

            dto.ManufacturerOptions = BuildSelectList(
                manufacturers.OrderBy(m => m.Name),
                m => m.Id.ToString(), m => m.Name,
                dto.ManufacturerId?.ToString());

            dto.PortAttacheOptions  = BuildSelectList(
                bases.OrderBy(b => b.BaseName),
                b => b.Id.ToString(), b => b.BaseName,
                dto.PortAttacheId?.ToString());

            dto.CountryOptions      = BuildSelectList(
                countries.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
                c => c.Id.ToString(), c => c.DisplayLabel,
                dto.OriginCountryId?.ToString());

            dto.VersionOptions      = BuildSelectList(
                versions.OrderBy(v => v.SortOrder),
                v => v.Id.ToString(), v => v.Name,
                dto.AircraftVersionId?.ToString());

            dto.MissionRoleOptions  = BuildSelectList(
                roles.OrderBy(r => r.SortOrder),
                r => r.Id.ToString(), r => r.Name,
                dto.MissionRoleId?.ToString());
        }

        public async Task RepopulateStep3Async(DossierStep3Dto dto)
        {
            var cdnTypes  = await _uow.CdnDocTypes
                .GetWhereAsync(t => t.IsActive);
            var countries = await _uow.Countries
                .GetWhereAsync(c => c.IsActive);

            dto.CdnDocTypeOptions = BuildSelectList(
                cdnTypes.OrderBy(t => t.SortOrder),
                t => t.Id.ToString(), t => t.DisplayLabel,
                dto.CdnDocTypeId?.ToString());

            dto.CountryOptions    = BuildSelectList(
                countries.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
                c => c.Id.ToString(), c => c.DisplayLabel,
                dto.ForeignCountryId?.ToString());
        }

        // ════════════════════════════════════════════════════════════
        //  SAVE STEP DATA
        // ════════════════════════════════════════════════════════════

        public async Task<ImmatriculationDossier> SaveStep1Async(
            DossierStep1Dto dto, string? userId)
        {
            ImmatriculationDossier dossier;

            if (dto.DossierId == 0)
            {
                // Create master + authority
                dossier = new ImmatriculationDossier
                {
                    Status          = "Brouillon",
                    CurrentStep     = 2,
                    CreatedAt       = DateTime.UtcNow,
                    CreatedByUserId = userId
                };
                _uow.Dossiers.Add(dossier);
                await _uow.CompleteAsync();

                var authority = MapToAuthority(dto,
                    new DossierAuthority { DossierId = dossier.Id });
                _uow.DossierAuthorities.Add(authority);
            }
            else
            {
                dossier = (await _uow.Dossiers.GetByIdAsync(dto.DossierId))!;

                var authority = await _uow.DossierAuthorities
                                    .GetByIdAsync(dossier.Id)
                                ?? new DossierAuthority { DossierId = dossier.Id };

                var isNew = authority.DossierId != dossier.Id;
                MapToAuthority(dto, authority);

                if (isNew) _uow.DossierAuthorities.Add(authority);
                else       _uow.DossierAuthorities.Update(authority);

                await AdvanceStepAsync(dossier, 2);
            }

            await _uow.CompleteAsync();
            return dossier;
        }

        public async Task SaveStep2Async(int dossierId, DossierStep2Dto dto)
        {
            var aircraft = await _uow.DossierAircrafts.GetByIdAsync(dossierId);

            if (aircraft == null)
            {
                aircraft = new DossierAircraft { DossierId = dossierId };
                MapToAircraft(dto, aircraft);
                _uow.DossierAircrafts.Add(aircraft);
            }
            else
            {
                MapToAircraft(dto, aircraft);
                _uow.DossierAircrafts.Update(aircraft);
            }

            var dossier = (await _uow.Dossiers.GetByIdAsync(dossierId))!;
            await AdvanceStepAsync(dossier, 3);
            await _uow.CompleteAsync();
        }

        public async Task SaveStep3Async(int dossierId, DossierStep3Dto dto)
        {
            var airworthiness = await _uow.DossierAirworthiness.GetByIdAsync(dossierId);

            if (airworthiness == null)
            {
                airworthiness = new DossierAirworthiness { DossierId = dossierId };
                MapToAirworthiness(dto, airworthiness);
                _uow.DossierAirworthiness.Add(airworthiness);
            }
            else
            {
                MapToAirworthiness(dto, airworthiness);
                _uow.DossierAirworthiness.Update(airworthiness);
            }

            var dossier = (await _uow.Dossiers.GetByIdAsync(dossierId))!;
            await AdvanceStepAsync(dossier, 4);
            await _uow.CompleteAsync();
        }

        public async Task SaveStep4AdvanceAsync(int dossierId)
        {
            var dossier = (await _uow.Dossiers.GetByIdAsync(dossierId))!;
            await AdvanceStepAsync(dossier, 5);
            await _uow.CompleteAsync();
        }

        // ════════════════════════════════════════════════════════════
        //  DOCUMENT UPLOAD / DELETE
        // ════════════════════════════════════════════════════════════

        public async Task<(bool Success, string? Error, object? Payload)>
            UploadDocumentAsync(
                int dossierId, int documentTypeId,
                IFormFile file, string? userId)
        {
            var docType = await _uow.ImmatriculationDocTypes
                .GetByIdAsync(documentTypeId);

            if (docType == null)
                return (false, "Type de document inconnu.", null);

            // Save file
            var result = await _fileService.SaveFileAsync(
                dossierId, docType.Code, docType.MaxFileSizeMb, file);

            if (!result.Success)
                return (false, result.ErrorMessage, null);

            // Soft-delete any existing active upload for this type
            var existing = await _uow.ImmatriculationDocuments
                .GetFirstOrDefaultAsync(d =>
                    d.DossierId == dossierId &&
                    d.DocumentTypeId == documentTypeId &&
                    d.IsActive);

            if (existing != null)
            {
                _fileService.MarkFileDeleted(existing.FilePath ?? "");
                existing.IsActive = false;
                _uow.ImmatriculationDocuments.Update(existing);
            }

            // Create new document record
            var doc = new ImmatriculationDocument
            {
                DossierId        = dossierId,
                DocumentTypeId   = documentTypeId,
                FilePath         = result.FilePath,
                FileName         = result.FileName,
                FileSize         = result.FileSize,
                MimeType         = result.MimeType,
                UploadedAt       = DateTime.UtcNow,
                UploadedByUserId = userId,
                IsActive         = true
            };

            _uow.ImmatriculationDocuments.Add(doc);

            var dossier = (await _uow.Dossiers.GetByIdAsync(dossierId))!;
            await AdvanceStepAsync(dossier, 4);
            await _uow.CompleteAsync();

            return (true, null, new
            {
                documentId      = doc.Id,
                fileName        = doc.FileName,
                fileSizeDisplay = doc.FileSizeDisplay
            });
        }

        public async Task<bool> DeleteDocumentAsync(int dossierId, int documentId)
        {
            var doc = await _uow.ImmatriculationDocuments.GetByIdAsync(documentId);

            if (doc == null || doc.DossierId != dossierId)
                return false;

            _fileService.MarkFileDeleted(doc.FilePath ?? "");
            doc.IsActive = false;
            _uow.ImmatriculationDocuments.Update(doc);
            await _uow.CompleteAsync();
            return true;
        }

        // ════════════════════════════════════════════════════════════
        //  SUBMISSION
        // ════════════════════════════════════════════════════════════

        public async Task<string> SubmitAsync(int dossierId, DossierStep5Dto dto)
        {
            var dossier = (await _uow.Dossiers.GetByIdAsync(dossierId))!;

            // Generate DossierNumber
            var year   = DateTime.UtcNow.Year;
            var count  = await _uow.Dossiers.CountAsync(d =>
                d.DossierNumber != null &&
                d.DossierNumber.StartsWith($"DAM-IMMAT-{year}-"));
            var number = $"DAM-IMMAT-{year}-{(count + 1):D4}";

            dossier.DossierNumber        = number;
            dossier.Status               = "Soumis";
            dossier.CurrentStep          = 5;
            dossier.AttestationCity      = dto.AttestationCity;
            dossier.AttestationDate      = dto.AttestationDate;
            dossier.SignatoryName        = dto.SignatoryName;
            dossier.AttestationConfirmed = true;
            dossier.SubmittedAt          = DateTime.UtcNow;
            dossier.LastModifiedAt       = DateTime.UtcNow;

            _uow.Dossiers.Update(dossier);
            await _uow.CompleteAsync();

            return number;
        }

        // ════════════════════════════════════════════════════════════
        //  QUERIES
        // ════════════════════════════════════════════════════════════

        public async Task<ImmatriculationDossier?> GetEditableDossierAsync(int id)
        {
            var dossier = await _uow.Dossiers.GetByIdAsync(id);
            return dossier?.IsActive == true ? dossier : null;
        }

        public async Task<bool> AllRequiredDocsUploadedAsync(int dossierId)
        {
            var required = await _uow.ImmatriculationDocTypes
                .GetWhereAsync(t => t.IsRequired && t.IsActive);
            var uploaded = await _uow.ImmatriculationDocuments
                .GetWhereAsync(d => d.DossierId == dossierId && d.IsActive);

            var uploadedIds = uploaded.Select(u => u.DocumentTypeId).ToHashSet();
            return required.All(t => uploadedIds.Contains(t.Id));
        }

        public async Task<WizardProgressVm> GetProgressVmAsync(int dossierId)
        {
            var dossier = await _uow.Dossiers.GetByIdAsync(dossierId);
            return new WizardProgressVm
            {
                DossierId   = dossierId,
                CurrentStep = dossier?.CurrentStep ?? 1,
                Status      = dossier?.Status ?? "Brouillon"
            };
        }

        // ════════════════════════════════════════════════════════════
        //  PRIVATE — Step advancement
        // ════════════════════════════════════════════════════════════

        private async Task AdvanceStepAsync(ImmatriculationDossier dossier, int step)
        {
            dossier.CurrentStep    = Math.Max(dossier.CurrentStep, step);
            dossier.LastModifiedAt = DateTime.UtcNow;
            _uow.Dossiers.Update(dossier);
            // Caller must call SaveAsync()
            await Task.CompletedTask;
        }

        // ════════════════════════════════════════════════════════════
        //  PRIVATE — Mappers
        // ════════════════════════════════════════════════════════════

        private static DossierAuthority MapToAuthority(
            DossierStep1Dto dto, DossierAuthority entity)
        {
            entity.EmployingAuthorityId = dto.EmployingAuthorityId;
            entity.BaseAerienneId       = dto.BaseAerienneId;
            entity.OgmnNumber           = dto.OgmnNumber?.Trim();
            entity.OgmnAggrementDate    = dto.OgmnAggrementDate;
            entity.OgmnSousPartie       = dto.OgmnSousPartie;
            entity.OgmnResponsable      = dto.OgmnResponsable?.Trim();
            entity.AeAddress            = dto.AeAddress?.Trim();
            entity.AePhone              = dto.AePhone?.Trim();
            entity.AeEmail              = dto.AeEmail?.Trim();
            return entity;
        }

        private static DossierStep2Dto MapToStep2Dto(DossierAircraft entity) => new()
        {
            AircraftCategoryId    = entity.AircraftCategoryId,
            AcTypeId              = entity.AcTypeId,
            AircraftSerie         = entity.AircraftSerie,
            AircraftVersionId     = entity.AircraftVersionId,
            MissionRoleId         = entity.MissionRoleId,
            ManufacturerId        = entity.ManufacturerId,
            SerialNumber          = entity.SerialNumber,
            ManufactureDate       = entity.ManufactureDate,
            ServiceEntryDate      = entity.ServiceEntryDate,
            PortAttacheId         = entity.PortAttacheId,
            OriginCountryId       = entity.OriginCountryId,
            ImmatriculationSuffix = entity.ImmatriculationSuffix
        };

        private static DossierAircraft MapToAircraft(
            DossierStep2Dto dto, DossierAircraft entity)
        {
            entity.AircraftCategoryId    = dto.AircraftCategoryId;
            entity.AcTypeId              = dto.AcTypeId;
            entity.AircraftSerie         = dto.AircraftSerie?.Trim();
            entity.AircraftVersionId     = dto.AircraftVersionId;
            entity.MissionRoleId         = dto.MissionRoleId;
            entity.ManufacturerId        = dto.ManufacturerId;
            entity.SerialNumber          = dto.SerialNumber?.Trim();
            entity.ManufactureDate       = dto.ManufactureDate;
            entity.ServiceEntryDate      = dto.ServiceEntryDate;
            entity.PortAttacheId         = dto.PortAttacheId;
            entity.OriginCountryId       = dto.OriginCountryId;
            entity.ImmatriculationSuffix = dto.ImmatriculationSuffix?
                .Trim().ToUpper();
            return entity;
        }

        private static DossierStep3Dto MapToStep3Dto(DossierAirworthiness entity) => new()
        {
            HasAirworthinessDoc   = entity.HasAirworthinessDoc,
            CdnDocTypeId          = entity.CdnDocTypeId,
            CdnReference          = entity.CdnReference,
            CdnDeliveryDate       = entity.CdnDeliveryDate,
            CdnExpiryDate         = entity.CdnExpiryDate,
            CdnRenewalRequested   = entity.CdnRenewalRequested,
            WasForeignRegistered  = entity.WasForeignRegistered,
            ForeignCountryId      = entity.ForeignCountryId,
            FormerImmatriculation = entity.FormerImmatriculation,
            ForeignRadiationDate  = entity.ForeignRadiationDate
        };

        private static DossierAirworthiness MapToAirworthiness(
            DossierStep3Dto dto, DossierAirworthiness entity)
        {
            entity.HasAirworthinessDoc   = dto.HasAirworthinessDoc;
            entity.CdnDocTypeId          = dto.HasAirworthinessDoc
                                               ? dto.CdnDocTypeId : null;
            entity.CdnReference          = dto.CdnReference?.Trim();
            entity.CdnDeliveryDate       = dto.CdnDeliveryDate;
            entity.CdnExpiryDate         = dto.CdnExpiryDate;
            entity.CdnRenewalRequested   = dto.CdnRenewalRequested;
            entity.WasForeignRegistered  = dto.WasForeignRegistered;
            entity.ForeignCountryId      = dto.WasForeignRegistered
                                               ? dto.ForeignCountryId : null;
            entity.FormerImmatriculation = dto.FormerImmatriculation?.Trim();
            entity.ForeignRadiationDate  = dto.WasForeignRegistered
                                               ? dto.ForeignRadiationDate : null;
            return entity;
        }

        // ── Generic SelectList builder ────────────────────────────────
        private static IEnumerable<SelectListItem> BuildSelectList<T>(
            IEnumerable<T>   items,
            Func<T, string>  valueSelector,
            Func<T, string>  textSelector,
            string?          selectedValue,
            string           placeholder = "— Selectionner —")
        {
            var list = new List<SelectListItem>
            {
                new() { Value = "", Text = placeholder,
                        Selected = string.IsNullOrEmpty(selectedValue) }
            };

            list.AddRange(items.Select(item => new SelectListItem
            {
                Value    = valueSelector(item),
                Text     = textSelector(item),
                Selected = valueSelector(item) == selectedValue
            }));

            return list;
        }
    }
}
