using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    /// <summary>
    /// NEW (Revision 13). Single shared conversion between a
    /// ComponentLifeLimitDimensionType's STORED unit (minutes for Hours —
    /// matches the pre-Revision-13 FH-in-minutes convention — plain int for
    /// Count/Days) and its DISPLAY unit (decimal hours for Hours, plain
    /// otherwise). Used by every place that reads/writes a per-dimension
    /// value from a form: ComponentLifeLimitProfileService (stage
    /// Interval/BandEnd/Tolerance), ComponentService (Receipt opening
    /// readings, event history display, list/due-list headline display).
    /// Keeping this in one place avoids those call sites silently drifting
    /// on rounding behavior.
    /// </summary>
    public static class DimensionUnitConverter
    {
        public static decimal? ToDisplayValue(ComponentLifeLimitDimensionUnit unit, int? storedValue)
        {
            if (!storedValue.HasValue) return null;
            return unit == ComponentLifeLimitDimensionUnit.Hours ? storedValue.Value / 60m : storedValue.Value;
        }

        public static decimal ToDisplayValue(ComponentLifeLimitDimensionUnit unit, int storedValue) =>
            unit == ComponentLifeLimitDimensionUnit.Hours ? storedValue / 60m : storedValue;

        public static int? ToStoredValue(ComponentLifeLimitDimensionUnit unit, decimal? displayValue)
        {
            if (!displayValue.HasValue) return null;
            return unit == ComponentLifeLimitDimensionUnit.Hours ? (int)(displayValue.Value * 60) : (int)Math.Round(displayValue.Value);
        }
    }
}
