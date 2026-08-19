using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    // Trimmed to fields that actually do something, after live testing
    // showed this form's "Organization Structure" + full role checklist
    // looked authoritative but weren't — see UsersController comments.
    //
    // Removed entirely (dead everywhere, never read by any policy or
    // scoping logic): DepartmentId, WingId, and their option lists.
    //
    // Kept, but re-labeled as what they actually are: BaseId (informational
    // only — no code reads it), SquadronId/AcMainGroupId (real, but only
    // as a Create-time default for SquadronOps' Mission/Odv screens, always
    // re-validated against the user's actual UserAssignment scope before
    // being trusted — never a source of authorization by themselves).
    //
    // The 7-checkbox Roles list is now a single IsAdmin toggle — "Admin" was
    // the only one of those seven that ModuleAccessHandler actually checks.
    // Real module access (Maintenance, SquadronOps, etc.) is granted
    // separately via UserAssignment — see the banner on this Edit screen.
    public class EditUserViewModel
    {
        public string? Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Base (informational only)")]
        public int? BaseId { get; set; }

        [Display(Name = "Squadron (SquadronOps default only)")]
        public int? SquadronId { get; set; }

        [Display(Name = "Aircraft Main Group (SquadronOps default only)")]
        public int? AcMainGroupId { get; set; }

        public bool IsActive { get; set; }

        [Display(Name = "Administrator (full access, bypasses all module scoping)")]
        public bool IsAdmin { get; set; }

        // Lists for selects (populated by controller)
        public List<SelectListItem>? BaseList { get; set; }
        public List<SelectListItem>? SquadronList { get; set; }
        public List<SelectListItem>? AcMainGroupList { get; set; }
    }
}
