using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AcMainGroup
{
    public class AcMainGroupViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int AcCategoryId { get; set; }

        [Required(ErrorMessage = "Base is required")]
        public int BaseId { get; set; }

        [Required(ErrorMessage = "Group Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<SelectListItem>? Bases { get; set; }
    }
}
