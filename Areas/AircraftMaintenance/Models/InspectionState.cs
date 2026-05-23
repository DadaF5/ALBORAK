using FRAProject.Areas.Settings.Models;
using Microsoft.Data.SqlClient;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class InspectionState
    {
        public int Id { get; set; }

        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        public int InspectionTypeId { get; set; }
        public InspectionType? InspectionType { get; set; }

        public int? LastDoneHours { get; set; }
        public int? LastDoneCycles { get; set; }
        public DateOnly? LastDoneDate { get; set; }

        public int? LastWorkOrderId { get; set; }
        public WorkOrder? LastWorkOrder { get; set; }

        public int? NextDueHours { get; set; }
        public int? NextDueCycles { get; set; }
        public DateOnly? NextDueDate { get; set; }

        public string? StatusSnapshot { get; set; } // OVERDUE | ALERT | OK | UNKNOWN

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}