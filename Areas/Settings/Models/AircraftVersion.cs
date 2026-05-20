
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft version / variant — child of AcType.
    /// Represents a specific block or variant within a type.
    /// e.g. AcType = F-16, AircraftVersion = Block 52+
    ///
    /// Inherits from LookupBase:
    ///   Id, Code (30), Name (150), Description (250),
    ///   IsActive, SortOrder (byte)
    ///
    /// Unique indexes configured in OnModelCreating:
    ///   UQ_AircraftVersions_Code_AcType  (Code + AcTypeId)
    ///   UQ_AircraftVersions_Name_AcType  (Name + AcTypeId)
    ///   — same Code/Name allowed across different AcTypes
    ///
    /// Table name: singular — platform convention.
    /// </summary>
    [Table("AircraftVersions")]   // singular — platform convention
    public class AircraftVersion : LookupBase
    {
        // ── FK → AcType ──────────────────────────────────────────────────
        // Configured in OnModelCreating — no [Required] or [ForeignKey]
        // attributes here (EF Core convention + Fluent API is sufficient).
        public int AcTypeId { get; set; }

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "Code — Name"
        /// e.g. "BLK52 — Block 52+"
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        // No virtual keyword — not needed in EF Core (lazy loading
        // requires explicit configuration; we use eager loading instead).
        public AcType? AcType { get; set; }
    }
}
