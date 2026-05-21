using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Airworthiness certificate attached to an aircraft.
    /// One aircraft → many certificates (one per type, potentially multiple
    /// historical rows per type — only the most recent IsActive=true matters).
    ///
    /// Supported certificate types (CertType):
    ///   CdN  — Certificat de Navigabilité
    ///   CEN  — Compte Rendu d'Examen de Navigabilité
    ///   PEA  — Programme d'Entretien Agréé
    ///   LME  — Liste des Modifications et Équipements
    ///   CDL  — Configuration Deviation List
    ///
    /// Table: "AircraftCertificates" — plural, platform convention.
    ///
    /// FK to Aircraft — soft delete only (IsActive = false).
    /// Physical document stored at:
    ///   D:\2BAFRA\Uploads\Certificates\{AircraftId}\{CertType}_{Reference}.pdf
    /// </summary>
    [Table("AircraftCertificates")]
    public class AircraftCertificate
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── FK → Aircraft ────────────────────────────────────────────────
        // Configured in Fluent API — no attribute here.
        public int AircraftId { get; set; }

        // ── Certificate type ─────────────────────────────────────────────
        /// <summary>
        /// Certificate type code.
        /// Valid values: "CdN" | "CEN" | "PEA" | "LME" | "CDL"
        /// Enforced in controller — not by DB constraint.
        /// Unique index: (AircraftId, CertType) — one active cert per type.
        /// </summary>
        public string CertType { get; set; } = string.Empty;

        // ── Certificate identity ─────────────────────────────────────────

        /// <summary>
        /// Official certificate reference number.
        /// e.g. "CdN-FRA-2024-00042", "CEN/F5E/2024-03"
        /// </summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// Authority that issued the certificate.
        /// e.g. "DAM", "DGAC", "FAA", "EASA"
        /// </summary>
        public string? IssuingAuthority { get; set; }

        public DateOnly? IssueDate  { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        // ── Document ─────────────────────────────────────────────────────
        /// <summary>
        /// Full physical path to the scanned certificate.
        /// D:\2BAFRA\Uploads\Certificates\{AircraftId}\{CertType}_{Reference}.pdf
        /// Platform rule: path only — no binary in SQL.
        /// </summary>
        public string? DocumentPath { get; set; }

        /// <summary>Original file name as uploaded.</summary>
        public string? DocumentName { get; set; }

        // ── Notes ────────────────────────────────────────────────────────
        public string? Notes { get; set; }

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Audit ────────────────────────────────────────────────────────
        public DateTime  CreatedAt       { get; set; } = DateTime.UtcNow;
        public string?   CreatedByUserId { get; set; }
        public DateTime? LastModifiedAt  { get; set; }

        // ── Computed — not mapped to DB ──────────────────────────────────

        /// <summary>
        /// Days until expiry.
        /// Positive = still valid.
        /// Negative = already expired.
        /// int.MaxValue = no expiry date set (PEA, LME, CDL typically).
        /// </summary>
        [NotMapped]
        public int DaysRemaining =>
            ExpiryDate.HasValue
                ? ExpiryDate.Value.DayNumber -
                  DateOnly.FromDateTime(DateTime.Today).DayNumber
                : int.MaxValue;

        /// <summary>
        /// Human-readable status label — used in DAM Dashboard table.
        /// Thresholds: expired &lt; 0 / warning ≤ 30 / valid &gt; 30 / no expiry
        /// </summary>
        [NotMapped]
        public string StatusLabel =>
            !ExpiryDate.HasValue ? "Sans limite"    :
            DaysRemaining < 0   ? "Expiré"          :
            DaysRemaining <= 30 ? "Expire Bientôt"  :
                                  "Valide";

        /// <summary>
        /// CSS class for the DAM Dashboard cert-valid/warning/expired icons.
        /// Matches the ::before pseudo-element selectors in the view.
        /// </summary>
        [NotMapped]
        public string StatusClass =>
            !ExpiryDate.HasValue  ? "cert-valid"   :
            DaysRemaining < 0    ? "cert-expired"  :
            DaysRemaining <= 30  ? "cert-warning"  :
                                   "cert-valid";

        /// <summary>
        /// Full type label — used in form dropdowns and list displays.
        /// </summary>
        [NotMapped]
        public string CertTypeLabel => CertType switch
        {
            "CdN" => "Certificat de Navigabilité (CdN)",
            "CEN" => "Compte Rendu d'Examen de Navigabilité (CEN)",
            "PEA" => "Programme d'Entretien Agréé (PEA)",
            "LME" => "Liste des Modifications et Équipements (LME)",
            "CDL" => "Configuration Deviation List (CDL)",
            _     => CertType
        };

        // ── Navigation properties ────────────────────────────────────────
        public Aircraft? Aircraft { get; set; }
    }
}
