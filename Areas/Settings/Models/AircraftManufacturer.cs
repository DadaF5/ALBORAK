using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft manufacturer lookup table.
    /// No FK dependencies — pure lookup.
    ///
    /// Inherits from LookupBase:
    ///   Id, Code (30), Name (150), Description (250),
    ///   IsActive, SortOrder (byte)
    ///
    /// Used as FK in:
    ///   DossierAircraft.ManufacturerId
    ///   Aircraft.ManufacturerId  (future)
    ///
    /// Table name: singular — platform convention.
    /// Note: existing DB table is "AircraftManufacturers" (plural).
    ///       Keep [Table] attribute until a rename migration is run.
    /// </summary>
    [Table("AircraftManufacturers")]   // plural — matches existing DB table
    public class AircraftManufacturer : LookupBase
    {
        // All fields inherited from LookupBase:
        //   Id, Code, Name, Description, IsActive, SortOrder

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>Used in dropdown lists: "Code — Name"</summary>
        [NotMapped]
        public string DisplayLabel => $"{Code} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        // Uncomment when Aircraft is added to the context:
        // public ICollection<Aircraft> Aircrafts { get; set; } = [];
    }
}