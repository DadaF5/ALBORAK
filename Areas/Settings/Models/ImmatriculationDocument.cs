using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Uploaded supporting document for an ImmatriculationDossier.
    /// One dossier → many documents (one per Art. 15 document type).
    ///
    /// Physical storage:
    ///   D:\2BAFRA\Uploads\Immatriculation\{DossierId}\{FileName}
    ///
    /// Platform rule: no binary in SQL — path only.
    ///
    /// FKs in this model:
    ///   DossierId      → ImmatriculationDossier (Cascade delete)
    ///   DocumentTypeId → ImmatriculationDocType
    /// </summary>
    [Table("ImmatriculationDocument")]
    public class ImmatriculationDocument
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Parent dossier ───────────────────────────────────────────────
        public int DossierId { get; set; }

        // ── Document type ────────────────────────────────────────────────
        // FK → ImmatriculationDocType (DOC01..DOC06)
        public int DocumentTypeId { get; set; }

        // ── File metadata ────────────────────────────────────────────────

        /// <summary>
        /// Full physical path.
        /// e.g. D:\2BAFRA\Uploads\Immatriculation\42\DOC01_propriete.pdf
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>Original file name as uploaded by the user.</summary>
        public string? FileName { get; set; }

        /// <summary>File size in bytes.</summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// MIME type — e.g. "application/pdf", "image/jpeg".
        /// </summary>
        public string? MimeType { get; set; }

        // ── Upload audit ─────────────────────────────────────────────────
        public DateTime? UploadedAt      { get; set; }
        public string?   UploadedByUserId { get; set; }

        // ── Soft delete ──────────────────────────────────────────────────
        // Allows replacing a document without losing upload history.
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────

        /// <summary>File size formatted — e.g. "2.4 Mo"</summary>
        [NotMapped]
        public string FileSizeDisplay =>
            FileSize.HasValue
                ? FileSize.Value switch
                {
                    < 1_024             => $"{FileSize.Value} o",
                    < 1_048_576         => $"{FileSize.Value / 1024.0:F1} Ko",
                    _                   => $"{FileSize.Value / 1_048_576.0:F1} Mo"
                }
                : "—";

        /// <summary>True when a file has been successfully uploaded.</summary>
        [NotMapped]
        public bool HasFile =>
            !string.IsNullOrWhiteSpace(FilePath) && IsActive;

        // ── Navigation properties ────────────────────────────────────────
        public ImmatriculationDossier  Dossier      { get; set; } = null!;
        public ImmatriculationDocType  DocumentType { get; set; } = null!;
    }
}
