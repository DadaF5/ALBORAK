using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    // See EditUserViewModel.cs for the full explanation of what was removed
    // and why. Same trim applied here for consistency between Create and Edit.
    public class RegisterUserViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Administrator (full access, bypasses all module scoping)")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Base (informational only)")]
        public int? BaseId { get; set; }
        public IEnumerable<SelectListItem> BaseList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Squadron (SquadronOps default only)")]
        public int? SquadronId { get; set; }
        public IEnumerable<SelectListItem> SquadronList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Aircraft Main Group (SquadronOps default only)")]
        public int? AcMainGroupId { get; set; }
        public IEnumerable<SelectListItem> AcMainGroupList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Account active")]
        public bool IsActive { get; set; } = true;
    }
}
