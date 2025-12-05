using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    public class CallSignViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "CallSign")]
        public string Code { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }

        [Display(Name = "Base")]
        public int? BaseId { get; set; }

        [Display(Name = "Squadron")]
        public int? SquadronId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Select lists populated by controller
        public IEnumerable<SelectListItem> BaseList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SquadronList { get; set; } = new List<SelectListItem>();
    }
}