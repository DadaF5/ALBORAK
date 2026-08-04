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
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Document Reference")]
        public string? DocReference { get; set; }

        [StringLength(20)]
        [Display(Name = "Edition")]
        public string? Edition { get; set; }

        [Display(Name = "Change Number")]
        public int? ChangeNo { get; set; }

        [Display(Name = "Change Date")]
        [DataType(DataType.Date)]
        public DateOnly? ChangeDate { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<AcTypeLookupViewModel> AcTypes { get; set; } = [];
    }
}