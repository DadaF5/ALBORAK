using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.SquadronOps.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft Type — specific variant within a main group.
    /// Hierarchy: AcCategory → AcMainGroup → AcType → AircraftVersion
    ///
    /// Examples:
    ///   AcMainGroup = "Chasse 2BAFRA" → AcType = "F-16C", "F-16D"
    ///   AcMainGroup = "Transport"     → AcType = "CN-235", "C-130"
    ///
    /// Two FK parents:
    ///   AcMainGroupId          → AcMainGroup  (required)
    ///   AircraftManufacturerId → AircraftManufacturer (optional)
    ///
    /// Technical specs used by:
    ///   SquadronOps  — sortie planning (SeatCount, MaxPassengers)
    ///   MRO2         — maintenance limits (MaxGrossWeight, MaxEngines)
    ///
    /// Table: "AcTypes" — plural kept (matches existing DB).
    /// Schema attribute removed — EF uses default schema (dbo).
    ///
    /// Changes vs original:
    ///   [Required] / [ForeignKey] annotations removed → Fluent API
    ///   virtual keyword removed → not needed in EF Core
    ///   new HashSet<>() → []
    ///   Code made non-nullable → Required (platform convention)
    ///   using SquadronOps.Controllers removed → was wrong namespace
    /// </summary>
    [Table("AcTypes")]
    public class AcType
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── Identity fields ──────────────────────────────────────────────

        /// <summary>
        /// Short uppercase code — unique per AcMainGroup.
        /// e.g. "F16C", "CN235", "C130"
        /// Changed from nullable to required — platform convention.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Full type name — e.g. "F-16C Fighting Falcon"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Optional description — 250 chars.</summary>
        public string? Description { get; set; }

        // ── Display ordering & status ─────────────────────────────────────
        public byte SortOrder { get; set; } = 99;
        public bool IsActive  { get; set; } = true;

        // ── Technical specifications ──────────────────────────────────────
        // Used by SquadronOps (sortie planning) and MRO2 (maintenance limits).

        /// <summary>Maximum gross weight in kilograms.</summary>
        public double MaxGrossWeight { get; set; }

        /// <summary>Maximum number of engines.</summary>
        public int MaxEngines { get; set; }

        /// <summary>Total seat count (crew + passengers).</summary>
        public int SeatCount { get; set; }

        /// <summary>Maximum passenger capacity (excluding crew).</summary>
        public int MaxPassengers { get; set; }

        // ── FK → AcMainGroup (required) ───────────────────────────────────
        public int AcMainGroupId { get; set; }

        // ── FK → AircraftManufacturer (optional) ──────────────────────────
        public int? AircraftManufacturerId { get; set; }

        // ── Computed — not mapped to DB ───────────────────────────────────
        /// <summary>Used in dropdown lists: "Code — Name"</summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ─────────────────────────────────────────
        // No virtual — EF Core uses explicit Include(), not lazy loading.
        public AcMainGroup?          AcMainGroup          { get; set; }
        public AircraftManufacturer? AircraftManufacturer { get; set; }

        // Children
        public ICollection<AircraftVersion> AircraftVersions { get; set; } = [];
        public ICollection<Aircraft>        Aircrafts        { get; set; } = [];
        public ICollection<Sortie>          Sorties          { get; set; } = [];
    }
}
