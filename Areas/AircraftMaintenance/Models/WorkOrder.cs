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

        public int InspectionTypeId { get; set; }
        public InspectionType? InspectionType { get; set; }

        public string WOType { get; set; } = "F12";   // F11 | F12
        public string WOKind { get; set; } = "PLANNED";
        public string Status { get; set; } = "DRAFT"; // DRAFT | OPEN | IN_PROGRESS | CLOSED

        public int OpenHours { get; set; }
        public int OpenCycles { get; set; }
        public DateOnly OpenDate { get; set; }

        public int? CloseHours { get; set; }
        public int? CloseCycles { get; set; }
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
    }
}