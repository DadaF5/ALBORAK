using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    // Minimal fields for self-registration. Prefer admin approval for scoping (Base/Squadron).
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

        // If you allow user to choose a Base/Unit at registration (not recommended),
        // include optional fields here; prefer admin assignment instead.
    }
}