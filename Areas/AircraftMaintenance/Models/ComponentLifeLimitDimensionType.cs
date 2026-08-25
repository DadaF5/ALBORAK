using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models; // AcMainGroup — confirmed namespace (AcMainGroup.cs/AcType.cs shared this session)

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
        Days = 2,

        /// <summary>
        /// NEW (Derogation implementation pass) — display-only units for the
        /// two new CALENDAR_MONTHS/CALENDAR_YEARS seeded dimensions (see
        /// ComponentLifeLimitDimensionTypeSeeder). DimensionUnitConverter
        /// treats both exactly like Days (pass-through, no ×60/÷60) — the
        /// ONLY thing Unit changes is which suffix is shown ("mois"/"ans"
        /// instead of "j") and which raw number the user types.
        ///
        /// IMPORTANT — these two dimensions are stored/evaluated by
        /// ComponentLifeStatusCalculator exactly like CALENDAR_DAYS today:
        /// as a plain cumulative DAY COUNT since the reference date
        /// (cum[cd.Id] = cumDays), not via real AddMonths/AddYears date
        /// math. That generalization (a per-Code date-increment table) was
        /// flagged in the Derogation design discussion as a real, separate
        /// piece of work — NOT done in this pass. Consequently
        /// CALENDAR_MONTHS/CALENDAR_YEARS are deliberately EXCLUDED from
        /// ComponentTypesController.PopulateDimensionTypeOptionsAsync (the
        /// life-limit PROFILE stage picker) — using either one on a profile
        /// stage today would have the calculator silently misread an
        /// Interval/BandEnd entered "in months" as a raw day-count. They are
        /// only exposed on the Derogation form (PopulateDerogationDimensionTypeOptionsAsync),
        /// whose Value is just stored/displayed, not run through the
        /// checkpoint-grid calculator in this revision. Re-enable them for
        /// profiles once the calculator's calendar math is generalized.
        /// </summary>
        Months = 3,
        Years = 4
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

        /// <summary>
        /// NEW — null = universal (every aircraft family; FH/Cycles/
        /// CalendarDays/landings stay null). Non-null restricts this
        /// dimension to one AcMainGroup — e.g. an F16-only "Block Hours" or
        /// "Engine Starts" row shouldn't appear in the dimension picker when
        /// editing an F5 or A-Jet PN's life-limit profile. Direct precedent:
        /// the legacy webform schema's tblMeca_Limitations.AcMainGroupId
        /// (Dadda shared this as reference this session). Resolving "which
        /// AcMainGroup(s) does THIS ComponentType belong to" is NOT a direct
        /// FK on ComponentType (a PN can serve multiple positions/AcTypes) —
        /// see IComponentTypeRepository.GetApplicableAcMainGroupIdsAsync,
        /// which walks ComponentTypePosition -> ComponentPosition.AcTypeId ->
        /// AcType.AcMainGroupId and returns the union.
        /// </summary>
        public int? AcMainGroupId { get; set; }
        [ForeignKey(nameof(AcMainGroupId))]
        public virtual AcMainGroup? AcMainGroup { get; set; }
    }
}
