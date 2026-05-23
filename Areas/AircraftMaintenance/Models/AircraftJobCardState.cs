using FRAProject.Areas.Settings.Models;
using Microsoft.Data.SqlClient;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class AircraftJobCardState
    {
        public int Id { get; set; }

        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        public int JobCardId { get; set; }
        public JobCard? JobCard { get; set; }

        public int? AppliedPlanningRuleId { get; set; }
        public JobCardPlanningRule? AppliedPlanningRule { get; set; }

        public int? LastExecutedHours { get; set; }
        public int? LastExecutedCycles { get; set; }
        public DateOnly? LastExecutedDate { get; set; }

        public int? NextDueHoursBase { get; set; }
        public int? NextDueHoursExtended { get; set; }

        public int? NextDueCyclesBase { get; set; }
        public int? NextDueCyclesExtended { get; set; }

        public DateOnly? NextDueDateBase { get; set; }
        public DateOnly? NextDueDateExtended { get; set; }

        public int? RemainingHoursBase { get; set; }
        public int? RemainingHoursExtended { get; set; }

        public int? RemainingCyclesBase { get; set; }
        public int? RemainingCyclesExtended { get; set; }

        public int? RemainingDaysBase { get; set; }
        public int? RemainingDaysExtended { get; set; }

        public int? LastWorkOrderId { get; set; }
        public WorkOrder? LastWorkOrder { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}