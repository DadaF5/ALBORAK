using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Airworthiness document type lookup table.
    /// Defines the type of document proving aircraft airworthiness.
    ///
    /// French equivalent: Type de document de navigabilité
    /// Regulatory reference: Form 5a — Step 3 (GUI-DPC-001 Art. 9–10)
    ///
    /// Used as FK in:
    ///   ImmatriculationDossier.CdnDocTypeId
    ///
    /// Seed data (3 rows — fixed by DAM regulation):
    ///   CDN  — Certificat de navigabilité
    ///   ADV  — Autorisation de vol
    ///   AUT  — Autre
    /// </summary>
    [Table("CdnDocType")]
    public class CdnDocType
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short uppercase code — CDN, ADV, AUT.
        /// Unique. Used in status badges and document references.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Full document type label.
        /// e.g. "Certificat de navigabilité", "Autorisation de vol".
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Optional description ─────────────────────────────────────────
        /// <summary>
        /// Additional context about when this document type applies.
        /// Optional.
        /// </summary>
        public string? Description { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "CDN — Certificat de navigabilité"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        // Uncomment when ImmatriculationDossier is added to the context.
        // public ICollection<ImmatriculationDossier> Dossiers { get; set; } = [];
    }
}
