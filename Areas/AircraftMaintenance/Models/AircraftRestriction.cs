using FRAProject.Areas.Settings.Models;   // AircraftCertificate
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Operational or maintenance restriction on an aircraft.
    ///
    /// Restriction types (RestrictionType):
    ///   OPS — Operational: flight envelope, G-limit, IFR ban, altitude cap…
    ///   MNT — Maintenance: SB compliance, part condition, inspection hold…
    ///
    /// Severity levels:
    ///   CRITICAL — aircraft must not fly until resolved
    ///   HIGH     — significant operational impact
    ///   MEDIUM   — monitored, minor impact
    ///
    /// Optional link to AircraftCertificate:
    ///   A restriction may originate from an expired or suspended cert.
    ///   CertificateId is null when restriction is standalone.
    ///
    /// Table: "AircraftRestrictions"
    /// </summary>
    [Table("AircraftRestrictions")]
    public class AircraftRestriction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── FK → Aircraft ────────────────────────────────────────────────
        public int AircraftId { get; set; }

        // ── Optional FK → AircraftCertificate ────────────────────────────
        /// <summary>
        /// Linked certificate — e.g. restriction raised because CEN expired.
        /// Null when restriction is independent of a certificate.
        /// </summary>
        public int? CertificateId { get; set; }

        // ── Classification ────────────────────────────────────────────────
        /// <summary>"OPS" | "MNT"</summary>
        public string RestrictionType { get; set; } = string.Empty;

        /// <summary>"CRITICAL" | "HIGH" | "MEDIUM"</summary>
        public string Severity { get; set; } = "HIGH";

        // ── Identity ──────────────────────────────────────────────────────
        /// <summary>Official reference number. e.g. "CRM-2024-045"</summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>What is restricted. e.g. "Vol IFR interdit"</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Issuing authority. e.g. "DAM", "OGMN"</summary>
        public string? IssuedBy { get; set; }

        // ── Dates ─────────────────────────────────────────────────────────
        public DateOnly StartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }   // null = indefinite

        // ── Notes ─────────────────────────────────────────────────────────
        public string? Notes { get; set; }

        // ── Soft delete ───────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Audit ─────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // ── Computed [NotMapped] ──────────────────────────────────────────
        [NotMapped]
        public int DaysRemaining =>
            ExpiryDate.HasValue
                ? ExpiryDate.Value.DayNumber -
                  DateOnly.FromDateTime(DateTime.Today).DayNumber
                : int.MaxValue;

        [NotMapped]
        public bool IsExpired =>
            ExpiryDate.HasValue && DaysRemaining < 0;

        [NotMapped]
        public string SeverityBadgeClass => Severity switch
        {
            "CRITICAL" => "bg-danger",
            "HIGH" => "bg-warning text-dark",
            "MEDIUM" => "bg-info text-dark",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string TypeLabel => RestrictionType switch
        {
            "OPS" => "Opérationnelle",
            "MNT" => "Maintenance",
            _ => RestrictionType
        };

        // ── Navigation ────────────────────────────────────────────────────
        public Aircraft? Aircraft { get; set; }
        public AircraftCertificate? Certificate { get; set; }
    }
}