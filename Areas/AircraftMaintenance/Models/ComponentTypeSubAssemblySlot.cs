using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW — eligibility rule for the recursive parent-child assembly tree
    /// (design doc: "Component Installation &amp; Hierarchical Tree").
    /// RESTRUCTURED this revision: this used to also carry SlotCode/SlotName/
    /// MaxCount directly (one table doing two jobs — slot definition AND
    /// per-PN eligibility — which let MaxCount drift out of sync between
    /// different eligible PNs for the same physical slot). Now purely "this
    /// child ComponentType is allowed to fill that Slot" — the slot itself
    /// (code, name, capacity) lives once in <see cref="ComponentTypeSlot"/>.
    ///
    /// One row = one (Slot, eligible child PN) combination. Several rows can
    /// share the same SlotId to allow more than one interchangeable child PN
    /// in the same physical slot (e.g. a hydraulic pump from two different
    /// manufacturers) — same multi-PN-per-slot shape ComponentTypePosition
    /// already uses for airframe positions.
    /// </summary>
    [Table("ComponentTypeSubAssemblySlotEligibilities", Schema = "dbo")]
    public class ComponentTypeSubAssemblySlot
    {
        public int Id { get; set; }

        [Required]
        public int SlotId { get; set; }
        [ForeignKey(nameof(SlotId))]
        public virtual ComponentTypeSlot? Slot { get; set; }

        [Required]
        public int ChildComponentTypeId { get; set; }
        [ForeignKey(nameof(ChildComponentTypeId))]
        public virtual ComponentType? ChildComponentType { get; set; }

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; }
    }
}
