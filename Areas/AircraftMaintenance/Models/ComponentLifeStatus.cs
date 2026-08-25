using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// 1:1 with Component. Materialized/cached result of
    /// ComponentLifeStatusCalculator — mirrors InspectionState's role for
    /// InspectionStatusCalculator. Recomputed whenever a ComponentEvent is
    /// recorded, and on-demand for the due-status list.
    /// Since-new counters are always tracked (informational total-time-since-new,
    /// and the basis used when the resolved profile's LifeBasis = SinceNew).
    /// Since-overhaul counters equal the since-new ones until the first Overhaul
    /// event, then reset — used when LifeBasis = SinceOverhaul.
    /// </summary>
    [Table("ComponentLifeStatuses", Schema = "dbo")]
    public class ComponentLifeStatus
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Component))]
        public int ComponentId { get; set; }
        public virtual Component? Component { get; set; }

        public DateOnly? LastOverhaulDate { get; set; }

        /// <summary>
        /// Revision 13: full per-dimension Cumulative/SinceOverhaul/Remaining
        /// values (used to be 15 fixed FH/Cycles/CalendarDays/TgoLandings/
        /// FullStopLandings columns) moved to the generic
        /// ComponentLifeStatusDimension child table — see that file for the
        /// full breakdown (Details.cshtml reads all of them). The three
        /// Driving* fields below are a small denormalized "headline" summary
        /// — whichever dimension pushed Status to its worst tier (Overdue >
        /// Alert > Ok), same "driving" concept the calculator already
        /// computed internally before this revision. NOTE: on a tie between
        /// two dimensions at the same status tier, the first one encountered
        /// (by DimensionType.SortOrder) wins, not necessarily the one with
        /// the smallest remaining margin — same tie-break the pre-Revision-13
        /// code used. Kept as plain columns so list views (Index, DueList)
        /// can show one badge without a join.
        /// </summary>
        public virtual ICollection<ComponentLifeStatusDimension> Dimensions { get; set; } = new List<ComponentLifeStatusDimension>();

        public int? DrivingDimensionTypeId { get; set; }
        [ForeignKey(nameof(DrivingDimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DrivingDimensionType { get; set; }
        public int? DrivingDimensionRemaining { get; set; }
        public int? DrivingDimensionTolerance { get; set; }

        /// <summary>Which ComponentLifeLimitProfile actually resolved for this Component's S/N — audit/transparency, so it's visible in the UI which schedule (and why — Profile.Reason) is being applied.</summary>
        public int? MatchedLifeLimitProfileId { get; set; }
        [ForeignKey(nameof(MatchedLifeLimitProfileId))]
        public virtual ComponentLifeLimitProfile? MatchedLifeLimitProfile { get; set; }

        /// <summary>SequenceOrder of the stage whose band the tracked value currently falls in — which checkpoint "Remaining*" is counting down to.</summary>
        public int? CurrentStageSequence { get; set; }

        /// <summary>
        /// How many mandatory overhaul checkpoints this component has crossed
        /// WITHOUT a corresponding Overhaul event ever being recorded — distinct
        /// from ordinary "overdue on the next one" status. Non-zero means the
        /// part has actually skipped one or more scheduled overhauls entirely,
        /// not just running late on the current one: real overstress risk
        /// (fatigue, cracking) the "next checkpoint" proximity check alone
        /// would otherwise hide. Forces Status to at least Overdue when > 0.
        /// </summary>
        public int MissedOverhaulCount { get; set; }

        /// <summary>True if the tracked value has passed the profile's final Retirement-stage checkpoint — the component has exceeded its hard life limit and should already be scrapped, not just overhauled.</summary>
        public bool LifeLimitExceeded { get; set; }

        public ComponentLifeStatusValue Status { get; set; } = ComponentLifeStatusValue.Unknown;

        /// <summary>
        /// NEW — true when one or more active, non-expired ComponentDerogation
        /// rows were applied while computing this status (see
        /// ComponentLifeStatusCalculator's derogation-wiring pass). Purely a
        /// display flag so Details/Index/DueList can tell a tech "the Restant
        /// figure below isn't the raw manufacturer schedule, a derogation
        /// already adjusted it" without joining ComponentDerogations. Does not
        /// say WHICH derogation or by how much — see ManageDerogations for
        /// that detail, linked from Details.cshtml.
        /// </summary>
        public bool HasActiveDerogation { get; set; }

        public DateTime LastComputedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
