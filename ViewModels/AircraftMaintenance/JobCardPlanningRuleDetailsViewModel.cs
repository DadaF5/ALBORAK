namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardPlanningRuleDetailsViewModel
    {
        public int Id { get; set; }
        public int JobCardId { get; set; }
        public string JobCardLabel { get; set; } = string.Empty;

        public string RuleName { get; set; } = string.Empty;
        public string? ConditionText { get; set; }
        public bool IsApplicable { get; set; }

        public int? InitialHours { get; set; }
        public int? InitialCycles { get; set; }
        public int? InitialCalendarValue { get; set; }
        public string? InitialCalendarUnit { get; set; }

        public int? RecurringHours { get; set; }
        public int? RecurringCycles { get; set; }
        public int? RecurringCalendarValue { get; set; }
        public string? RecurringCalendarUnit { get; set; }

        public int? ManufacturerSerialFrom { get; set; }
        public int? ManufacturerSerialTo { get; set; }

        public string? RequiredComplianceCode { get; set; }
        public string? ForbiddenComplianceCode { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}