using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AcType
{
    public class AcTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [StringLength(100)]
        [Display(Name = "Type Name")]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Main Group is required")]
        [Display(Name = "Main Group")]
        public int AcMainGroupId { get; set; }

        public double MaxGrossweight { get; set; }
        public int MaxPassengers { get; set; }
        public int MaxEngines { get; set; } = 1;


        // For dropdown list
        public IEnumerable<SelectListItem>? AcMainGroups { get; set; }
    }
}
