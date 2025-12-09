using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
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

        // Strongly-typed role selection (checkboxes)
        public List<string> SelectedRoles { get; set; } = new List<string>();

        // Roles available for admin to select (controller will populate this)
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

        // Organization scoping fields (admin can pick defaults for the new user)
        [Display(Name = "Base (optional)")]
        public int? BaseId { get; set; }
        public IEnumerable<SelectListItem> BaseList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Wing (optional)")]
        public int? WingId { get; set; }
        public IEnumerable<SelectListItem> WingList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Department (optional)")]
        public int? DepartmentId { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Squadron (optional)")]
        public int? SquadronId { get; set; }
        public IEnumerable<SelectListItem> SquadronList { get; set; } = new List<SelectListItem>();

        [Display(Name = "Aircraft Main Group (optional)")]
        public int? AcMainGroupId { get; set; }
        public IEnumerable<SelectListItem> AcMainGroupList { get; set; } = new List<SelectListItem>();

        // Admin-editable activation flag
        [Display(Name = "Account active")]
        public bool IsActive { get; set; } = true;
    }
}