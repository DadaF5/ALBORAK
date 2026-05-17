using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.DTOs
{
    public class CountryFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code ISO est obligatoire.")]
        [StringLength(2, MinimumLength = 2,
            ErrorMessage = "Le code ISO doit contenir exactement 2 caractères.")]
        [Display(Name = "Code ISO")]
        public string IsoCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du pays est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Nom du pays")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Continent")]
        public string? Continent { get; set; }

        [Display(Name = "Ordre d'affichage")]
        [Range(0, 9999)]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }
}
