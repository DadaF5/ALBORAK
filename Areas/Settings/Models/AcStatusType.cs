
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft status type lookup table.
    /// Defines the possible operational states of an aircraft.
    ///
    /// Used as FK in:
    ///   Aircraft.AcStatusTypeId
    ///
    /// Seed data examples:
    ///   OPR — Opérationnel
    ///   MNT — En maintenance
    ///   AOG — Aircraft on Ground
    ///   STK — En stockage
    ///   RAD — Radié
    /// </summary>
    [Table("AcStatusType")]     // singular — platform convention
    public class AcStatusType
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — e.g. OPR, MNT, AOG.
        /// Unique. Used in reports and status badges.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Full status label — e.g. "Opérationnel", "En maintenance".
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Optional description ─────────────────────────────────────────
        /// <summary>
        /// Additional detail about when this status applies.
        /// Optional — not required for seed data.
        /// </summary>
        public string? Description { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        /// <summary>
        /// Controls position in dropdown lists.
        /// int — consistent with all other platform lookup tables.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "OPR — Opérationnel"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        public ICollection<Aircraft> Aircrafts { get; set; } = [];
    }
}
