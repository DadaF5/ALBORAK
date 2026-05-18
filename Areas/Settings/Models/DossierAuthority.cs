using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Step 1 data — Autorité d'emploi (AE) and OGMN approval.
    /// 1:1 with ImmatriculationDossier via shared PK.
    ///
    /// Shared PK pattern:
    ///   DossierId is both the PK and the FK to ImmatriculationDossier.
    ///   This enforces strict 1:1 at the DB level — no orphan rows.
    ///   EF config: HasOne.WithOne.HasForeignKey.
    ///
    /// FKs in this model:
    ///   EmployingAuthorityId → EmployingAuthority
    ///   BaseAerienneId       → Base (single FK — clean)
    /// </summary>
    [Table("DossierAuthority")]
    public class DossierAuthority
    {
        // ── Shared PK = FK to ImmatriculationDossier ─────────────────────
        // No [DatabaseGenerated] — value comes from the parent dossier.
        [Key]
        public int DossierId { get; set; }

        // ── FK → EmployingAuthority ──────────────────────────────────────
        public int? EmployingAuthorityId { get; set; }

        // ── FK → Base (single FK — no dual FK complexity here) ──────────
        public int? BaseAerienneId { get; set; }

        // ── OGMN approval ────────────────────────────────────────────────

        /// <summary>e.g. OGMN-FRA-01 — delivered by DAM</summary>
        public string? OgmnNumber { get; set; }

        public DateOnly? OgmnAggrementDate { get; set; }

        /// <summary>G / G+I / Autre</summary>
        public string? OgmnSousPartie { get; set; }

        /// <summary>Name and rank of OGMN responsible officer</summary>
        public string? OgmnResponsable { get; set; }

        // ── AE contact details ───────────────────────────────────────────
        public string? AeAddress { get; set; }
        public string? AePhone   { get; set; }
        public string? AeEmail   { get; set; }

        // ── Navigation properties ────────────────────────────────────────
        public ImmatriculationDossier Dossier           { get; set; } = null!;
        public EmployingAuthority?    EmployingAuthority { get; set; }
        public Base?                  BaseAerienne       { get; set; }
    }
}
