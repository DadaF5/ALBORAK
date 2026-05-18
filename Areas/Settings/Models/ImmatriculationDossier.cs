using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Immatriculation dossier master record.
    /// Owns lifecycle (Status, CurrentStep) and audit fields.
    ///
    /// Step data lives in three 1:1 child models:
    ///   DossierAuthority     → Step 1 (AE + OGMN)
    ///   DossierAircraft      → Step 2 (identification + immat)
    ///   DossierAirworthiness → Step 3 (CdN + foreign registration)
    ///
    /// Step 4 → ImmatriculationDocument (1:many — unchanged)
    /// Step 5 attestation → stays on master (4 fields only)
    ///
    /// Regulatory reference: Guide GUI-DPC-001, Art. 15
    ///
    /// Lifecycle:
    ///   Brouillon → Soumis → En examen → Approuvé / Rejeté
    /// </summary>
    [Table("ImmatriculationDossier")]
    public class ImmatriculationDossier
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Dossier identity ─────────────────────────────────────────────

        /// <summary>
        /// DAM reference — e.g. DAM-IMMAT-2026-0042.
        /// NULL while Brouillon. Generated on Submit.
        /// Unique index in OnModelCreating.
        /// </summary>
        public string? DossierNumber { get; set; }

        /// <summary>
        /// Workflow status — stored as string for SQL readability.
        /// Valid: Brouillon / Soumis / En examen / Approuve / Rejete
        /// Enforced in service layer, not by annotation.
        /// </summary>
        public string Status { get; set; } = "Brouillon";

        /// <summary>
        /// Current wizard step (1–5). Persisted so user can
        /// resume from where they stopped. Drives progress bar.
        /// </summary>
        public int CurrentStep { get; set; } = 1;

        // ── Step 5 — Attestation ─────────────────────────────────────────
        // Stays on master — formal legal signature closing the dossier.

        /// <summary>"Fait à" — city or base where document is signed.</summary>
        public string?   AttestationCity      { get; set; }
        public DateOnly? AttestationDate      { get; set; }

        /// <summary>Name, grade and function of signatory.</summary>
        public string?   SignatoryName        { get; set; }

        /// <summary>
        /// Legal attestation — must be true before submission.
        /// Réf. Art. 15, Guide GUI-DPC-001.
        /// </summary>
        public bool AttestationConfirmed { get; set; } = false;

        // ── Audit ────────────────────────────────────────────────────────
        public DateTime  CreatedAt       { get; set; } = DateTime.UtcNow;
        public string?   CreatedByUserId { get; set; }
        public DateTime? SubmittedAt     { get; set; }
        public DateTime? LastModifiedAt  { get; set; }
        public bool      IsActive        { get; set; } = true;

        // ── Computed ─────────────────────────────────────────────────────
        [NotMapped]
        public bool IsEditable => Status == "Brouillon";

        // ── Navigation properties ────────────────────────────────────────

        // 1:1 — null until that step is first saved
        public DossierAuthority?     Authority     { get; set; }
        public DossierAircraft?      Aircraft      { get; set; }
        public DossierAirworthiness? Airworthiness { get; set; }

        // 1:many — Step 4 documents
        public ICollection<ImmatriculationDocument> Documents { get; set; } = [];
    }
}
