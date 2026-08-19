using System;

namespace FRAProject.ViewModels
{
    // Used by Index, Details, and Delete views.
    // WingId/WingName and DepartmentId/DepartmentName removed — dead
    // everywhere (see EditUserViewModel.cs), no view shows them anymore.
    public class UserListViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public DateTime? LastLoginUtc { get; set; }

        // Informational only — no code reads this for authorization.
        public int? BaseId { get; set; }
        public string? BaseName { get; set; }

        // Real, but only as a Create-time default for SquadronOps screens —
        // never a source of authorization by itself.
        public int? SquadronId { get; set; }
        public string? SquadronName { get; set; }

        public int? AcMainGroupId { get; set; }
        public string? AcMainGroupName { get; set; }

        public bool IsActive { get; set; } = true;

        // Identity roles assigned to the user. Only "Admin" affects module
        // access — everything else here is informational.
        public string[]? Roles { get; set; }
    }
}
