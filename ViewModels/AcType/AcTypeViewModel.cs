using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AcType
{
    public class AcTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du type est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom du Type")]
        public string Name { get; set; }

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le groupe principal est obligatoire.")]
        [Display(Name = "Groupe Principal")]
        public int AcMainGroupId { get; set; }

        [StringLength(30)]
        [Display(Name = "Code")]
        public string? Code { get; set; }

        [Display(Name = "Nb. Moteurs Max")]
        public int MaxEngines { get; set; } = 1;

        [Display(Name = "Passagers Max")]
        public int MaxPassengers { get; set; }

        [Display(Name = "Masse Max (kg)")]
        public double MaxGrossweight { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        // For dropdown list
        public IEnumerable<SelectListItem>? AcMainGroups { get; set; }
    }
}
