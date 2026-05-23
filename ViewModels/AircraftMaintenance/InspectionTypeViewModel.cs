using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Aircraft Type is required")]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [Display(Name = "Next Inspection Type")]
        public int? NextInspectionTypeId { get; set; }

        [Display(Name = "Sort Order")]
        [Range(0, 255)]
        public byte SortOrder { get; set; } = 99;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Dropdowns
        public IEnumerable<SelectListItem>? AcTypes { get; set; }
        public IEnumerable<SelectListItem>? NextInspectionTypes { get; set; }
    }
}
