using FRAProject.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class WorkOrderJobCardSignOff
    {
        public int Id { get; set; }

        public int WorkOrderJobCardId { get; set; }
        public WorkOrderJobCard? WorkOrderJobCard { get; set; }

        public string Level { get; set; } = string.Empty; // TECHNICIAN | APRS | NAVIGABILITY | COMMANDER
        public bool IsMandatory { get; set; } = true;

        public string? SignedByUserId { get; set; }
        public ApplicationUser? SignedByUser { get; set; }

        public DateTime? SignedAtUtc { get; set; }
        public bool? Accepted { get; set; }
        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}