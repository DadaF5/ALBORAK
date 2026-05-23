using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class MaintenanceProgram : LookupBase
    {
        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }

        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<InspectionTypeProgram> InspectionTypePrograms { get; set; } = [];
        public ICollection<ProgramJobCard> ProgramJobCards { get; set; } = [];
        public ICollection<WorkOrderJobCard> WorkOrderJobCards { get; set; } = [];
    }
}