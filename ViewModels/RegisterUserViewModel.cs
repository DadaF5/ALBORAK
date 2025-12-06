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

        // Optional: Base selection for scoping
        [Display(Name = "Base (optional)")]
        public int? BaseId { get; set; }

        public IEnumerable<SelectListItem> BaseList { get; set; } = new List<SelectListItem>();
    }
}