using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW — generic "current cumulative reading" storage for any
    /// aircraft-level counter that is NOT one of the three universal
    /// scalars already on Aircraft (TotalFlightMinutes/TotalCycles/
    /// TotalLandings — those stay exactly where they are; see the Hybrid
    /// decision in the project handoff doc, "Open question — GBX vs REEL
    /// landings tracking"). One row per (AircraftId, DimensionTypeId)
    /// holding the CURRENT value — not a change log. (That's a different
    /// concept, already covered on the Component side by
    /// ComponentEventReading — a per-event snapshot, not a running total.)
    ///
    /// This is the piece that makes "a new aircraft-type-specific counter is
    /// just a seeded ComponentLifeLimitDimensionType row, not a migration"
    /// (already true on the Component side since Revision 13) ALSO true at
    /// the Aircraft level. TGO_LANDINGS today (A-Jet touch-and-go count,
    /// discovered from the legacy A-Jet webform), C130 APU starts or
    /// whatever a different family needs tomorrow — none of it will ever
    /// need a new Aircraft column or a migration once this table exists.
    /// Only a seeded DimensionType row (optionally scoped to one
    /// AcMainGroup via that model's own AcMainGroupId field — e.g. an
    /// A-Jet-only counter doesn't have to appear for F5/F16/C130) plus a
    /// write into this table from wherever that counter's real source
    /// finalizes (a Sortie, a manual correction screen, whatever it is).
    ///
    /// Absence of a row = "no reading available yet" — the exact same
    /// contract IAircraftReadingProvider already documents for a missing
    /// dictionary key on GetCurrentReadingsAsync, never an error.
    /// </summary>
    [Table("AircraftReadings", Schema = "dbo")]
    public class AircraftReading
    {
        public int Id { get; set; }

        public int AircraftId { get; set; }
        [ForeignKey(nameof(AircraftId))]
        public virtual Aircraft? Aircraft { get; set; }

        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        /// <summary>
        /// Same minutes-for-Hours / plain-count convention as every other
        /// dimension value in this module (see
        /// ComponentLifeLimitDimensionUnit) — the UI layer converts, this
        /// column always stores the raw underlying int.
        /// </summary>
        public int Value { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
