using System;
using System.Reflection;

namespace FRAProject.ViewModels
{
    // Simple view model for the Users index listing.
    // Populate this in your UsersController (or AccountController index) with user roles and base name.
    public class UserListViewModel
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Roles { get; set; } = "";      // comma-separated roles
        public string? BaseName { get; set; }        // optional text for user's Base
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
