namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class JobCardPlanningRule
    {
        public int Id { get; set; }

        public int JobCardId { get; set; }
        public JobCard? JobCard { get; set; }

        public string RuleName { get; set; } = string.Empty;
        public string? ConditionText { get; set; }

        public bool IsApplicable { get; set; } = true;

        // Initial thresholds
        public int? InitialHours { get; set; }
        public int? InitialCycles { get; set; }
        public int? InitialCalendarValue { get; set; }
        public string? InitialCalendarUnit { get; set; } // DAY | MONTH | YEAR

        // Recurrence thresholds
        public int? RecurringHours { get; set; }
        public int? RecurringCycles { get; set; }
        public int? RecurringCalendarValue { get; set; }
        public string? RecurringCalendarUnit { get; set; } // DAY | MONTH | YEAR

        // Aircraft manufacturer serial range (numeric part only)
        public int? ManufacturerSerialFrom { get; set; }
        public int? ManufacturerSerialTo { get; set; }

        // Placeholder for future SB/compliance logic
        public string? RequiredComplianceCode { get; set; }
        public string? ForbiddenComplianceCode { get; set; }

        public int SortOrder { get; set; } = 100;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<AircraftJobCardState> AircraftJobCardStates { get; set; } = [];
    }
}