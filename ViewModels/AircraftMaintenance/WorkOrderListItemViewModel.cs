namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderListItemViewModel
    {
        public int Id { get; set; }
        public string WONumber { get; set; } = string.Empty;

        public int AircraftId { get; set; }
        public string AircraftLabel { get; set; } = string.Empty;

        // Replaces singular InspectionTypeId/InspectionTypeLabel
        public List<string> InspectionTypeLabels { get; set; } = [];

        public string WOType { get; set; } = string.Empty;
        public string WOKind { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int OpenHours { get; set; }
        public int OpenCycles { get; set; }
        public DateOnly OpenDate { get; set; }

        public int? CloseHours { get; set; }
        public int? CloseCycles { get; set; }
        public DateOnly? CloseDate { get; set; }
    }
}