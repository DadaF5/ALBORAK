using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class BaseCreateDto
    {
        public int Id { get; set; }
 
        [Required(ErrorMessage = "Le code base est obligatoire")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Le code doit contenir entre 2 et 10 caractères")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Le code doit contenir uniquement des lettres majuscules et des chiffres")]
        [Display(Name = "Code Base")]
        public string BaseCode { get; set; } = string.Empty;
 
        [Required(ErrorMessage = "Le nom de la base est obligatoire")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Le nom doit contenir entre 3 et 100 caractères")]
        [Display(Name = "Nom de la Base")]
        public string BaseName { get; set; } = string.Empty;
 
        [Required(ErrorMessage = "La localisation est obligatoire")]
        [StringLength(100, ErrorMessage = "La localisation ne peut pas dépasser 100 caractères")]
        [Display(Name = "Localisation")]
        public string Location { get; set; } = string.Empty;
 
        [Display(Name = "Base Active")]
        public bool IsActive { get; set; } = true;
 
        [Range(-90, 90, ErrorMessage = "La latitude doit être entre -90 et 90")]
        [Display(Name = "Latitude")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        public decimal? Latitude { get; set; }
 
        [Range(-180, 180, ErrorMessage = "La longitude doit être entre -180 et 180")]
        [Display(Name = "Longitude")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        public decimal? Longitude { get; set; }
    }
}