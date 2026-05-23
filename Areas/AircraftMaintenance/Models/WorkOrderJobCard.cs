namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class WorkOrderJobCard
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public int JobCardId { get; set; }
        public JobCard? JobCard { get; set; }

        public int MaintenanceProgramId { get; set; }
        public MaintenanceProgram? MaintenanceProgram { get; set; }

        public int SortOrder { get; set; } = 100;
        public bool IsMandatory { get; set; } = true;

        public string Status { get; set; } = "PENDING"; // PENDING | IN_PROGRESS | DONE | N_A | HOLD
        public string? NAJustification { get; set; }
        public string? Observations { get; set; }

        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<WorkOrderJobCardSignOff> SignOffs { get; set; } = [];
    }
}