using Microsoft.EntityFrameworkCore;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// Single, isolated point of contact with Aircraft's "current cumulative"
    /// running-total fields. Confirmed against the real Aircraft.cs this
    /// session:
    ///   - FH/Cycles: real fields are Aircraft.TotalFlightMinutes (int,
    ///     minutes — matches this module's minutes convention exactly) and
    ///     Aircraft.TotalCycles (int). Fixed below, no longer a guess.
    ///   - Landings: Aircraft has ONE running total, Aircraft.TotalLandings
    ///     (int) — not split into touch-and-go vs full-stop the way this
    ///     module's dimensions distinguish them. Provisionally mapped to
    ///     FULLSTOP_LANDINGS below (the more common aviation-log meaning of a
    ///     bare "landings" counter) — CONFIRM this with Dadda before trusting
    ///     it; if TotalLandings is actually a combined TGO+full-stop count,
    ///     or specifically touch-and-goes, this mapping is wrong.
    ///     TGO_LANDINGS still has no source at all and is simply absent from
    ///     the returned dictionary.
    ///
    /// Revision 13: rewritten to be dimension-Code-aware instead of returning
    /// a fixed 4-field record. A dimension Code this provider does not know
    /// how to resolve (including any future aircraft-specific counter added
    /// later purely as a new ComponentLifeLimitDimensionType row — C130 APU
    /// starts, Canadair "number of Drops", etc.) is simply ABSENT from the
    /// returned dictionary — callers (ComponentLifeStatusCalculator) must
    /// treat a missing key exactly like the old fixed fields being null:
    /// "no reading available yet", never an error. Wiring a real source for
    /// a newly added dimension is a follow-up to this class specifically —
    /// flagged as an open question for Dadda (where would APU-start count or
    /// drop count actually live? A new Sortie field? A dedicated log table?
    /// Not decided yet) — and does not block anything else in this module.
    /// CALENDAR_DAYS deliberately has no entry here at all: it is computed
    /// by the calculator directly from Component.ManufactureDate /
    /// ComponentLifeStatus.LastOverhaulDate, never from Aircraft.
    /// </summary>
    public interface IAircraftReadingProvider
    {
        /// <summary>
        /// Current cumulative reading for every non-calendar dimension this
        /// provider can resolve from Aircraft's running totals, keyed by
        /// ComponentLifeLimitDimensionType.Code. Missing key = no reading
        /// available (same meaning the old fixed-field null already had) —
        /// never throws for an unresolvable/unknown Code.
        /// </summary>
        Task<IReadOnlyDictionary<string, int>> GetCurrentReadingsAsync(int aircraftId);
    }

    public class AircraftReadingProvider : IAircraftReadingProvider
    {
        private readonly FRAContext _context;
        public AircraftReadingProvider(FRAContext context) => _context = context;

        public async Task<IReadOnlyDictionary<string, int>> GetCurrentReadingsAsync(int aircraftId)
        {
            var reading = await _context.Set<FRAProject.Areas.Settings.Models.Aircraft>()
                .Where(a => a.Id == aircraftId)
                .Select(a => new { a.TotalFlightMinutes, a.TotalCycles, a.TotalLandings })
                .FirstOrDefaultAsync();

            var result = new Dictionary<string, int>();
            if (reading == null) return result;

            // Aircraft.TotalFlightMinutes/TotalCycles/TotalLandings are plain
            // (non-nullable) int fields on the real Aircraft entity — no
            // HasValue check needed, just map them straight across.
            result["FH"] = reading.TotalFlightMinutes;
            result["CYCLES"] = reading.TotalCycles;

            // TGO_LANDINGS: no source field exists yet on Aircraft — left
            // absent, same as before.
            result["FULLSTOP_LANDINGS"] = reading.TotalLandings;

            return result;
        }
    }
}
