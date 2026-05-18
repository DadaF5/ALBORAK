using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Immatriculation document type lookup table.
    /// Defines the required supporting documents for a DAM
    /// immatriculation dossier (Form 5a — Step 4).
    ///
    /// Regulatory reference: Art. 15, Points 1–6, GUI-DPC-001
    ///
    /// Used as FK in:
    ///   ImmatriculationDocument.DocumentTypeId
    ///
    /// Seed data: 6 fixed rows — defined by DAM regulation.
    /// These rows must NEVER be hard-deleted — use IsActive only.
    ///
    ///   DOC01 — Justificatif de propriété              [Obligatoire]
    ///   DOC02 — Photo plaque signalétique              [Obligatoire]
    ///   DOC03 — Certificat de radiation étranger       [Si applicable]
    ///   DOC04 — Copie contrat d'assurance              [Obligatoire]
    ///   DOC05 — Certificat de navigabilité / AdV       [Si disponible]
    ///   DOC06 — Documents de dédouanement              [Si applicable]
    /// </summary>
    [Table("ImmatriculationDocType")]
    public class ImmatriculationDocType
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Short code ───────────────────────────────────────────────────
        /// <summary>
        /// Short code — DOC01 through DOC06.
        /// Unique. Matches the Art. 15.x numbering from GUI-DPC-001.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ── Full name ────────────────────────────────────────────────────
        /// <summary>
        /// Full document name as it appears in Form 5a Step 4.
        /// Unique index configured in OnModelCreating.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // ── Regulatory reference ─────────────────────────────────────────
        /// <summary>
        /// Article reference from GUI-DPC-001.
        /// e.g. "Art. 15.1", "Art. 15.4"
        /// Optional — displayed as a label in the upload zone.
        /// </summary>
        public string? ArticleReference { get; set; }

        // ── Required flag ────────────────────────────────────────────────
        /// <summary>
        /// true  = Obligatoire (must be uploaded before dossier submission)
        /// false = Si applicable / Si disponible (conditional upload)
        /// Controls validation logic in ImmatriculationDossier.
        /// </summary>
        public bool IsRequired { get; set; } = false;

        // ── Accepted formats ─────────────────────────────────────────────
        /// <summary>
        /// Comma-separated list of accepted MIME types or extensions.
        /// e.g. "PDF" / "JPG,PNG" / "PDF,JPG,PNG"
        /// Used by the upload zone to hint accepted file types.
        /// Optional — no restriction if null.
        /// </summary>
        public string? AcceptedFormats { get; set; }

        // ── Max file size ────────────────────────────────────────────────
        /// <summary>
        /// Maximum accepted file size in megabytes.
        /// Used by the upload zone as a UI hint and validation rule.
        /// Optional — no restriction if null.
        /// </summary>
        public int? MaxFileSizeMb { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        /// <summary>
        /// IMPORTANT: These rows are legally mandated.
        /// Never hard-delete — only deactivate via IsActive = false.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "DOC01 — Justificatif de propriété"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        /// <summary>
        /// Human-readable requirement label for UI display.
        /// </summary>
        [NotMapped]
        public string RequirementLabel => IsRequired
            ? "Obligatoire"
            : "Si applicable";

        // ── Navigation properties ────────────────────────────────────────
        // Uncomment when ImmatriculationDocument is added to the context.
        // public ICollection<ImmatriculationDocument> Documents { get; set; } = [];
    }
}
