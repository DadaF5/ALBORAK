using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Functional role within a platform module.
    ///
    /// This table is SEEDED — never edited by end users.
    /// One Module → Many ModuleRoles.
    ///
    /// Table: "ModuleRoles"
    ///
    /// Seed data (MAINTENANCE module):
    ///   TECHNICIAN         Technicien              CanWrite=true  SignOff=TECHNICIAN
    ///   APRS               Inspecteur APRS         CanWrite=true  SignOff=APRS
    ///   NAVIGABILITY_OFFICER Officier Navigabilité CanWrite=true  SignOff=NAVIGABILITY
    ///   COMMANDER          Commandant              CanWrite=true  SignOff=COMMANDER
    ///   BASE_SUPERVISOR    Superviseur de Base     CanWrite=false SignOff=null
    ///   MASTER_SUPERVISOR  Superviseur Central     CanWrite=false SignOff=null
    ///
    /// Key design decisions:
    ///   ShowBaseScope  = false means the Base DDL is hidden in the
    ///                    assignment form (MASTER_SUPERVISOR sees all).
    ///   ShowGroupScope = false means the AcMainGroup DDL is hidden
    ///                    (HR, Medical, BASE_SUPERVISOR see all groups).
    ///   SignOffLevel   drives the WOJobCardSignOff.Level check —
    ///                    only the matching role can sign that level.
    /// </summary>
    [Table("ModuleRoles")]
    public class ModuleRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── FK → Module ───────────────────────────────────────────────────
        [Required]
        [StringLength(20)]
        public string ModuleCode { get; set; } = string.Empty;

        // ── Role identity ─────────────────────────────────────────────────
        /// <summary>
        /// Short code — used in authorization checks.
        /// e.g. "TECHNICIAN", "APRS", "BASE_SUPERVISOR"
        /// Unique within a module.
        /// </summary>
        [Required]
        [StringLength(30)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Display name in French — shown in assignment DDL.
        /// e.g. "Technicien de maintenance"
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // ── Permission flags ──────────────────────────────────────────────
        /// <summary>
        /// false = read-only role.
        /// BASE_SUPERVISOR and MASTER_SUPERVISOR are read-only.
        /// All others can write (create/edit/sign).
        /// </summary>
        public bool CanWrite { get; set; } = true;

        // ── Sign-off level ────────────────────────────────────────────────
        /// <summary>
        /// Which WOJobCardSignOff.Level this role can sign.
        /// "TECHNICIAN" | "APRS" | "NAVIGABILITY" | "COMMANDER"
        /// null for supervisor roles (they don't sign).
        /// </summary>
        [StringLength(20)]
        public string? SignOffLevel { get; set; }

        // ── Scope UI hints ────────────────────────────────────────────────
        /// <summary>
        /// false = hide the Base DDL in the assignment form.
        /// Used for MASTER_SUPERVISOR — they see all bases.
        /// </summary>
        public bool ShowBaseScope { get; set; } = true;

        /// <summary>
        /// false = hide the AcMainGroup DDL in the assignment form.
        /// Used for HR, Healthcare, BASE_SUPERVISOR roles.
        /// </summary>
        public bool ShowGroupScope { get; set; } = true;

        /// <summary>
        /// true = show the Wing DDL in the assignment form.
        /// Used for SquadronOps roles where Wing matters
        /// (Pilot, Instructor, Scheduler).
        /// false for all other modules — Wing is irrelevant.
        /// </summary>
        public bool ShowWingScope { get; set; } = false;

        // ── Display ───────────────────────────────────────────────────────
        public bool IsActive  { get; set; } = true;
        public byte SortOrder { get; set; } = 99;

        // ── Navigation ────────────────────────────────────────────────────
        public Module?                     Module      { get; set; }
        //public ICollection<UserAssignment> Assignments { get; set; } = [];
    }
}
