using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class AircraftVersionCreateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Le code doit contenir entre 2 et 30 caractères")]
        [Display(Name = "Code Version")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 150 caractères")]
        [Display(Name = "Nom Version / Block")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La description ne peut pas dépasser 250 caractères")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le type d'aéronef est obligatoire")]
        [Display(Name = "Type d'Aéronef")]
        public int AcTypeId { get; set; }

        [Display(Name = "Version Active")]
        public bool IsActive { get; set; } = true;

        [Range(0, 255, ErrorMessage = "L'ordre de tri doit être entre 0 et 255")]
        [Display(Name = "Ordre de Tri")]
        public byte SortOrder { get; set; } = 99;
    }
}
