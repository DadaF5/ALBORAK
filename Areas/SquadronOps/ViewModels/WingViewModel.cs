using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    public class WingViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Wing Short Name")]
        public string Name { get; set; }

        [Required, StringLength(60)]
        [Display(Name = "Wing Long Name")]
        public string WingLong { get; set; }

        // FK to Department
        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";

        // Optional AcMainGroup
        [Display(Name = "Main Group")]
        public int? AcMainGroupId { get; set; }
        public string AcMainGroupName { get; set; } = "";

        // Optional Base
        [Display(Name = "Base")]
        public int? BaseId { get; set; }
        public string BaseName { get; set; } = "";


        public bool Active { get; set; } = true;

        //// Dropdown lists for Create/Edit
        public List<SelectListItem>? Departments { get; set; }
        public List<SelectListItem>? AcMainGroups { get; set; }
        public List<SelectListItem>? Bases { get; set; }

        // Optional: List of Squadrons under this Wing
        public ICollection<SquadronViewModel>? Squadrons { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{Name} ({DepartmentName})";
    }
}
