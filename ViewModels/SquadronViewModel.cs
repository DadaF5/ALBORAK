using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

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

        // When true, the controller will remove the stored logo file and clear LogoPath
        [NotMapped]
        [Display(Name = "Remove existing logo")]
        public bool RemoveLogo { get; set; } = false;

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

        // FK to Base (for filtering and display)
        [Display(Name = "Base")]
        public int? BaseId { get; set; }
        public string BaseName { get; set; } = "";

        public bool Active { get; set; } = true;

        // Dropdowns for Wings and Bases
        public List<SelectListItem>? Wings { get; set; }
        public List<SelectListItem>? Bases { get; set; }

        // Computed display
        [Display(Name = "Full Name")]
        public string FullName => $"{Name} ({WingName})";
    }
}