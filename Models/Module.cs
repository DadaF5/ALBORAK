using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Platform module catalog.
    ///
    /// One row per functional area in GMAO FRA.
    /// This table is SEEDED — never edited by end users.
    /// New modules are added via migrations + seed data only.
    ///
    /// Relationship:
    ///   Module → ModuleRole (one module has many roles)
    ///   Module → UserAssignment (many users assigned to this module)
    ///
    /// Table: "Modules"
    ///
    /// Seed data:
    ///   MAINTENANCE   Maintenance Aéronefs
    ///   HR            Ressources Humaines
    ///   HEALTHCARE    Service Médical
    ///   SQUADRONOPS   Opérations Escadron
    ///   SETTINGS      Administration Système  ← admin only
    /// </summary>
    [Table("Modules")]
    public class Module
    {
        // ── PK — string code (not int) ────────────────────────────────────
        /// <summary>
        /// Short code — used as FK in UserAssignment and ModuleRole.
        /// Uppercase convention: "MAINTENANCE", "HR", "HEALTHCARE"
        /// Max 20 chars — enough for any module name.
        /// </summary>
        [Key]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Display name in French — shown in admin UI.
        /// e.g. "Maintenance Aéronefs"
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short description — shown as tooltip in assignment form.
        /// </summary>
        [StringLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// FontAwesome icon class for sidebar display.
        /// e.g. "fas fa-wrench", "fas fa-users"
        /// </summary>
        [StringLength(50)]
        public string? IconClass { get; set; }

        /// <summary>
        /// Inactive modules still have data but cannot receive
        /// new assignments. Existing assignments remain valid.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; } = 99;

        // ── Navigation ────────────────────────────────────────────────────
        public ICollection<ModuleRole>     Roles       { get; set; } = [];
        //public ICollection<UserAssignment> Assignments { get; set; } = [];
    }
}
