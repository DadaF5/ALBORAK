using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Models; // ApplicationUser — confirmed namespace (WorkOrderJobCardSignOff.cs)
using FRAProject.Areas.Settings.Models; // Aircraft

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Append-only genealogy log. One row per install/removal/overhaul/etc.
    /// NEVER edited or deleted after creation — same "revoke-and-recreate, full
    /// history preserved" convention as UserAssignment. This table is the real
    /// source of truth; Component's Current* fields are a derived projection.
    /// </summary>
    [Table("ComponentEvents", Schema = "dbo")]
    public class ComponentEvent
    {
        public int Id { get; set; }

        [Required]
        public int ComponentId { get; set; }
        [ForeignKey(nameof(ComponentId))]
        public virtual Component? Component { get; set; }

        [Required]
        public ComponentEventType EventType { get; set; }

        [Required]
        public DateOnly EventDate { get; set; }

        /// <summary>Set for Install/Remove.</summary>
        public int? AircraftId { get; set; }
        [ForeignKey(nameof(AircraftId))]
        public virtual Aircraft? Aircraft { get; set; }

        /// <summary>Set for Install/Remove.</summary>
        public int? PositionId { get; set; }
        [ForeignKey(nameof(PositionId))]
        public virtual ComponentPosition? Position { get; set; }

        /// <summary>
        /// Revision 13: the per-dimension snapshot (used to be 4 fixed
        /// AircraftFHAtEventMinutes/AircraftCyclesAtEvent/
        /// AircraftTgoLandingsAtEvent/AircraftFullStopLandingsAtEvent columns)
        /// moved to the generic ComponentEventReading child table — see that
        /// file. Same convention as before (WorkOrder.OpenFH/CloseFH-style
        /// immutable snapshot, one row per dimension actually resolvable via
        /// IAircraftReadingProvider at the moment of this event); never
        /// recomputed retroactively even if the aircraft's readings are later
        /// corrected. A dimension with no source yet (e.g. a brand-new
        /// DimensionType nobody has wired IAircraftReadingProvider for) simply
        /// gets no row here for this event, same as it being null before.
        /// </summary>
        public virtual ICollection<ComponentEventReading> Readings { get; set; } = new List<ComponentEventReading>();

        /// <summary>Only set for EventType = Remove.</summary>
        public ComponentRemovalReason? RemovalReason { get; set; }

        /// <summary>
        /// NEW — set for EventType = AttachToParent/DetachFromParent: which
        /// parent Component this sub-assembly was attached to / detached from.
        /// Kept separate from ComponentId (the sub-assembly itself) so the log
        /// reads naturally: "Component X attached to RelatedParentComponent Y".
        /// FK to Component, Restrict delete (see ComponentEventConfiguration) —
        /// mirrors the same "never edited/deleted, immutable log" discipline
        /// as the rest of this entity.
        /// </summary>
        public int? RelatedParentComponentId { get; set; }
        [ForeignKey(nameof(RelatedParentComponentId))]
        public virtual Component? RelatedParentComponent { get; set; }

        /// <summary>NEW — genealogy-log copy of which slot on RelatedParentComponent this event attached to / detached from (see Component.CurrentSlotCode). Only set for AttachToParent/DetachFromParent.</summary>
        [StringLength(30)]
        public string? SlotCode { get; set; }

        /// <summary>
        /// Optional link to the WorkOrder this swap happened under. Plain nullable
        /// FK, not a junction — one WorkOrder can cause several ComponentEvents
        /// (e.g. both engines pulled in the same corrective WO), each event points
        /// back at the same WorkOrderId.
        /// </summary>
        public int? LinkedWorkOrderId { get; set; }
        [ForeignKey(nameof(LinkedWorkOrderId))]
        public virtual WorkOrder? LinkedWorkOrder { get; set; }

        [Required]
        public string PerformedByUserId { get; set; } = string.Empty;
        [ForeignKey(nameof(PerformedByUserId))]
        public virtual ApplicationUser? PerformedByUser { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
