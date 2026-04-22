using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class AcTypeCreateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Le code doit contenir entre 2 et 30 caractères")]
        [Display(Name = "Code Type")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères")]
        [Display(Name = "Nom du Type")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire")]
        [StringLength(250, ErrorMessage = "La description ne peut pas dépasser 250 caractères")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le groupe principal est obligatoire")]
        [Display(Name = "Groupe Principal (Aircraft Family)")]
        public int AcMainGroupId { get; set; }

        [Display(Name = "Constructeur")]
        public int? AircraftManufacturerId { get; set; }

        [Required(ErrorMessage = "Le poids maximum est obligatoire")]
        [Range(0.1, 999999, ErrorMessage = "Le poids doit être supérieur à 0")]
        [Display(Name = "Poids Maximum (kg)")]
        public double MaxGrossweight { get; set; }

        [Required(ErrorMessage = "Le nombre de passagers est obligatoire")]
        [Range(0, 999, ErrorMessage = "Le nombre de passagers doit être entre 0 et 999")]
        [Display(Name = "Passagers Maximum")]
        public int MaxPassengers { get; set; }

        [Required(ErrorMessage = "Le nombre de sièges est obligatoire")]
        [Range(1, 999, ErrorMessage = "Le nombre de sièges doit être entre 1 et 999")]
        [Display(Name = "Nombre de Sièges")]
        public int SeatCount { get; set; }

        [Required(ErrorMessage = "Le nombre de moteurs est obligatoire")]
        [Range(1, 8, ErrorMessage = "Le nombre de moteurs doit être entre 1 et 8")]
        [Display(Name = "Nombre de Moteurs")]
        public int MaxEngines { get; set; }

        [Display(Name = "Type Actif")]
        public bool IsActive { get; set; } = true;

        [Range(0, 255, ErrorMessage = "L'ordre de tri doit être entre 0 et 255")]
        [Display(Name = "Ordre de Tri")]
        public byte SortOrder { get; set; } = 99;
    }
}
