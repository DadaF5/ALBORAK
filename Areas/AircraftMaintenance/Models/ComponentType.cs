using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models; // AircraftManufacturer — ASSUMPTION, matches existing lookup namespace.

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Part-number-level catalog data (not a physical instance). One row per
    /// Part Number. Physical serialized instances are Component rows.
    /// </summary>
    [Table("ComponentTypes", Schema = "dbo")]
    public class ComponentType
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string PartNumber { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Nomenclature { get; set; } = string.Empty;

        public int? AtaId { get; set; }
        [ForeignKey(nameof(AtaId))]
        public virtual Ata? Ata { get; set; }

        public int? AircraftManufacturerId { get; set; }
        [ForeignKey(nameof(AircraftManufacturerId))]
        public virtual AircraftManufacturer? AircraftManufacturer { get; set; }

        [Required]
        public ComponentTrackingMethod TrackingMethod { get; set; } = ComponentTrackingMethod.OnCondition;

        /// <summary>
        /// Default true — most tracked aviation components carry a unique serial
        /// number. Left here (not hardcoded) in case a future non-serialized
        /// trackable item shows up; not exercised by v1 workflows.
        /// </summary>
        public bool IsSerialized { get; set; } = true;

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; }

        /// <summary>
        /// One or more staged life-limit schedules — which one applies to a
        /// given physical Component is resolved at runtime via each profile's
        /// ApplicabilityRuleType (see ComponentLifeLimitProfile). A PN can have
        /// zero profiles (pure OnCondition), one PN_BASED default, or several
        /// SN-specific ones with or without a PN_BASED fallback.
        /// </summary>
        public virtual ICollection<ComponentLifeLimitProfile> LifeLimitProfiles { get; set; } = new List<ComponentLifeLimitProfile>();
        public virtual ICollection<ComponentTypePosition> ComponentTypePositions { get; set; } = new HashSet<ComponentTypePosition>();
        public virtual ICollection<Component> Components { get; set; } = new HashSet<Component>();

        /// <summary>
        /// NEW — named sub-assembly slots where THIS ComponentType is the
        /// parent/host (e.g. Engine -> [DEEC slot, HYD_PUMP slot]). Each slot
        /// carries its own capacity (ComponentTypeSlot.MaxCount) and eligible
        /// child PN list (ComponentTypeSlot.EligibleChildren) — mirrors the
        /// ComponentTypePosition "which PN(s) may sit in which slot" pattern,
        /// one level down the tree.
        /// </summary>
        public virtual ICollection<ComponentTypeSlot> ChildSlots { get; set; } = new HashSet<ComponentTypeSlot>();

        /// <summary>
        /// NEW — reverse side: eligibility rules where THIS ComponentType is
        /// itself an eligible child of some other ComponentType's slot (e.g.
        /// the DEEC PN, looked up from the DEEC's own ComponentType row).
        /// </summary>
        public virtual ICollection<ComponentTypeSubAssemblySlot> EligibleAsChildIn { get; set; } = new HashSet<ComponentTypeSubAssemblySlot>();
    }
}
