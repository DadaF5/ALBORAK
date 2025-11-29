using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.ViewModels
{
    public class SquadronViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Squadron Name")]
        public string Name { get; set; }

        [StringLength(20)]
        [Display(Name = "Call-Sign (BORAK)")]
        public string? CallSign { get; set; }

        [StringLength(100)]
        [Display(Name = "Logo Path")]
        public string? LogoPath { get; set; }

        [NotMapped]
        [Display(Name = "Squadron Logo")]
        public IFormFile? LogoFile { get; set; }

        [StringLength(40)]
        [Display(Name = "Nom de l'Escadron")]
        public string? FrenchName { get; set; }

        [StringLength(10)]
        [Display(Name = "Short Call-Sign (BRK)")]
        public string? CallSignShort { get; set; }

        // FK to Wing
        [Required]
        [Display(Name = "Wing")]
        public int WingId { get; set; }
        public string WingName { get; set; } = "";

        public bool Active { get; set; } = true;

        // Dropdown for Wings
        public List<SelectListItem>? Wings { get; set; }

        // Computed display
        [Display(Name = "Full Name")]
        public string FullName => $"{Name} ({WingName})";
    }
}
