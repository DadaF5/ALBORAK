using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// Single, isolated point of contact with Aircraft's "current cumulative"
    /// running-total readings. Confirmed against the real Aircraft.cs:
    ///   - FH/Cycles: real fields are Aircraft.TotalFlightMinutes (int,
    ///     minutes — matches this module's minutes convention exactly) and
    ///     Aircraft.TotalCycles (int).
    ///   - Landings: Aircraft has ONE running total, Aircraft.TotalLandings
    ///     (int), mapped to FULLSTOP_LANDINGS. Confirmed as the HYBRID
    ///     decision (see project handoff doc, "Open question — GBX vs REEL
    ///     landings tracking"): these three stay exactly where they are —
    ///     nothing about them varies by aircraft type, no reason to move
    ///     them off Aircraft's own columns.
    ///
    /// NEW — everything that DOES vary by aircraft family (today:
    /// TGO_LANDINGS, the A-Jet touch-and-go count Dadda found in the legacy
    /// A-Jet webform; tomorrow: whatever the next family needs — C130 APU
    /// starts, drop count, anything) is read from the new generic
    /// AircraftReading table instead of a new Aircraft column. This is the
    /// whole point of the table: a brand-new aircraft-type-specific counter
    /// never needs a migration again — only a seeded
    /// ComponentLifeLimitDimensionType row (already possible from the
    /// existing "Dimensions de suivi" CRUD, optionally scoped to one
    /// AcMainGroup via that model's own AcMainGroupId) plus calls to
    /// IncrementReadingAsync/SetReadingAsync below from wherever that
    /// counter's real source finalizes.
    ///
    /// A dimension Code this provider cannot resolve (no scalar mapping, no
    /// AircraftReading row) is simply ABSENT from the returned dictionary —
    /// callers (ComponentLifeStatusCalculator) already treat a missing key
    /// exactly like "no reading available yet", never an error. This
    /// contract is unchanged by the AircraftReading addition.
    ///
    /// CALENDAR_DAYS deliberately has no entry here at all: it is computed
    /// by the calculator directly from Component.ManufactureDate /
    /// ComponentLifeStatus.LastOverhaulDate, never from Aircraft.
    /// </summary>
    public interface IAircraftReadingProvider
    {
        /// <summary>
        /// Current cumulative reading for every dimension this provider can
        /// resolve — either one of the three hybrid scalars on Aircraft, or
        /// a row in AircraftReading — keyed by
        /// ComponentLifeLimitDimensionType.Code. Missing key = no reading
        /// available; never throws for an unresolvable/unknown Code.
        /// </summary>
        Task<IReadOnlyDictionary<string, int>> GetCurrentReadingsAsync(int aircraftId);

        /// <summary>
        /// NEW — adds `delta` to the current AircraftReading value for
        /// (aircraftId, dimensionCode), creating the row at `delta` if none
        /// exists yet. This is the generic write path for any
        /// aircraft-type-specific counter — call this once per real-world
        /// event that should bump the running total (e.g. a Sortie
        /// finalizing with a non-null TGOsLandings for an A-Jet).
        ///
        /// ASSUMES additive/cumulative semantics, matching how
        /// TotalFlightMinutes/TotalCycles/TotalLandings are presumed to
        /// already accumulate per finalized Sortie (not yet confirmed
        /// against the real Sortie-finalization code at the time this was
        /// written). If that code instead recomputes an absolute total from
        /// scratch each time rather than adding a delta, use
        /// SetReadingAsync instead — do not call both for the same event.
        ///
        /// No-ops silently (returns without writing) if dimensionCode does
        /// not match any ComponentLifeLimitDimensionType.Code — same
        /// "never throws for an unresolvable Code" contract as the read
        /// side, so a typo'd Code fails visibly in testing (the reading
        /// just never appears) rather than crashing a Sortie finalization.
        /// </summary>
        Task IncrementReadingAsync(int aircraftId, string dimensionCode, int delta);

        /// <summary>
        /// NEW — sets (overwrites) the current AircraftReading value for
        /// (aircraftId, dimensionCode), creating the row if none exists.
        /// Alternative to IncrementReadingAsync for a finalization flow that
        /// already computes the aircraft's new absolute total itself rather
        /// than a per-sortie delta. Same silent no-op behavior for an
        /// unresolvable dimensionCode.
        /// </summary>
        Task SetReadingAsync(int aircraftId, string dimensionCode, int value);
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
            // HasValue check needed, just map them straight across. Hybrid
            // decision: these three stay on Aircraft's own columns, never
            // move into AircraftReading.
            result["FH"] = reading.TotalFlightMinutes;
            result["CYCLES"] = reading.TotalCycles;
            result["FULLSTOP_LANDINGS"] = reading.TotalLandings;

            // NEW — every aircraft-type-specific counter (TGO_LANDINGS today)
            // comes from here instead of a hardcoded column. If a future row
            // in this table ever used one of the three Codes above, it wins
            // (extends/overrides the scalar) — but under the Hybrid decision
            // nothing should ever write FH/CYCLES/FULLSTOP_LANDINGS here.
            var generic = await _context.Set<AircraftReading>()
                .Where(r => r.AircraftId == aircraftId)
                .Select(r => new { r.DimensionType!.Code, r.Value })
                .ToListAsync();
            foreach (var g in generic)
                result[g.Code] = g.Value;

            return result;
        }

        public async Task IncrementReadingAsync(int aircraftId, string dimensionCode, int delta)
        {
            if (delta == 0) return;

            var dimensionTypeId = await ResolveDimensionTypeIdAsync(dimensionCode);
            if (dimensionTypeId == null) return; // unresolvable Code — silent no-op, see interface doc

            var row = await _context.Set<AircraftReading>()
                .FirstOrDefaultAsync(r => r.AircraftId == aircraftId && r.DimensionTypeId == dimensionTypeId.Value);

            if (row == null)
            {
                _context.Set<AircraftReading>().Add(new AircraftReading
                {
                    AircraftId = aircraftId,
                    DimensionTypeId = dimensionTypeId.Value,
                    Value = delta,
                    LastUpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                row.Value += delta;
                row.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetReadingAsync(int aircraftId, string dimensionCode, int value)
        {
            var dimensionTypeId = await ResolveDimensionTypeIdAsync(dimensionCode);
            if (dimensionTypeId == null) return; // unresolvable Code — silent no-op, see interface doc

            var row = await _context.Set<AircraftReading>()
                .FirstOrDefaultAsync(r => r.AircraftId == aircraftId && r.DimensionTypeId == dimensionTypeId.Value);

            if (row == null)
            {
                _context.Set<AircraftReading>().Add(new AircraftReading
                {
                    AircraftId = aircraftId,
                    DimensionTypeId = dimensionTypeId.Value,
                    Value = value,
                    LastUpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                row.Value = value;
                row.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int?> ResolveDimensionTypeIdAsync(string dimensionCode)
        {
            return await _context.Set<ComponentLifeLimitDimensionType>()
                .Where(d => d.Code == dimensionCode)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();
        }
    }
}
