using System;

namespace FRAProject.Models
{
    // Extend IdentityUser via partial class in your project (make sure to inherit IdentityUser elsewhere).
    // Add only small, frequently-read properties here (IDs for scoping); heavy/large objects belong to separate tables.
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        // Profile
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Convenience for UI
        public string DisplayName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? UserName ?? Email ?? ""
            : $"{FirstName} {LastName}";

        // Organization scoping
        public int? BaseId { get; set; }
        public int? WingId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SquadronId { get; set; }
        public int? AcMainGroupId { get; set; }

        // Domain metadata
        public string? JobTitle { get; set; }
        public string? EmployeeNumber { get; set; } // personnel id

        // Preferences
        public string? TimeZone { get; set; }   // e.g. "Europe/Amsterdam"
        public string? Locale { get; set; }     // e.g. "en-US"

        // Operational flags / lifecycle
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }

        // Last login / audit
        public DateTime? LastLoginUtc { get; set; }
    }
}