using System;

namespace FRAProject.Models
{
    // Extend IdentityUser via partial class in your project (make sure to inherit IdentityUser elsewhere).
    // Add only small, frequently-read properties here (IDs for scoping); heavy/large objects belong to separate tables.
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        // ── Profile ──────────────────────────────────────────────────────
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Military grade — "Adjudant", "Lieutenant", "Capitaine"...
        /// Display only — not used for authorization.
        /// </summary>
        public string? Rank { get; set; }

        /// <summary>
        /// Military matricule / badge number — unique identifier
        /// used in the physical hangar environment.
        /// </summary>
        public string? BadgeNumber { get; set; }

        // Convenience for UI
        public string DisplayName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? UserName ?? Email ?? ""
            : $"{FirstName} {LastName}";

        /// <summary>
        /// Short label including rank — used in sign-off displays.
        /// e.g. "Adj. Mohammed ALAMI"
        /// </summary>
        public string FullLabel => string.IsNullOrWhiteSpace(Rank)
            ? DisplayName
            : $"{Rank} {DisplayName}";

        // ── Organization scoping (existing — unchanged, live in production) ──
        // NOTE: these five fields are read directly by OdvPlanningController,
        // SortiesController, SortieCrewsController, OdvsController, and are set
        // as identity claims by AppClaimsPrincipalFactory. Do NOT remove, rename,
        // or route these through a different model without a planned migration —
        // see USER_MANAGEMENT_MERGE_PLAN.md §6.
        public int? BaseId { get; set; }
        public int? WingId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SquadronId { get; set; }
        public int? AcMainGroupId { get; set; }

        /// <summary>
        /// Home base (port d'affectation) — distinct from BaseId, which is
        /// the current/deployed base. Example: aircraft/personnel homed at
        /// Base 8 but currently deployed to Base 2 → BaseId=2, HomeBaseId=8.
        /// Not yet read anywhere — added for future use, no live dependents.
        /// </summary>
        public int? HomeBaseId { get; set; }

        // ── Domain metadata ─────────────────────────────────────────────
        public string? JobTitle { get; set; }
        public string? EmployeeNumber { get; set; } // personnel id

        // ── Preferences ──────────────────────────────────────────────────
        public string? TimeZone { get; set; }   // e.g. "Europe/Amsterdam"
        public string? Locale { get; set; }     // e.g. "en-US"

        // ── Operational flags / lifecycle ──────────────────────────────
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }

        // ── Last login / audit ───────────────────────────────────────────
        public DateTime? LastLoginUtc { get; set; }

        // ── Navigation ────────────────────────────────────────────────────
        /// <summary>
        /// One-to-one domain profile. Added for UserProfileConfiguration
        /// (Plan §4 Step 3). Nullable — most users won't have one until
        /// UserProfiles rows are created.
        /// </summary>
        public UserProfile? Profile { get; set; }

        // ── NOT YET ADDED ─────────────────────────────────────────────────
        // UserAssignments (ICollection)      → add only as part of the scoping migration (Plan §6)
    }
}