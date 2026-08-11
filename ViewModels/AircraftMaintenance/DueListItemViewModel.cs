namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class DueListItemViewModel
    {
        public int AircraftId { get; set; }
        public string AircraftLabel { get; set; } = string.Empty;
        public string AcTypeLabel { get; set; } = string.Empty;

        public int InspectionTypeId { get; set; }
        public string InspectionTypeCode { get; set; } = string.Empty;
        public string InspectionTypeName { get; set; } = string.Empty;

        public int CurrentHours { get; set; }
        public int CurrentCycles { get; set; }

        public int? LastDoneHours { get; set; }
        public DateOnly? LastDoneDate { get; set; }

        public int? NextDueHours { get; set; }
        public int? NextDueCycles { get; set; }
        public DateOnly? NextDueDate { get; set; }

        // Negative = overdue by this many hours. Null if no NextDueHours
        // set yet (e.g. UNKNOWN — never done, no baseline to compute from).
        public int? RemainingHours { get; set; }

        public string Status { get; set; } = "UNKNOWN"; // OVERDUE | ALERT | OK | UNKNOWN

        // Lower = more urgent, used for default sort order
        public int StatusSeverity => Status switch
        {
            "OVERDUE" => 0,
            "ALERT" => 1,
            "UNKNOWN" => 2,
            "OK" => 3,
            _ => 4
        };
    }
}