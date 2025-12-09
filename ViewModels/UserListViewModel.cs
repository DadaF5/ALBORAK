using System;
using System.Reflection;

namespace FRAProject.ViewModels
{
    // Simple view model for the Users index listing.
    // Populate this in your UsersController (or AccountController index) with user roles and base name.
    public class UserListViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAtUtc { get; set; }

        // NEW: last login timestamp (added per your request)
        public DateTime? LastLoginUtc { get; set; }

        // IDs (optional) and friendly names to avoid lookups in the view
        public int? BaseId { get; set; }
        public string? BaseName { get; set; }

        public int? WingId { get; set; }
        public string? WingName { get; set; }

        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public int? SquadronId { get; set; }
        public string? SquadronName { get; set; }

        public int? AcMainGroupId { get; set; }
        public string? AcMainGroupName { get; set; }

        public bool IsActive { get; set; } = true;

        // Roles assigned to the user
        public string[]? Roles { get; set; }
    }
}
