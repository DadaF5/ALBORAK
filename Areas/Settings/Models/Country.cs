using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// Country lookup table — EF Core entity only.
    /// Validation rules live in CountryFormDto.
    ///
    /// Used as FK in:
    ///   ImmatriculationDossier.OriginCountryId  — country of manufacture
    ///   ImmatriculationDossier.ForeignCountryId — country of former registration
    ///   AircraftManufacturer.CountryId          — manufacturer country (future)
    /// </summary>
    [Table("Country")]
    public class Country
    {
        // ── Primary Key ──────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ── ISO 3166-1 alpha-2 code ──────────────────────────────────────
        /// <summary>
        /// ISO 3166-1 alpha-2 code — e.g. MA, FR, US.
        /// Unique index configured in OnModelCreating.
        /// Fixed CHAR(2) column type configured in OnModelCreating.
        /// </summary>
        public string IsoCode { get; set; } = string.Empty;

        // ── Name ─────────────────────────────────────────────────────────
        public string Name { get; set; } = string.Empty;

        // ── Continent ────────────────────────────────────────────────────
        public string? Continent { get; set; }

        // ── Display ordering ─────────────────────────────────────────────
        public int SortOrder { get; set; } = 0;

        // ── Soft delete ──────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;

        // ── Computed — not mapped to DB ──────────────────────────────────
        /// <summary>
        /// Used in dropdown lists: "MA — Maroc"
        /// Built in C# — never hits the DB.
        /// </summary>
        [NotMapped]
        public string DisplayLabel => $"{IsoCode} — {Name}";

        // ── Navigation properties ────────────────────────────────────────
        // Uncomment when ImmatriculationDossier is added to the context.
        // public ICollection<ImmatriculationDossier> OriginCountryDossiers  { get; set; } = [];
        // public ICollection<ImmatriculationDossier> ForeignCountryDossiers { get; set; } = [];
    }
}