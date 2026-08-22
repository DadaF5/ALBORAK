using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (Revision 12) — 1:1 OPTIONAL with Component. Captures the
    /// "opening balance" for a component that enters this system already
    /// used (a serviceable part transferred in, or historic data being
    /// backfilled) — without this, ComponentLifeStatusCalculator has no way
    /// to know a component already had prior usage, and every counter
    /// silently starts at 0 regardless of real history.
    ///
    /// Deliberately a SEPARATE table, not columns on Component: most
    /// receipts are new-from-factory (all these values are genuinely zero)
    /// or OnCondition components that never consult them at all, so most
    /// Components will never have a row here — avoids permanent NULL/0
    /// columns on the core Component entity for the common case. Also keeps
    /// this "static input, set once at Receipt" data separate from
    /// ComponentLifeStatus, which is fully recomputed/overwritten on every
    /// RecomputeAsync call — mixing the two on one row would risk the
    /// calculator accidentally clobbering an input field it doesn't own.
    ///
    /// Only ComponentLifeStatusCalculator.RecomputeAsync reads this (as the
    /// seed value before walking ComponentEvent history) — nothing else
    /// should. CalendarDays-since-new does NOT need a field here: it's
    /// already derived from Component.ManufactureDate, which the receiving
    /// tech sets to the part's real manufacture date regardless of whether
    /// an opening reading exists.
    /// </summary>
    [Table("ComponentInitialReadings", Schema = "dbo")]
    public class ComponentInitialReading
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Component))]
        public int ComponentId { get; set; }
        public virtual Component? Component { get; set; }

        // ── Prior overhaul(s), before this system ever saw the part ─────
        // PriorOverhaulCount is the TOTAL count of overhauls the part had
        // before Receipt — needed for MissedOverhaulCount accuracy (a part
        // received well past several fixed-grid checkpoints with only ONE
        // prior overhaul on record should flag the ones actually skipped,
        // not the ones already legitimately performed before this system
        // existed). PriorLastOverhaulDate describes only the MOST RECENT
        // prior overhaul — that's what the since-overhaul clock actually
        // resets against; the per-dimension since-that-overhaul baseline
        // lives on each ComponentInitialReadingValue row (see below).
        public int PriorOverhaulCount { get; set; }
        public DateOnly? PriorLastOverhaulDate { get; set; }

        /// <summary>
        /// Revision 13: the per-dimension opening balance (used to be 8
        /// fixed InitialFHMinutes/InitialCycles/InitialTgoLandings/
        /// InitialFullStopLandings/PriorSinceOverhaul* columns) moved to the
        /// generic ComponentInitialReadingValue child table — see that file.
        /// </summary>
        public virtual ICollection<ComponentInitialReadingValue> Values { get; set; } = new List<ComponentInitialReadingValue>();

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Remarks { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string RecordedByUserId { get; set; } = string.Empty;
        [ForeignKey(nameof(RecordedByUserId))]
        public virtual FRAProject.Models.ApplicationUser? RecordedByUser { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
