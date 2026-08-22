using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (normalized out of ComponentTypeSubAssemblySlot) — the SLOT
    /// DEFINITION for the recursive parent-child assembly tree: one named,
    /// capacity-limited physical location on a parent ComponentType, e.g.
    /// ComponentType "F-16 Engine" defines slots "DEEC" (MaxCount 1) and
    /// "HYD_PUMP" (MaxCount 2). Capacity is a property of the physical slot
    /// itself, not of whichever interchangeable part happens to fill it —
    /// this is what the original single-table design got wrong (MaxCount was
    /// duplicated on every eligible-PN row, so two different eligible PNs for
    /// the same slot could disagree on capacity). Which PN(s) are allowed to
    /// fill this slot is a separate concern, in
    /// <see cref="ComponentTypeSubAssemblySlot"/> — pure eligibility rows
    /// pointing back at this slot.
    /// </summary>
    [Table("ComponentTypeSlots", Schema = "dbo")]
    public class ComponentTypeSlot
    {
        public int Id { get; set; }

        [Required]
        public int ParentComponentTypeId { get; set; }
        [ForeignKey(nameof(ParentComponentTypeId))]
        public virtual ComponentType? ParentComponentType { get; set; }

        /// <summary>Short mnemonic identifying this slot on the parent, e.g. "DEEC", "HYD_PUMP", "HYD_PUMP_L". Unique per parent ComponentType.</summary>
        [Required, StringLength(30)]
        public string SlotCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string SlotName { get; set; } = string.Empty;

        /// <summary>Physical capacity of this named slot on the parent (e.g. 1 for DEEC, 2 for a twin-hydraulic-pump slot). Default 1. Enforced by ComponentService.AttachToParentAsync by counting the parent's current ChildComponents sharing this SlotCode.</summary>
        public byte MaxCount { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; }

        /// <summary>Which child ComponentType(s) are eligible to fill this slot — several rows here means several interchangeable PNs (e.g. from different manufacturers) all fit the same physical slot.</summary>
        public virtual ICollection<ComponentTypeSubAssemblySlot> EligibleChildren { get; set; } = new HashSet<ComponentTypeSubAssemblySlot>();
    }
}
