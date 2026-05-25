namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string WONumber { get; set; } = string.Empty;

        public int AircraftId { get; set; }
        public string AircraftLabel { get; set; } = string.Empty;

        public int InspectionTypeId { get; set; }
        public string InspectionTypeLabel { get; set; } = string.Empty;

        public string WOType { get; set; } = string.Empty;
        public string WOKind { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int OpenHours { get; set; }
        public int OpenCycles { get; set; }
        public DateOnly OpenDate { get; set; }

        public int? CloseHours { get; set; }
        public int? CloseCycles { get; set; }
        public DateOnly? CloseDate { get; set; }

        public string? OpenedByUserName { get; set; }
        public string? ClosedByUserName { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public List<WorkOrderJobCardItemViewModel> JobCards { get; set; } = [];
    }
}