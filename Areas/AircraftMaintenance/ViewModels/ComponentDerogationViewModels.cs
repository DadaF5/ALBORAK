using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Common.Validation;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    /// <summary>
    /// NEW — one row per ComponentDerogation, for the ManageDerogations list
    /// (mirrors ComponentLifeLimitProfileListDto/ManageLifeLimits.cshtml).
    /// Append-only — this list has no "Modifier" action, only "Nouvelle
    /// dérogation" and (read-only) history.
    /// </summary>
    public class ComponentDerogationListDto
    {
        public int Id { get; set; }
        public string? DimensionTypeCode { get; set; }
        public string? DimensionTypeName { get; set; }
        public ComponentLifeLimitStageType TargetStageType { get; set; }
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; }
        public string? SerialNumber { get; set; }
        public string? SerialNumberPrefix { get; set; }
        public string? SerialBoundary { get; set; }
        public string? LotReference { get; set; }
        public ComponentToleranceType Mode { get; set; }
        public DerogationDirection Direction { get; set; }
        public decimal Value { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateOnly IssuedDate { get; set; }
        public DateOnly? EffectiveUntil { get; set; }
        public DerogationTier? Tier { get; set; }
        public string? ApprovalAuthority { get; set; }
        public string? SupportingEvidence { get; set; }
        public bool IsConditional { get; set; }
        public string? ConditionDescription { get; set; }
        public bool IsActive { get; set; }
        public int? SupersedesDerogationId { get; set; }

        /// <summary>NEW — one human-readable line built server-side by ComponentDerogationService (e.g. "Extension : +24 mois (Absolue) — Réforme"), same "compute it once in the service, keep the view thin" convention as ComponentLifeLimitProfileListDto.StageSummaries.</summary>
        public string Summary { get; set; } = string.Empty;

        // NEW — Void action fields, populated only when IsActive = false.
        public string? VoidReason { get; set; }
        public DateTime? VoidedAtUtc { get; set; }
        public string? VoidedByUserName { get; set; }
    }

    /// <summary>NEW — Void action's posted form. Deliberately minimal (id + reason only) — every other field on a voided row is untouched, per the "Void flips IsActive + stamps who/when/why, nothing else" rule.</summary>
    public class ComponentDerogationVoidDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }

        [Required(ErrorMessage = "Le motif d'annulation est requis.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>NEW — Create form. No Edit form ships this revision — append-only, same convention as ComponentEvent (see ComponentDerogation.cs class doc comment).</summary>
    public class ComponentDerogationFormDto
    {
        [Required]
        public int ComponentTypeId { get; set; }

        [Required]
        [Display(Name = "Dimension concernée")]
        public int DimensionTypeId { get; set; }

        [Required]
        [Display(Name = "Palier ciblé")]
        public ComponentLifeLimitStageType TargetStageType { get; set; } = ComponentLifeLimitStageType.Retirement;

        [Required]
        [Display(Name = "Applicabilité")]
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; } = ApplicabilityRuleType.PnBased;

        [Display(Name = "Numéro de série (S/N)")]
        [StringLength(60)]
        public string? SerialNumber { get; set; }

        [Display(Name = "Préfixe S/N")]
        [StringLength(20)]
        public string? SerialNumberPrefix { get; set; }

        [Display(Name = "Borne (numérique)")]
        [StringLength(30)]
        public string? SerialBoundary { get; set; }

        [Display(Name = "Référence de lot")]
        [StringLength(60)]
        public string? LotReference { get; set; }

        [Required]
        [Display(Name = "Mode")]
        public ComponentToleranceType Mode { get; set; } = ComponentToleranceType.Absolute;

        [Required]
        [Display(Name = "Sens")]
        public DerogationDirection Direction { get; set; } = DerogationDirection.Extension;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La valeur doit être positive.")]
        [Display(Name = "Valeur")]        
        public decimal Value { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Référence (document/directive)")]
        public string Reference { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Motif")]
        public string? Reason { get; set; }

        [Required]
        [Display(Name = "Date d'émission")]
        [DataType(DataType.Date)]
        [NotFutureDate]
        [NotBefore(1940, 1, 1)]
        public DateOnly IssuedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Valide jusqu'au")]
        [DataType(DataType.Date)]
        [NotBefore(1940, 1, 1)] // no NotFutureDate here — an expiry is normally in the future
        public DateOnly? EffectiveUntil { get; set; }

        [Display(Name = "Catégorie (type TCTO)")]
        public DerogationTier? Tier { get; set; }

        [StringLength(200)]
        [Display(Name = "Autorité d'approbation")]
        public string? ApprovalAuthority { get; set; }

        [StringLength(500)]
        [Display(Name = "Justification / élément probant")]
        public string? SupportingEvidence { get; set; }

        [Display(Name = "Conditionnelle")]
        public bool IsConditional { get; set; }

        [StringLength(500)]
        [Display(Name = "Description de la condition")]
        public string? ConditionDescription { get; set; }

        [Display(Name = "Corrige/annule la dérogation")]
        public int? SupersedesDerogationId { get; set; }

        // Display-only, populated on read so a picker with a pre-selected
        // dimension can show its label without a separate lookup — ignored
        // on POST, same "trust only the Id" convention as
        // ComponentLifeLimitStageDimensionFormDto.
        public string? DimensionTypeCode { get; set; }
        public string? DimensionTypeName { get; set; }
    }
}
