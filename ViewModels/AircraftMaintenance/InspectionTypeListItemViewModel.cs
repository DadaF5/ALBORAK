namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeListItemViewModel
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

        // Additive — shown as compact badges in the Index list
        public List<string> ProgramCodes { get; set; } = [];

        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}