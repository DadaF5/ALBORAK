using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Step 3 data — Navigabilité and foreign registration history.
    /// 1:1 with ImmatriculationDossier via shared PK.
    ///
    /// ForeignCountryId lives here (not in DossierAircraft)
    /// because it relates to previous registration history,
    /// not to the aircraft's identity or manufacturing origin.
    /// This keeps DossierAircraft free of dual FK to Country.
    ///
    /// FKs in this model:
    ///   CdnDocTypeId     → CdnDocType      (nullable)
    ///   ForeignCountryId → Country         (nullable — single FK)
    ///
    /// No dual FK in this model either. Clean.
    /// </summary>
    [Table("DossierAirworthiness")]
    public class DossierAirworthiness
    {
        // ── Shared PK = FK to ImmatriculationDossier ─────────────────────
        [Key]
        public int DossierId { get; set; }

        // ── Airworthiness document ───────────────────────────────────────

        /// <summary>
        /// true  = aircraft has a valid airworthiness document
        /// false = document not yet obtained (CdN request will be filed)
        /// </summary>
        public bool HasAirworthinessDoc { get; set; } = false;

        // FK → CdnDocType (CDN / ADV / AUT)
        // NULL when HasAirworthinessDoc = false
        public int? CdnDocTypeId { get; set; }

        public string?   CdnReference    { get; set; }
        public DateOnly? CdnDeliveryDate { get; set; }
        public DateOnly? CdnExpiryDate   { get; set; }

        /// <summary>
        /// "Demande de délivrance de CdN associée" checkbox.
        /// Relevant only when HasAirworthinessDoc = false.
        /// </summary>
        public bool CdnRenewalRequested { get; set; } = false;

        // ── Foreign registration history ─────────────────────────────────

        /// <summary>
        /// true = aircraft was previously registered in a foreign state.
        /// Triggers additional fields + mandatory DOC03 upload (Art. 15.3).
        /// </summary>
        public bool WasForeignRegistered { get; set; } = false;

        // FK → Country — foreign state of previous registration
        // Single FK — no dual FK here (OriginCountryId is on DossierAircraft)
        public int? ForeignCountryId { get; set; }

        public string?   FormerImmatriculation { get; set; }
        public DateOnly? ForeignRadiationDate  { get; set; }

        // ── Navigation properties ────────────────────────────────────────
        public ImmatriculationDossier Dossier       { get; set; } = null!;
        public CdnDocType?            CdnDocType    { get; set; }
        public Country?               ForeignCountry { get; set; }
    }
}
