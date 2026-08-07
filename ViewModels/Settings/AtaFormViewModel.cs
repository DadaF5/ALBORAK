using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.Settings
{
    public class AtaFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Code ATA")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Titre")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Catégorie")]
        public int? AtaCategoryId { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        public List<AtaCategoryLookupViewModel> Categories { get; set; } = [];
    }
}