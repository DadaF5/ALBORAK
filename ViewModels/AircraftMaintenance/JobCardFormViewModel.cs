using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [StringLength(20)]
        [Display(Name = "ATA")]
        public string? AtaCode { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Card Code")]
        public string CardCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(20)]
        [Display(Name = "Specialty")]
        public string? Specialty { get; set; }

        [Display(Name = "Allocated Time (minutes)")]
        public int AllocatedTimeMinutes { get; set; }

        [StringLength(100)]
        [Display(Name = "TO Reference")]
        public string? ToReference { get; set; }

        [StringLength(100)]
        [Display(Name = "Document Reference")]
        public string? DocReference { get; set; }

        [StringLength(30)]
        [Display(Name = "Edition")]
        public string? Edition { get; set; }

        [Display(Name = "Change No")]
        public int? ChangeNo { get; set; }

        [Display(Name = "Change Date")]
        public DateOnly? ChangeDate { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<AcTypeLookupViewModel> AcTypes { get; set; } = [];
        public List<string> SpecialtyOptions { get; set; } = ["MECA", "AVION", "ELEC", "STRUCT", "OTHER"];
    }
}