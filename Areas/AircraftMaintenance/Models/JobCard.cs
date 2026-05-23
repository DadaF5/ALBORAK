using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class JobCard
    {
        public int Id { get; set; }

        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }

        public string? AtaCode { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? Specialty { get; set; } // MECA | AVION | ELEC | STRUCT | OTHER
        public int AllocatedTimeMinutes { get; set; }

        public string? ToReference { get; set; }
        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public int SortOrder { get; set; } = 100;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public string DisplayLabel => $"{CardCode} — {Title}";

        public ICollection<ProgramJobCard> ProgramJobCards { get; set; } = [];
        public ICollection<JobCardPlanningRule> PlanningRules { get; set; } = [];
        public ICollection<JobCardAttachment> Attachments { get; set; } = [];
        public ICollection<AircraftJobCardState> AircraftJobCardStates { get; set; } = [];
        public ICollection<WorkOrderJobCard> WorkOrderJobCards { get; set; } = [];
    }
}