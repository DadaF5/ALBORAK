using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Common.Validation;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class ComponentListDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Nomenclature { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public ComponentStatus Status { get; set; }
        public string? LocationLabel { get; set; } // aircraft+position, or stock base
        public ComponentLifeStatusValue LifeStatus { get; set; }

        /// <summary>
        /// Revision 13: the 5 fixed Remaining* fields collapsed to one
        /// "headline" dimension — mirrors ComponentLifeStatus.Driving*
        /// (see that entity's doc comment). Null DrivingDimensionCode means
        /// no dimension currently constrains this component (Unknown/
        /// NotLifeLimited, or a matched profile with no usable stage data).
        /// </summary>
        public string? DrivingDimensionCode { get; set; }
        public string? DrivingDimensionName { get; set; }
        public FRAProject.Areas.AircraftMaintenance.Models.ComponentLifeLimitDimensionUnit? DrivingDimensionUnit { get; set; }
        /// <summary>Remaining, in DISPLAY units (decimal hours for an Hours-unit dimension, plain otherwise) — see ComponentLifeLimitProfileService's ToDisplayValue for the same convention.</summary>
        public decimal? DrivingDimensionRemainingDisplay { get; set; }

        public int MissedOverhaulCount { get; set; }
        public bool LifeLimitExceeded { get; set; }

        /// <summary>NEW — true when an active ComponentDerogation was applied while computing this row's status — see ComponentLifeStatus.HasActiveDerogation.</summary>
        public bool HasActiveDerogation { get; set; }

        /// <summary>NEW — true when this Component is attached as a sub-assembly to another Component (ParentComponentId set) rather than being a root/top-level item.</summary>
        public bool IsSubAssembly { get; set; }
        /// <summary>NEW — "P/N — S/N" label of the parent Component, set only when IsSubAssembly.</summary>
        public string? ParentLabel { get; set; }
        /// <summary>NEW — count of direct children currently attached (0 for a component with no sub-assemblies).</summary>
        public int ChildCount { get; set; }
    }

    public class ComponentReceiptDto
    {
        [Required(ErrorMessage = "Le type de composant est requis.")]
        [Display(Name = "Numéro de pièce (P/N)")]
        public int ComponentTypeId { get; set; }

        [Required(ErrorMessage = "Le numéro de série est requis.")]
        [StringLength(60)]
        [Display(Name = "Numéro de série (S/N)")]
        public string SerialNumber { get; set; } = string.Empty;

        [Display(Name = "Date de fabrication")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly? ManufactureDate { get; set; }

        [Required(ErrorMessage = "La base de stockage est requise.")]
        [Display(Name = "Base de stockage")]
        public int StockBaseId { get; set; }

        [Required]
        [Display(Name = "Date de réception")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }

        // ── NEW (Revision 12), generic per-dimension since Revision 13 —
        // opening reading, only relevant for a part received with
        // pre-existing usage (used/serviceable transfer-in). An empty/all-
        // zero InitialValues list, a component behaves exactly as before
        // (starts at zero). One row per non-calendar dimension the receiving
        // tech chooses to fill in — CALENDAR_DAYS deliberately never appears
        // here, see ComponentInitialReading's doc comment.

        [Display(Name = "Déjà révisé avant réception")]
        public bool HasPriorOverhaul { get; set; }

        [Display(Name = "Nombre de révisions antérieures")]
        public int? PriorOverhaulCount { get; set; }

        [Display(Name = "Date de la dernière révision antérieure")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly? PriorLastOverhaulDate { get; set; }

        public List<ComponentInitialReadingValueFormDto> InitialValues { get; set; } = new();
    }

    /// <summary>
    /// NEW (Revision 13) — one dimension's opening balance on the Receipt
    /// form. Interval/InitialValue/PriorSinceOverhaulValue are in DISPLAY
    /// units (decimal hours for an Hours-unit dimension, plain otherwise) —
    /// same convention as ComponentLifeLimitStageDimensionFormDto, converted
    /// to stored units (minutes for Hours) by ComponentService.ReceiptAsync.
    /// </summary>
    public class ComponentInitialReadingValueFormDto
    {
        [Required]
        public int DimensionTypeId { get; set; }

        // Display-only, populated on read — ignored on POST.
        public string? DimensionTypeCode { get; set; }
        public string? DimensionTypeName { get; set; }
        public FRAProject.Areas.AircraftMaintenance.Models.ComponentLifeLimitDimensionUnit Unit { get; set; }

        [Display(Name = "Valeur initiale")]
        public decimal? InitialValue { get; set; }

        [Display(Name = "Valeur depuis la dernière révision antérieure")]
        public decimal? PriorSinceOverhaulValue { get; set; }
    }

    public class ComponentInstallDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required(ErrorMessage = "L'aéronef est requis.")]
        [Display(Name = "Aéronef")]
        public int AircraftId { get; set; }

        [Required(ErrorMessage = "La position est requise.")]
        [Display(Name = "Position")]
        public int PositionId { get; set; }

        [Required]
        [Display(Name = "Date d'installation")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Ordre de travail lié")]
        public int? LinkedWorkOrderId { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }

    public class ComponentRemoveDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required(ErrorMessage = "Le motif de dépose est requis.")]
        [Display(Name = "Motif de dépose")]
        public ComponentRemovalReason RemovalReason { get; set; }

        [Required]
        [Display(Name = "Date de dépose")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "La destination est requise.")]
        [Display(Name = "Destination")]
        public ComponentStatus Destination { get; set; } = ComponentStatus.InStock; // InStock or UnderRepair only

        [Required(ErrorMessage = "La base de stockage est requise.")]
        [Display(Name = "Base de stockage")]
        public int StockBaseId { get; set; }

        [Display(Name = "Ordre de travail lié")]
        public int? LinkedWorkOrderId { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }

    /// <summary>NEW — attach a sub-assembly (e.g. a DEEC) onto a parent Component (e.g. an Engine). See design doc §2, "Component Installation &amp; Hierarchical Tree".</summary>
    public class ComponentAttachToParentDto
    {
        /// <summary>The sub-assembly being attached (the child).</summary>
        [Required]
        public int ComponentId { get; set; }

        [Required(ErrorMessage = "Le composant parent est requis.")]
        [Display(Name = "Composant parent (hôte)")]
        public int ParentComponentId { get; set; }

        [Required(ErrorMessage = "L'emplacement (slot) est requis.")]
        [Display(Name = "Emplacement (slot)")]
        public string SlotCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date d'attache")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Ordre de travail lié")]
        public int? LinkedWorkOrderId { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }

    /// <summary>NEW — detach a sub-assembly from its current parent Component. Feeds Cannibalization by pairing with a later AttachToParent onto a different parent — same "Remove then Install elsewhere" shape as the top-level Remove/Install pair.</summary>
    public class ComponentDetachFromParentDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required]
        [Display(Name = "Date de détache")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        /// <summary>Optional — set to Cannibalization when this detach is a tactical Canni pull rather than routine maintenance access.</summary>
        [Display(Name = "Motif")]
        public ComponentRemovalReason? RemovalReason { get; set; }

        [Display(Name = "Ordre de travail lié")]
        public int? LinkedWorkOrderId { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// NEW — admin form for a ComponentTypeSlot DEFINITION (code/name/capacity
    /// only — no PN here). Which PN(s) are eligible for this slot is managed
    /// separately via ComponentTypeSlotEligibilityFormDto, on a dedicated
    /// "manage eligible parts" sub-page (ComponentTypesController.
    /// ManageSlotEligibility), same reasoning as splitting the two entities:
    /// capacity is a property of the slot, eligible PNs are a variable-length
    /// list under it, not one form.
    /// </summary>
    public class ComponentTypeSlotFormDto
    {
        public int? Id { get; set; }

        [Required]
        public int ParentComponentTypeId { get; set; }

        [Required(ErrorMessage = "Le code d'emplacement est requis.")]
        [StringLength(30)]
        [Display(Name = "Code emplacement (ex: DEEC, HYD_PUMP)")]
        public string SlotCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom d'emplacement est requis.")]
        [StringLength(100)]
        [Display(Name = "Nom de l'emplacement")]
        public string SlotName { get; set; } = string.Empty;

        [Range(1, 255)]
        [Display(Name = "Capacité max de l'emplacement")]
        public byte MaxCount { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; }
    }

    /// <summary>NEW — adds one eligible child PN to an existing ComponentTypeSlot (ComponentTypesController.AddEligiblePart).</summary>
    public class ComponentTypeSlotEligibilityFormDto
    {
        [Required]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "Le numéro de pièce enfant éligible est requis.")]
        [Display(Name = "Numéro de pièce enfant éligible")]
        public int ChildComponentTypeId { get; set; }
    }

    /// <summary>NEW — one row in a parent's attached-sub-assemblies tree (Details view).</summary>
    public class ComponentChildViewModel
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Nomenclature { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public ComponentLifeStatusValue LifeStatus { get; set; }
        public bool LifeLimitExceeded { get; set; }
    }

    /// <summary>
    /// NEW — per-slot readiness breakdown for a parent Component INSTANCE
    /// (Details view), one row per slot defined on that Component's
    /// ComponentType — "Supported 2 / Installed 1", listing the actual PN/SN
    /// of whatever's currently filling the slot and how many/which
    /// interchangeable PNs are eligible to fill the rest.
    /// </summary>
    public class ComponentSlotStatusViewModel
    {
        public string SlotCode { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public byte MaxCount { get; set; }
        /// <summary>"P/N — Nomenclature" for every active eligible child PN, regardless of whether it's currently installed — this is the "what's supported" list.</summary>
        public List<string> SupportedPartNumbers { get; set; } = new();
        public int InstalledCount { get; set; }
        /// <summary>MaxCount - InstalledCount, clamped at 0.</summary>
        public int MissingCount { get; set; }
        public List<ComponentChildViewModel> InstalledChildren { get; set; } = new();
    }

    public class ComponentOverhaulDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required]
        [Display(Name = "Date de révision")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }

    public class ComponentScrapDto
    {
        [Required]
        public int ComponentId { get; set; }

        [Required]
        [Display(Name = "Date de réforme")]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Le motif est requis.")]
        [StringLength(500)]
        [Display(Name = "Motif")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ComponentHistoryItemViewModel
    {
        public ComponentEventType EventType { get; set; }
        public DateOnly EventDate { get; set; }
        public string? AircraftLabel { get; set; }
        public string? PositionLabel { get; set; }
        /// <summary>Revision 13: the 4 fixed AircraftXxxAtEvent fields collapsed to one row-per-dimension list — see ComponentEvent.Readings.</summary>
        public List<ComponentEventReadingItemViewModel> Readings { get; set; } = new();
        public ComponentRemovalReason? RemovalReason { get; set; }
        public string? LinkedWorkOrderNumber { get; set; }
        public string PerformedByUserName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        /// <summary>NEW — "P/N — S/N" label of the related parent Component, set only for AttachToParent/DetachFromParent rows.</summary>
        public string? RelatedParentComponentLabel { get; set; }
    }

    /// <summary>NEW (Revision 13) — one dimension's snapshot value on a ComponentHistoryItemViewModel row. Value is in DISPLAY units.</summary>
    public class ComponentEventReadingItemViewModel
    {
        public string DimensionCode { get; set; } = string.Empty;
        public string DimensionName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    /// <summary>Row shape for the unified DueList/DamDashboard view (see integration guide §Unify).</summary>
    public class ComponentDueListItemViewModel
    {
        public int ComponentId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Nomenclature { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string? AircraftRegistration { get; set; }
        public string? PositionLabel { get; set; }
        public ComponentLifeStatusValue Status { get; set; }

        /// <summary>Revision 13: same "headline" collapse as ComponentListDto — see that DTO's doc comment.</summary>
        public string? DrivingDimensionCode { get; set; }
        public string? DrivingDimensionName { get; set; }
        public FRAProject.Areas.AircraftMaintenance.Models.ComponentLifeLimitDimensionUnit? DrivingDimensionUnit { get; set; }
        public decimal? DrivingDimensionRemainingDisplay { get; set; }

        public int MissedOverhaulCount { get; set; }
        public bool LifeLimitExceeded { get; set; }

        /// <summary>NEW — see ComponentLifeStatus.HasActiveDerogation.</summary>
        public bool HasActiveDerogation { get; set; }
    }
}
