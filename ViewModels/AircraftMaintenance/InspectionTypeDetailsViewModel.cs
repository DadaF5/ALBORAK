namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeDetailsViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;

        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;

        public int? IntervalHours { get; set; }
        public int? IntervalCycles { get; set; }
        public int? CalendarValue { get; set; }
        public string? CalendarUnit { get; set; }

        public int? ToleranceHours { get; set; }
        public int? ToleranceCycles { get; set; }
        public int? ToleranceCalendarValue { get; set; }
        public string? ToleranceCalendarUnit { get; set; }

        public int? NextInspectionTypeId { get; set; }
        public string? NextInspectionTypeLabel { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public List<MaintenanceProgramListItemViewModel> Programs { get; set; } = [];
    }
}