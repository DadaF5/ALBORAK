using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW — the "computation reference" lookup: which event/date a
    /// dimension's tracked value is measured FROM. Generalizes the old
    /// profile-wide ComponentLifeBasis (SinceNew/SinceOverhaul only, applied
    /// uniformly to every dimension) down to a per-(profile, dimension)
    /// choice — see ComponentLifeLimitStageDimension.ReferenceBasisId.
    ///
    /// Same role as the legacy webform schema's tblMeca_LimitationReference
    /// table (Dadda shared this as reference) — an extensible lookup rather
    /// than a fixed enum, so a future aircraft-specific basis doesn't need a
    /// code change, same "seeded row, not a migration" principle as
    /// ComponentLifeLimitDimensionType (Revision 13).
    ///
    /// The 4 seeded Codes are a STABLE CONTRACT — ComponentLifeStatusCalculator
    /// switches on Code, never Id (same convention as
    /// ComponentLifeLimitDimensionType). Meaning is UNIT-AGNOSTIC — the same
    /// Code means something coherent whether the dimension it's attached to
    /// is calendar-based or hours/count-based:
    ///
    ///   SINCE_NEW           Calendar: days since ManufactureDate.
    ///                        Hours/Count: cumulative since new (includes any
    ///                        opening/prior usage recorded at Receipt).
    ///                        DEFAULT — matches the pre-existing behavior
    ///                        exactly, so every profile that never sets this
    ///                        explicitly keeps working unchanged.
    ///
    ///   SINCE_OVERHAUL       Calendar: days since the last Overhaul event
    ///                        (or since SINCE_NEW's start if none yet).
    ///                        Hours/Count: resets to 0 at every Overhaul
    ///                        event. Same counter this module already
    ///                        computed as "so" before this basis concept
    ///                        existed.
    ///
    ///   SINCE_INSTALL        Current install window ONLY — resets to 0 at
    ///                        every Remove/DetachFromParent (and again at the
    ///                        next Install/AttachToParent, defensively).
    ///                        Calendar: days since the most recent
    ///                        Install/AttachToParent event, while currently
    ///                        installed; 0 while removed. Hours/Count: usage
    ///                        accrued since the most recent Install/Attach.
    ///
    ///   SINCE_FIRST_INSTALL  Permanent — set once, never resets on later
    ///                        Remove/Reinstall/Overhaul. Calendar: days since
    ///                        the EARLIEST of {first Install/AttachToParent
    ///                        event, first Remove event with Destination =
    ///                        UnderRepair} — i.e. Dadda's Service Life
    ///                        definition ("component life whenever it's
    ///                        removed from stock condition, either installed
    ///                        or removed to workshop"). Hours/Count: usage
    ///                        accrued from the first Install/AttachToParent
    ///                        event onward only (excludes any opening/prior
    ///                        usage baseline SINCE_NEW would include).
    ///
    /// Not typed by Unit/IsCalendarBased on this row deliberately — unlike
    /// ComponentLifeLimitDimensionType (where Unit changes how a value is
    /// entered/displayed), a reference basis is a pure "which date/event is
    /// zero" rule and reads the same regardless of what's being measured.
    /// </summary>
    [Table("ComponentReferenceBases", Schema = "dbo")]
    public class ComponentReferenceBasis
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; }
    }
}
