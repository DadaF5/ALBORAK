using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models; // Aircraft, Base — ASSUMPTION, matches existing lookup namespace.

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// A single physical, serialized part instance (PartNumber + SerialNumber).
    /// Current location/status is a denormalized "current state" projection —
    /// the source of truth for history is the ComponentEvent log, never this
    /// row's past values (this row only ever reflects "now").
    /// Unique (ComponentTypeId, SerialNumber) is enforced via Fluent API in
    /// ComponentConfiguration, not a data annotation here — avoid duplicating it.
    /// </summary>
    [Table("Components", Schema = "dbo")]
    public class Component
    {
        public int Id { get; set; }

        [Required]
        public int ComponentTypeId { get; set; }
        [ForeignKey(nameof(ComponentTypeId))]
        public virtual ComponentType? ComponentType { get; set; }

        [Required, StringLength(60)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public ComponentStatus Status { get; set; } = ComponentStatus.InStock;

        /// <summary>Set when Status = InStock or UnderRepair — "where is it sitting" for Base-level scoping.</summary>
        public int? StockBaseId { get; set; }
        [ForeignKey(nameof(StockBaseId))]
        public virtual Base? StockBase { get; set; }

        /// <summary>Set when Status = Installed.</summary>
        public int? CurrentAircraftId { get; set; }
        [ForeignKey(nameof(CurrentAircraftId))]
        public virtual Aircraft? CurrentAircraft { get; set; }

        /// <summary>Set when Status = Installed.</summary>
        public int? CurrentPositionId { get; set; }
        [ForeignKey(nameof(CurrentPositionId))]
        public virtual ComponentPosition? CurrentPosition { get; set; }

        public DateOnly? ManufactureDate { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// NEW — recursive parent-child assembly tree (design doc: "Component
        /// Installation &amp; Hierarchical Tree"). Null = this Component is a
        /// top-level/root item, positioned normally via CurrentAircraftId/
        /// CurrentPositionId/StockBaseId. Non-null = this Component is a
        /// sub-assembly attached to another Component (e.g. a DEEC or Fuel
        /// Pump attached to an Engine); its own CurrentAircraftId/
        /// CurrentPositionId/StockBaseId are then left null and its effective
        /// location/status is derived by walking ParentComponentId up to the
        /// root (see ComponentService.ResolveEffectiveLocationAsync). Self-FK,
        /// Restrict delete (configured in ComponentConfiguration) — a parent
        /// with children attached cannot be deleted outright; detach children
        /// first (same discipline as every other FK in this module).
        /// </summary>
        public int? ParentComponentId { get; set; }
        [ForeignKey(nameof(ParentComponentId))]
        public virtual Component? ParentComponent { get; set; }
        public virtual ICollection<Component> ChildComponents { get; set; } = new HashSet<Component>();

        /// <summary>
        /// NEW — which named slot on ParentComponent this sub-assembly currently
        /// occupies (matches a ComponentTypeSubAssemblySlot.SlotCode for the
        /// parent's ComponentType), e.g. "DEEC", "HYD_PUMP_L". Null when
        /// ParentComponentId is null. Plain string, not a FK — the eligibility
        /// rule itself lives in ComponentTypeSubAssemblySlot; this is only the
        /// "which physical slot is filled right now" projection, used by
        /// AttachToParentAsync to enforce each slot's MaxCount capacity by
        /// counting siblings sharing the same (ParentComponentId, CurrentSlotCode).
        /// </summary>
        [StringLength(30)]
        public string? CurrentSlotCode { get; set; }

        public virtual ComponentLifeStatus? ComponentLifeStatus { get; set; }
        public virtual ICollection<ComponentEvent> ComponentEvents { get; set; } = new HashSet<ComponentEvent>();

        /// <summary>NEW (Revision 12) — opening FH/Cycles/Landings/prior-overhaul baseline, only present for a component received with pre-existing usage. See ComponentInitialReading.cs.</summary>
        public virtual ComponentInitialReading? InitialReading { get; set; }
    }
}
