using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Mission role lookup table.
    /// Defines the operational role / mission type of an aircraft.
    ///
    /// French equivalent: Rôle et mission
    /// Regulatory reference: Form 5a Step 2 — "Rôle et Mission" DDL
    ///
    /// Used as FK in:
    ///   ImmatriculationDossier.MissionRoleId
    ///
    /// Optional FK to AcCategory — allows filtering roles
    /// by aircraft category in the UI (e.g. show only helicopter
    /// roles when category = HELI). NULL means role applies
    /// to all categories.
    ///
    /// Seed data: 11 rows matching Form 5a Step 2 dropdown exactly.
    /// </summary>
    [Table("MissionRole")]
    public class MissionRole
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — e.g. CHASSE, SAR, ISR.
        /// Unique. Used in reports and status references.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Full mission role label — e.g. "Chasse / Interception".
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Category filter (optional FK) ────────────────────────────────
        /// <summary>
        /// Optional FK to AcCategory.
        /// When set, this role is specific to that aircraft category.
        /// When NULL, the role applies across all categories.
        ///
        /// Examples:
        ///   ASSAULT (Hélicoptère d'assaut) → AcCategoryId = HELI.Id
        ///   UAV-ISR (Drone ISR)            → AcCategoryId = UAS.Id
        ///   CHASSE  (Chasse)               → AcCategoryId = AVION.Id
        ///   SAR                            → AcCategoryId = NULL (applies to both AVION and HELI)
        /// </summary>
        public int? AcCategoryId { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "SAR — SAR / CSAR"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        /// <summary>
        /// Optional parent category — NULL means role is cross-category.
        /// </summary>
        public AcCategory? AcCategory { get; set; }

        // Uncomment when ImmatriculationDossier is added to the context.
        // public ICollection<ImmatriculationDossier> Dossiers { get; set; } = [];
    }
}
