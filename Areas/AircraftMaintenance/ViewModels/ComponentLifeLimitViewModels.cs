using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class ComponentLifeLimitProfileListDto
    {
        public int Id { get; set; }
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; }
        public string? SerialNumber { get; set; }
        public string? SerialNumberPrefix { get; set; }
        public string? SerialBoundary { get; set; }
        public string? Reason { get; set; }
        public ComponentLifeBasis LifeBasis { get; set; }
        public bool IsActive { get; set; }
        public int StageCount { get; set; }
    }

    public class ComponentLifeLimitProfileFormDto
    {
        public int? Id { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }

        [Required]
        [Display(Name = "Applicabilité")]
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; } = ApplicabilityRuleType.PnBased;

        [Display(Name = "Numéro de série (S/N)")]
        [StringLength(60)]
        public string? SerialNumber { get; set; } // required when Specific

        [Display(Name = "Préfixe S/N")]
        [StringLength(20)]
        public string? SerialNumberPrefix { get; set; } // RangeFrom/RangeTo

        [Display(Name = "Borne (numérique)")]
        [StringLength(30)]
        public string? SerialBoundary { get; set; } // RangeFrom/RangeTo

        [Display(Name = "Motif / référence")]
        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        [Display(Name = "Base du calcul")]
        public ComponentLifeBasis LifeBasis { get; set; } = ComponentLifeBasis.SinceNew;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        public List<ComponentLifeLimitStageFormDto> Stages { get; set; } = new();
    }

    /// <summary>
    /// Revision 13: the 15 fixed Interval*/BandEnd*/Tolerance* fields (one
    /// trio per hardcoded dimension) collapsed to a single dynamic
    /// Dimensions list — one row per dimension actually configured on this
    /// stage. Adding a future dimension (C130 APU starts, Canadair "number
    /// of Drops") needs NO change here: it just becomes another possible row,
    /// sourced from ComponentLifeLimitDimensionTypeOptionDto.
    /// </summary>
    public class ComponentLifeLimitStageFormDto
    {
        public int SequenceOrder { get; set; }

        [Required]
        [Display(Name = "Type d'étape")]
        public ComponentLifeLimitStageType StageType { get; set; } = ComponentLifeLimitStageType.Overhaul;

        [Required]
        [Display(Name = "Type de tolérance")]
        public ComponentToleranceType ToleranceType { get; set; } = ComponentToleranceType.Absolute;

        public List<ComponentLifeLimitStageDimensionFormDto> Dimensions { get; set; } = new();
    }

    /// <summary>
    /// One dimension row within a stage. Interval/BandEnd/Tolerance are kept
    /// in DISPLAY units here (decimal hours for an Hours-unit dimension —
    /// matches the pre-Revision-13 FH-in-hours convention — plain whole
    /// numbers for Count/Days) and converted to the entity's stored unit
    /// (minutes for Hours, as-is otherwise) by
    /// ComponentLifeLimitProfileService, the same way IntervalFHHours*60 used
    /// to be converted before this revision.
    /// </summary>
    public class ComponentLifeLimitStageDimensionFormDto
    {
        [Required]
        public int DimensionTypeId { get; set; }

        // Display-only, populated on read so the view can render the row
        // (dimension Code/Name/Unit) without a separate lookup — ignored on
        // POST, DimensionTypeId is the only field the service trusts.
        public string? DimensionTypeCode { get; set; }
        public string? DimensionTypeName { get; set; }
        public ComponentLifeLimitDimensionUnit Unit { get; set; }

        [Display(Name = "Intervalle")]
        public decimal? Interval { get; set; }
        [Display(Name = "Fin de palier")]
        public decimal? BandEnd { get; set; }
        [Display(Name = "Tolérance")]
        public decimal? Tolerance { get; set; }
    }

    /// <summary>NEW (Revision 13) — populates the "add a dimension to this stage" picker in the stage editor. One row per active ComponentLifeLimitDimensionType.</summary>
    public class ComponentLifeLimitDimensionTypeOptionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ComponentLifeLimitDimensionUnit Unit { get; set; }
        public bool IsCalendarBased { get; set; }
    }
}
