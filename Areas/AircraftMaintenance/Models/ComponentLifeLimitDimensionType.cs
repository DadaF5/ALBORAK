using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// How a dimension's raw integer value is stored/entered. Hours mirrors
    /// this module's existing FH convention exactly — the underlying column
    /// stores MINUTES, the UI shows/accepts decimal hours (×60 on save, /60
    /// on load, same as every *FHHours DTO field before this revision).
    /// Count and Days both store a plain whole number, entered as-is — Days
    /// exists as a distinct unit only so a future dimension can be
    /// calendar-based without being confused for a plain count (see
    /// IsCalendarBased below, which is the flag that actually changes
    /// calculator behavior; Unit only changes how the UI reads the number).
    /// </summary>
    public enum ComponentLifeLimitDimensionUnit
    {
        Hours = 0,
        Count = 1,
        Days = 2
    }

    /// <summary>
    /// NEW (Revision 13) — replaces the fixed FH/Cycles/CalendarDays/
    /// TgoLandings/FullStopLandings columns that used to be hardcoded on
    /// ComponentLifeLimitStage/ComponentLifeStatus/ComponentEvent/
    /// ComponentInitialReading. A new counter an aircraft type needs (APU
    /// starts, drop count, whatever comes next) is now a ROW here, not a
    /// migration + calculator code change.
    ///
    /// The 5 dimensions this module originally shipped with are seeded by
    /// ComponentLifeLimitDimensionTypeSeeder with STABLE Codes
    /// (FH/CYCLES/CALENDAR_DAYS/TGO_LANDINGS/FULLSTOP_LANDINGS) — code that
    /// needs to special-case one of them (IAircraftReadingProvider's known
    /// sources, the calculator's calendar-day handling) switches on Code,
    /// never on Id, so re-seeding/reordering never breaks anything.
    ///
    /// IsCalendarBased is what actually changes calculator behavior: a
    /// calendar-based dimension is computed from dates (ManufactureDate /
    /// LastOverhaulDate vs. today), never from ComponentEvent snapshots or
    /// IAircraftReadingProvider — CALENDAR_DAYS is the only one seeded with
    /// this set to true. A new REAL counter (APU starts, drop count) is
    /// always IsCalendarBased = false, even though its Unit might be Count.
    /// </summary>
    [Table("ComponentLifeLimitDimensionTypes", Schema = "dbo")]
    public class ComponentLifeLimitDimensionType
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ComponentLifeLimitDimensionUnit Unit { get; set; } = ComponentLifeLimitDimensionUnit.Count;

        public bool IsCalendarBased { get; set; }

        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; }
    }
}
