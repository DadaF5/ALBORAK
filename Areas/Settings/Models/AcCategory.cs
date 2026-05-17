using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Aircraft category lookup table.
    /// Top level of the aircraft classification hierarchy:
    ///   AcCategory → AcMainGroup → AcType → AircraftVersion
    ///
    /// Moved from FRAProject.Areas.AircraftMaintenance.Models to
    /// FRAProject.Models — this is a cross-cutting lookup used by
    /// both AircraftMaintenance and ImmatriculationDossier (Form 5a).
    ///
    /// DB note: if your existing migration used [Column("AcCategoryId")]
    /// keep the attribute below until you run a rename migration.
    /// Once renamed in DB, remove the attribute entirely.
    ///
    /// Used as FK in:
    ///   AcMainGroup.AcCategoryId
    ///   ImmatriculationDossier.AircraftCategoryId  (Form 5a Step 2)
    ///   MissionRole.CategoryId                     (optional filter)
    ///
    /// Seed data:
    ///   AVION — Avion (voilure fixe)
    ///   HELI  — Hélicoptère (voilure tournante)
    ///   UAS   — UAS / Drone (sans équipage)
    /// </summary>
    [Table("AcCategory")]       // singular — platform convention
    public class AcCategory
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // Keep [Column] attribute only if your DB column is still "AcCategoryId".
        // Remove once you run a rename migration → Id.
        [Column("AcCategoryId")]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — e.g. AVION, HELI, UAS.
        /// Unique. Matches the radio card values in Form 5a Step 2.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Display name — e.g. "Avion", "Hélicoptère", "UAS / Drone".
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Optional description ─────────────────────────────────────────
        /// <summary>
        /// Additional detail — e.g. "Aéronef à voilure fixe".
        /// Optional — not required.
        /// </summary>
        public string? Description { get; set; }

        // ── Icon key ─────────────────────────────────────────────────────
        /// <summary>
        /// Emoji or icon identifier used in radio card UI (Form 5a).
        /// e.g. "✈" for Avion, "🚁" for Hélicoptère, "◈" for UAS.
        /// Optional — display only, not used in business logic.
        /// </summary>
        public string? IconKey { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "AVION — Avion"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        public ICollection<AcMainGroup> AcMainGroups { get; set; } = [];

        // Uncomment when ImmatriculationDossier is added:
        // public ICollection<ImmatriculationDossier> Dossiers { get; set; } = [];

        // Uncomment when MissionRole is added:
        // public ICollection<MissionRole> MissionRoles { get; set; } = [];
    }
}
