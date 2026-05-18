using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ════════════════════════════════════════════════════════════════
    //  STEP 1 DTO — DossierAuthority
    //  Maps to: DossierAuthority model
    //  FKs:     EmployingAuthorityId, BaseAerienneId
    // ════════════════════════════════════════════════════════════════
    public class DossierStep1Dto
    {
        /// <summary>ImmatriculationDossier.Id — 0 on first POST.</summary>
        public int DossierId { get; set; }

        [Required(ErrorMessage = "L'autorite d'emploi est obligatoire.")]
        [Display(Name = "Autorite d'emploi")]
        public int? EmployingAuthorityId { get; set; }

        [Required(ErrorMessage = "La base aerienne est obligatoire.")]
        [Display(Name = "Base aerienne (BAFRA)")]
        public int? BaseAerienneId { get; set; }

        [Required(ErrorMessage = "Le numero OGMN est obligatoire.")]
        [StringLength(30)]
        [Display(Name = "N Agrement OGMN")]
        public string? OgmnNumber { get; set; }

        [Required(ErrorMessage = "La date d'agrement est obligatoire.")]
        [Display(Name = "Date d'agrement OGMN")]
        public DateOnly? OgmnAggrementDate { get; set; }

        [Display(Name = "Sous-partie agrement")]
        public string? OgmnSousPartie { get; set; }

        [StringLength(100)]
        [Display(Name = "Responsable OGMN")]
        public string? OgmnResponsable { get; set; }

        [Required(ErrorMessage = "L'adresse est obligatoire.")]
        [StringLength(200)]
        [Display(Name = "Adresse postale")]
        public string? AeAddress { get; set; }

        [StringLength(30)]
        [Display(Name = "Telephone")]
        public string? AePhone { get; set; }

        [EmailAddress(ErrorMessage = "Format email invalide.")]
        [StringLength(100)]
        [Display(Name = "Messagerie electronique")]
        public string? AeEmail { get; set; }

        // ── Dropdowns ─────────────────────────────────────────────
        public IEnumerable<SelectListItem> AuthorityOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions      { get; set; } = [];

        // ── Radio options — static ─────────────────────────────────
        public static readonly string[] SousPartieOptions = ["G", "G+I", "Autre"];
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 2 DTO — DossierAircraft
    //  Maps to: DossierAircraft model
    //  FKs:     AircraftCategoryId, AcTypeId, AircraftVersionId,
    //           MissionRoleId, ManufacturerId, PortAttacheId,
    //           OriginCountryId
    //  AJAX:    AcTypeOptions, VersionOptions, MissionRoleOptions
    //           cascade from AircraftCategoryId / AcTypeId
    // ════════════════════════════════════════════════════════════════
    public class DossierStep2Dto
    {
        public int DossierId { get; set; }

        [Required(ErrorMessage = "La categorie d'aeronef est obligatoire.")]
        [Display(Name = "Categorie")]
        public int? AircraftCategoryId { get; set; }

        [Required(ErrorMessage = "Le type d'aeronef est obligatoire.")]
        [Display(Name = "Type d'aeronef")]
        public int? AcTypeId { get; set; }

        [StringLength(50)]
        [Display(Name = "Serie dans le type")]
        public string? AircraftSerie { get; set; }

        [Display(Name = "Version / Variante")]
        public int? AircraftVersionId { get; set; }

        [Display(Name = "Role et mission")]
        public int? MissionRoleId { get; set; }

        [Required(ErrorMessage = "Le constructeur est obligatoire.")]
        [Display(Name = "Constructeur")]
        public int? ManufacturerId { get; set; }

        [Required(ErrorMessage = "Le numero de serie est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "N de serie constructeur")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "La date de fabrication est obligatoire.")]
        [Display(Name = "Date de fabrication")]
        public DateOnly? ManufactureDate { get; set; }

        [Display(Name = "Date d'arrivee en service")]
        public DateOnly? ServiceEntryDate { get; set; }

        [Required(ErrorMessage = "Le port d'attache est obligatoire.")]
        [Display(Name = "Port d'attache")]
        public int? PortAttacheId { get; set; }

        [Display(Name = "Pays d'origine")]
        public int? OriginCountryId { get; set; }

        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "L'immatriculation doit contenir exactement 3 lettres.")]
        [RegularExpression(@"^[A-Za-z]{3}$",
            ErrorMessage = "L'immatriculation ne doit contenir que des lettres.")]
        [Display(Name = "Immatriculation (suffixe CN-***)")]
        public string? ImmatriculationSuffix { get; set; }

        // ── Static dropdowns — loaded on GET ──────────────────────
        public IEnumerable<SelectListItem> CategoryOptions     { get; set; } = [];
        public IEnumerable<SelectListItem> ManufacturerOptions { get; set; } = [];
        public IEnumerable<SelectListItem> PortAttacheOptions  { get; set; } = [];
        public IEnumerable<SelectListItem> CountryOptions      { get; set; } = [];

        // ── AJAX-populated dropdowns ──────────────────────────────
        // Pre-filled on GET from saved values.
        // Replaced by JS fetch on category or type change.
        public IEnumerable<SelectListItem> AcTypeOptions      { get; set; } = [];
        public IEnumerable<SelectListItem> VersionOptions     { get; set; } = [];
        public IEnumerable<SelectListItem> MissionRoleOptions { get; set; } = [];
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 3 DTO — DossierAirworthiness
    //  Maps to: DossierAirworthiness model
    //  FKs:     CdnDocTypeId, ForeignCountryId
    //  NOTE:    ForeignCountryId lives HERE not in Step 2
    //           — it belongs to registration history,
    //             not aircraft identity.
    // ════════════════════════════════════════════════════════════════
    public class DossierStep3Dto
    {
        public int DossierId { get; set; }

        [Display(Name = "Document de navigabilite disponible")]
        public bool HasAirworthinessDoc { get; set; } = false;

        // Conditional — required only when HasAirworthinessDoc = true
        // Enforced in controller, not annotation
        [Display(Name = "Type de document")]
        public int? CdnDocTypeId { get; set; }

        [StringLength(50)]
        [Display(Name = "Reference du document")]
        public string? CdnReference { get; set; }

        [Display(Name = "Date de delivrance")]
        public DateOnly? CdnDeliveryDate { get; set; }

        [Display(Name = "Date d'expiration")]
        public DateOnly? CdnExpiryDate { get; set; }

        [Display(Name = "Demande de delivrance de CdN associee")]
        public bool CdnRenewalRequested { get; set; } = false;

        [Display(Name = "Immatriculation etrangere anterieure")]
        public bool WasForeignRegistered { get; set; } = false;

        // Conditional — required only when WasForeignRegistered = true
        [Display(Name = "Etat / Registre d'origine")]
        public int? ForeignCountryId { get; set; }

        [StringLength(30)]
        [Display(Name = "Ancienne immatriculation")]
        public string? FormerImmatriculation { get; set; }

        [Display(Name = "Date de radiation du registre etranger")]
        public DateOnly? ForeignRadiationDate { get; set; }

        // ── Dropdowns ─────────────────────────────────────────────
        public IEnumerable<SelectListItem> CdnDocTypeOptions { get; set; } = [];
        public IEnumerable<SelectListItem> CountryOptions    { get; set; } = [];
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 4 DTO — Documents (ImmatriculationDocument)
    //  No scalar fields — documents are child rows.
    // ════════════════════════════════════════════════════════════════
    public class DossierStep4Dto
    {
        public int DossierId { get; set; }

        /// <summary>
        /// One slot per ImmatriculationDocType (6 total).
        /// Built by controller from seed data + existing uploads.
        /// </summary>
        public List<DocumentSlotVm> Slots { get; set; } = [];

        /// <summary>True when all IsRequired slots have uploads.</summary>
        public bool AllRequiredUploaded =>
            Slots.Where(s => s.IsRequired).All(s => s.IsUploaded);
    }

    /// <summary>
    /// One upload slot — combines document type definition
    /// with current upload status.
    /// </summary>
    public class DocumentSlotVm
    {
        // From ImmatriculationDocType
        public int     DocumentTypeId   { get; set; }
        public string  Code             { get; set; } = string.Empty;
        public string  Name             { get; set; } = string.Empty;
        public string? ArticleReference { get; set; }
        public bool    IsRequired       { get; set; }
        public string? AcceptedFormats  { get; set; }
        public int?    MaxFileSizeMb    { get; set; }

        // From ImmatriculationDocument (null = not yet uploaded)
        public int?    DocumentId       { get; set; }
        public string? FileName         { get; set; }
        public string? FileSizeDisplay  { get; set; }

        public bool IsUploaded => DocumentId.HasValue;

        /// <summary>HTML accept attribute — "PDF,JPG" to ".pdf,.jpg"</summary>
        public string AcceptAttribute =>
            string.IsNullOrWhiteSpace(AcceptedFormats)
                ? "*"
                : string.Join(",",
                    AcceptedFormats
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => $".{f.Trim().ToLower()}"));

        public string RequirementBadgeClass =>
            IsRequired ? "bg-danger" : "bg-secondary";

        public string RequirementLabel =>
            IsRequired ? "Obligatoire" : "Si applicable";
    }

    // ════════════════════════════════════════════════════════════════
    //  STEP 5 DTO — Attestation (on master ImmatriculationDossier)
    // ════════════════════════════════════════════════════════════════
    public class DossierStep5Dto
    {
        public int DossierId { get; set; }

        [Required(ErrorMessage = "La ville est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Fait a")]
        public string? AttestationCity { get; set; }

        [Required(ErrorMessage = "La date est obligatoire.")]
        [Display(Name = "Date")]
        public DateOnly? AttestationDate { get; set; }

        [Required(ErrorMessage = "Le nom du signataire est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom, grade et fonction du signataire")]
        public string? SignatoryName { get; set; }

        [Display(Name = "Attestation du demandeur")]
        [Range(typeof(bool), "true", "true",
            ErrorMessage = "Vous devez confirmer l'attestation avant de soumettre.")]
        public bool AttestationConfirmed { get; set; } = false;

        // ── Read-only summary — populated by controller ────────────
        // Built from the 3 child models — never posted back.
        public string?  FullImmatriculation { get; set; }
        public string?  AircraftTypeName    { get; set; }
        public string?  AuthorityName       { get; set; }
        public string?  OgmnNumber          { get; set; }
        public string?  SerialNumber        { get; set; }
        public int      UploadedDocCount    { get; set; }
        public int      RequiredDocCount    { get; set; }
        public bool     AllRequiredUploaded => UploadedDocCount >= RequiredDocCount;
    }

    // ════════════════════════════════════════════════════════════════
    //  LIST ITEM VM
    // ════════════════════════════════════════════════════════════════
    public class DossierListVm
    {
        public int      Id                  { get; set; }
        public string?  DossierNumber       { get; set; }
        public string   Status              { get; set; } = string.Empty;
        public int      CurrentStep         { get; set; }
        public string?  FullImmatriculation { get; set; }
        public string?  AcTypeName          { get; set; }
        public string?  AuthorityName       { get; set; }
        public string?  OgmnNumber          { get; set; }
        public DateTime  CreatedAt          { get; set; }
        public DateTime? SubmittedAt        { get; set; }
        public bool      IsEditable         { get; set; }

        public string StatusBadgeClass => Status switch
        {
            "Brouillon"  => "bg-secondary",
            "Soumis"     => "bg-primary",
            "En examen"  => "bg-warning text-dark",
            "Approuve"   => "bg-success",
            "Rejete"     => "bg-danger",
            _            => "bg-secondary"
        };

        public string StepLabel => CurrentStep switch
        {
            1 => "Autorite d'emploi",
            2 => "Identification",
            3 => "Navigabilite",
            4 => "Documents",
            5 => "Validation",
            _ => "—"
        };
    }

    // ════════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    // ════════════════════════════════════════════════════════════════
    public class DossierIndexVm
    {
        public List<DossierListVm> Items      { get; set; } = [];
        public int                 TotalCount { get; set; }
        public int                 TotalPages { get; set; }

        public string? SearchNumber { get; set; }
        public string? SearchImmat  { get; set; }
        public string? SearchStatus { get; set; }

        public string SortColumn    { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

        public static readonly string[] AllStatuses =
            ["Brouillon", "Soumis", "En examen", "Approuve", "Rejete"];

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage     => PageNumber < TotalPages;
        public int  FirstItem       => TotalCount == 0
                                           ? 0
                                           : (PageNumber - 1) * PageSize + 1;
        public int  LastItem        => Math.Min(PageNumber * PageSize, TotalCount);

        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        public string SortIconFor(string column) =>
            SortColumn != column     ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up"
                                     : "fa-sort-down";
    }

    // ════════════════════════════════════════════════════════════════
    //  WIZARD PROGRESS VM
    //  Passed to _WizardProgress partial on every step view.
    //  CurrentStep read from ImmatriculationDossier master.
    // ════════════════════════════════════════════════════════════════
    public class WizardProgressVm
    {
        public int    DossierId   { get; set; }
        public int    CurrentStep { get; set; }
        public string Status      { get; set; } = "Brouillon";

        public static readonly string[] StepLabels =
        [
            "Autorite d'emploi",   // 1
            "Identification",       // 2
            "Navigabilite",         // 3
            "Documents",            // 4
            "Validation"            // 5
        ];

        /// <summary>
        /// CSS state for each step button.
        ///   done    → completed (back nav allowed)
        ///   active  → current step
        ///   pending → not yet reached
        /// </summary>
        public string StepState(int step) =>
            step < CurrentStep    ? "done"
            : step == CurrentStep ? "active"
            : "pending";

        /// <summary>
        /// True when user can click back to a previous step.
        /// Only while Status = Brouillon.
        /// </summary>
        public bool CanNavigateTo(int step) =>
            Status == "Brouillon" && step <= CurrentStep;
    }
}
