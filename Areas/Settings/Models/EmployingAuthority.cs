using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Employing Authority (Autorité d'Emploi — AE) lookup table.
    /// Represents the military organisation responsible for an aircraft.
    ///
    /// Used as FK in:
    ///   ImmatriculationDossier.EmployingAuthorityId
    ///
    /// Seed data (5 rows — fixed by regulation):
    ///   FRA — Forces Royales Air
    ///   MR  — Marine Royale
    ///   GR  — Gendarmerie Royale
    ///   FT  — Forces Terrestres
    ///   AUT — Autre
    /// </summary>
    [Table("EmployingAuthority")]
    public class EmployingAuthority
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — e.g. FRA, MR, GR, FT, AUT.
        /// Unique. Max 10 chars.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Full official name — e.g. "Forces Royales Air".
        /// Unique. Displayed in dropdowns and form labels.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Display ordering ─────────────────────────────────────────────
        /// <summary>
        /// Controls position in dropdown lists.
        /// FRA = 1 (always first — primary user of the platform).
        /// </summary>
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "FRA — Forces Royales Air"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        // Uncomment when ImmatriculationDossier is added to the context.
        // public ICollection<ImmatriculationDossier> Dossiers { get; set; } = [];
    }
}
