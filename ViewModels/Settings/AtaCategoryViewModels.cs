using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.Settings
{
    public class AtaCategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }

    public class AtaCategoryListItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}