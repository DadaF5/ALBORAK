using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class MaintenanceProgramFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Program Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Program Name")]
        public string Name { get; set; } = string.Empty;

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
    }
}