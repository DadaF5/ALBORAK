using FRAProject.Areas.Settings.Models;
using FRAProject.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        public string WONumber { get; set; } = string.Empty;

        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        // ── Legacy single-InspectionType field ──────────────────────────
        // Now nullable and NOT the source of truth. Kept only so any
        // existing code/data referencing it doesn't break. Real
        // multi-InspectionType tracking is WorkOrderInspectionTypes below.
        // Safe to drop in a future cleanup migration once confirmed unused.
        public int? InspectionTypeId { get; set; }
        public InspectionType? InspectionType { get; set; }

        public string WOType { get; set; } = "F12";   // F11 | F12
        public string WOKind { get; set; } = "PLANNED"; // PLANNED | CORRECTIVE
        public string Status { get; set; } = "DRAFT"; // DRAFT | OPEN | IN_PROGRESS | CLOSED

        public int OpenHours { get; set; }
        public int OpenCycles { get; set; }
        public int OpenLandings { get; set; }

        public DateOnly OpenDate { get; set; }

        public int? CloseHours { get; set; }
        public int? CloseCycles { get; set; }
        public int? CloseLandings { get; set; }         // ← ADD THIS LINE
        public DateOnly? CloseDate { get; set; }

        public string? OpenedByUserId { get; set; }
        public ApplicationUser? OpenedByUser { get; set; }

        public string? ClosedByUserId { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<WorkOrderJobCard> WorkOrderJobCards { get; set; } = [];
        public ICollection<InspectionState> InspectionStatesAsLastWorkOrder { get; set; } = [];
        public ICollection<AircraftJobCardState> AircraftJobCardStatesAsLastWorkOrder { get; set; } = [];

        // ── Real multi-InspectionType tracking ──────────────────────────
        // The set of InspectionTypes this WorkOrder is intended to satisfy.
        // Empty for corrective (Snag-driven) work orders. At WO-close time,
        // loop over this collection and update InspectionState for each
        // one independently (each has its own IntervalFH/Cycles/Days).
        public ICollection<WorkOrderInspectionType> WorkOrderInspectionTypes { get; set; } = [];
    }
}