using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Card Code")]
        public string CardCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(10)]
        [Display(Name = "ATA Chapter")]
        public string? AtaCode { get; set; }

        [Display(Name = "Specialty")]
        public string? Specialty { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La valeur doit être positive.")]
        [Display(Name = "Allocated Time (minutes)")]
        public int AllocatedTimeMinutes { get; set; }

        // ── Added fields ─────────────────────────────────────────────────
        [StringLength(20)]
        [Display(Name = "Work Area(s)")]
        public string? WorkAreas { get; set; }

        [Display(Name = "Mech. No.")]
        public int? MechNo { get; set; }

        [Display(Name = "Electrical Power")]
        public string? ElectricalPowerRequired { get; set; }

        [StringLength(30)]
        [Display(Name = "Figure Ref.")]
        public string? FigureRef { get; set; }

        [StringLength(100)]
        [Display(Name = "T.O. Reference")]
        public string? ToReference { get; set; }

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

        // Extended per real TO XX1F-5E-6WC-3 card data — "APG" added, list
        // may still be incomplete pending more card samples.
        public static readonly List<string> SpecialtyOptions =
            ["MECA", "AVION", "ELEC", "STRUCT", "APG", "OTHER"];

        public static readonly List<string> ElectricalPowerOptions =
            ["ON", "OFF", "NA"];
    }
}