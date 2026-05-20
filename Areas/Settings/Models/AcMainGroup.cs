
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft main group — second level of the aircraft hierarchy.
    ///   AcCategory → AcMainGroup → AcType → AircraftVersion
    ///
    /// Groups aircraft of the same category at a given base.
    /// e.g. Category=AVION, Base=2ème BAFRA → Group "Chasse 2BAFRA"
    ///
    /// Does NOT inherit LookupBase — has two FKs (AcCategory + Base)
    /// and domain-specific Description limit (50 chars matches DB).
    ///
    /// Added vs original:
    ///   Code      — short unique code (migration required)
    ///   SortOrder — display ordering  (migration required)
    ///   IsActive  — renamed from Active (migration required)
    ///
    /// Table: "AcMainGroups" — plural kept (matches existing DB).
    /// Schema attribute removed — EF uses default schema (dbo).
    /// </summary>
    [Table("AcMainGroups")]
    public class AcMainGroup
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — unique identifier.
        /// e.g. "CHASSE-2B", "TRANS-1B"
        /// Added for platform alignment — migration needed.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Name ─────────────────────────────────────────────────────────
        /// <summary>50 chars — matches existing DB column size.</summary>
        public string Name { get; set; } = string.Empty;

        // ── Description ──────────────────────────────────────────────────
        /// <summary>50 chars — matches existing DB column size.</summary>
        public string? Description { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        /// <summary>
        /// byte (0–255) — consistent with LookupBase convention.
        /// Added for platform alignment — migration needed.
        /// </summary>
        public byte SortOrder { get; set; } = 99;

        // ── Soft delete ──────────────────────────────────────────────────
        /// <summary>
        /// Renamed from "Active" → "IsActive" — platform convention.
        /// Migration needed: EXEC sp_rename 'AcMainGroups.Active', 'IsActive'
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ── FK → AcCategory ──────────────────────────────────────────────
        // Configured in OnModelCreating — no [Required] attribute here.
        public int AcCategoryId { get; set; }

        // ── FK → Base ────────────────────────────────────────────────────
        // Configured in OnModelCreating — no [Required] attribute here.
        public int BaseId { get; set; }

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>Used in dropdown lists: "Code — Name"</summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        public AcCategory? AcCategory { get; set; }
        public Base?       Base       { get; set; }

        // Children — no virtual (EF Core uses explicit Include())
        public ICollection<AcType> AcTypes { get; set; } = [];
        public ICollection<Odv>    Odvs    { get; set; } = [];
    }
}
