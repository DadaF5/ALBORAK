using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// A full staged life-limit schedule for a ComponentType. A single PN can
    /// have more than one profile — which one applies to a given physical
    /// Component is resolved via ApplicabilityRuleType, same
    /// SPECIFIC > RANGE > PN_BASED priority as JobCardApplicability. Real
    /// example: PN-123abc/SN-xyz gets a SPECIFIC profile with its own schedule;
    /// PN-123abc/SN-lmn gets a different SPECIFIC profile; any other serial of
    /// PN-123abc falls back to the PN_BASED default profile if one exists,
    /// otherwise no hard-time limit applies to it.
    /// </summary>
    [Table("ComponentLifeLimitProfiles", Schema = "dbo")]
    public class ComponentLifeLimitProfile
    {
        public int Id { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }
        [ForeignKey(nameof(ComponentTypeId))]
        public virtual ComponentType? ComponentType { get; set; }

        [Required]
        public ApplicabilityRuleType ApplicabilityRuleType { get; set; } = ApplicabilityRuleType.PnBased;

        /// <summary>Required when ApplicabilityRuleType = Specific.</summary>
        [StringLength(60)]
        public string? SerialNumber { get; set; }

        /// <summary>Required when ApplicabilityRuleType = RangeFrom/RangeTo — e.g. "LM".</summary>
        [StringLength(20)]
        public string? SerialNumberPrefix { get; set; }

        /// <summary>Numeric part of the serial, as text (compared numerically, not lexically — see calculator). RangeFrom: this SN and above. RangeTo: this SN and below.</summary>
        [StringLength(30)]
        public string? SerialBoundary { get; set; }

        /// <summary>Verbatim source text for why this profile/boundary exists — same convention as JobCardApplicability.Reason (e.g. "SN LM4436 and up — per SB 2ABC-27-014").</summary>
        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        public ComponentLifeBasis LifeBasis { get; set; } = ComponentLifeBasis.SinceNew;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ComponentLifeLimitStage> Stages { get; set; } = new List<ComponentLifeLimitStage>();
    }
}
