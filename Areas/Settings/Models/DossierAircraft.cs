
using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Step 2 data — Aircraft identification and immatriculation.
    /// 1:1 with ImmatriculationDossier via shared PK.
    ///
    /// This is the most FK-heavy model — isolated here so the
    /// master stays clean.
    ///
    /// FKs in this model:
    ///   AircraftCategoryId → AcCategory
    ///   AcTypeId           → AcType
    ///   AircraftVersionId  → AircraftVersion  (nullable)
    ///   MissionRoleId      → MissionRole      (nullable)
    ///   ManufacturerId     → AircraftManufacturer
    ///   PortAttacheId      → Base             (single FK — clean)
    ///   OriginCountryId    → Country          ← DUAL FK 1/2
    ///   ForeignCountryId   → Country          ← DUAL FK 2/2
    ///
    /// Wait — ForeignCountryId belongs to Step 3 (foreign registration).
    /// Moved to DossierAirworthiness to keep this model clean.
    /// Result: only ONE FK to Country here (OriginCountryId). No dual.
    /// </summary>
    [Table("DossierAircraft")]
    public class DossierAircraft
    {
        // ── Shared PK = FK to ImmatriculationDossier ─────────────────────
        [Key]
        public int DossierId { get; set; }

        // ── Aircraft classification ──────────────────────────────────────
        public int? AircraftCategoryId  { get; set; }
        public int? AcTypeId            { get; set; }

        /// <summary>
        /// Free-text serie — e.g. "Block 52+".
        /// Complements AircraftVersionId when version is known.
        /// </summary>
        public string? AircraftSerie     { get; set; }

        public int? AircraftVersionId   { get; set; }
        public int? MissionRoleId       { get; set; }

        // ── Constructeur ─────────────────────────────────────────────────
        public int? ManufacturerId      { get; set; }

        /// <summary>Constructeur serial number — from identification plate.</summary>
        public string? SerialNumber      { get; set; }

        public DateOnly? ManufactureDate  { get; set; }
        public DateOnly? ServiceEntryDate { get; set; }

        // ── Base & Origin ────────────────────────────────────────────────

        /// <summary>Physical home base of the aircraft — single FK to Base.</summary>
        public int? PortAttacheId       { get; set; }

        /// <summary>
        /// Country where the aircraft was manufactured.
        /// Single FK to Country — ForeignCountryId moved to DossierAirworthiness.
        /// No dual FK in this model.
        /// </summary>
        public int? OriginCountryId     { get; set; }

        // ── Immatriculation ──────────────────────────────────────────────

        /// <summary>
        /// 3-letter suffix — e.g. "FAA" → full mark "CN-FAA".
        /// Unique at DB level. Assigned by DAM.
        /// </summary>
        public string? ImmatriculationSuffix { get; set; }

        // ── Computed — not mapped to DB ──────────────────────────────────

        /// <summary>Full immatriculation — "CN-" + ImmatriculationSuffix.</summary>
        [NotMapped]
        public string? FullImmatriculation =>
            string.IsNullOrWhiteSpace(ImmatriculationSuffix)
                ? null
                : $"CN-{ImmatriculationSuffix.ToUpper()}";

        // ── Navigation properties ────────────────────────────────────────
        public ImmatriculationDossier  Dossier          { get; set; } = null!;
        public AcCategory?             AircraftCategory  { get; set; }
        public AcType?                 AcType            { get; set; }
        public AircraftVersion?        AircraftVersion   { get; set; }
        public MissionRole?            MissionRole       { get; set; }
        public AircraftManufacturer?   Manufacturer      { get; set; }
        public Base?                   PortAttache       { get; set; }
        public Country?                OriginCountry     { get; set; }
    }
}
