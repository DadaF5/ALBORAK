using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class InspectionType : LookupBase
    {
        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }

        // PLANNED for now, keep extensible
        public string Kind { get; set; } = "PLANNED";

        public int? IntervalHours { get; set; }
        public int? IntervalCycles { get; set; }

        public int? CalendarValue { get; set; }
        public string? CalendarUnit { get; set; } // DAY | MONTH | YEAR

        public int? ToleranceHours { get; set; }
        public int? ToleranceCycles { get; set; }

        public int? ToleranceCalendarValue { get; set; }
        public string? ToleranceCalendarUnit { get; set; } // DAY | MONTH | YEAR

        public int? NextInspectionTypeId { get; set; }
        public InspectionType? NextInspectionType { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<InspectionTypeProgram> InspectionTypePrograms { get; set; } = [];
        public ICollection<InspectionState> InspectionStates { get; set; } = [];
        public ICollection<WorkOrder> WorkOrders { get; set; } = [];
    }
}