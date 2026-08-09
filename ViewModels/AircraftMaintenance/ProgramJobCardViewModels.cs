using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class ProgramJobCardManageViewModel
    {
        public int MaintenanceProgramId { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string AcTypeLabel { get; set; } = string.Empty;

        public List<ProgramJobCardItemViewModel> AssignedCards { get; set; } = [];
        public List<JobCardLookupViewModel> AvailableCards { get; set; } = [];

        // Bulk range-assign inputs (redisplayed on validation failure)
        [Display(Name = "Code de début")]
        public string? BulkFromCode { get; set; }

        [Display(Name = "Code de fin")]
        public string? BulkToCode { get; set; }
    }

    public class ProgramJobCardItemViewModel
    {
        public int Id { get; set; } // ProgramJobCard.Id
        public int JobCardId { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public int SortOrder { get; set; }
    }

    public class JobCardLookupViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DisplayLabel => $"{Code} — {Title}";
    }
}