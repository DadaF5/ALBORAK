using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class ComponentPositionListDto
    {
        public int Id { get; set; }
        public string AcTypeName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AtaLabel { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComponentPositionFormDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le type d'aéronef est requis.")]
        [Display(Name = "Type d'aéronef")]
        public int AcTypeId { get; set; }

        [Required(ErrorMessage = "Le code est requis.")]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(150)]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Chapitre ATA")]
        public int? AtaId { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; }
    }
}
