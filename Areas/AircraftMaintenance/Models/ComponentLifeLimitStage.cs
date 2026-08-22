using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// One ordered band in a ComponentLifeLimitProfile's staged schedule.
    /// Example (PN-123abc/SN-xyz): stage 1 = first overhaul at 3000h (interval
    /// 3000, band-end 3000); stage 2 = every 1000h up to 6000h (interval 1000,
    /// band-end 6000); stage 3 = every 500h up to 10000h life limit (interval
    /// 500, band-end 10000, StageType=Retirement).
    ///
    /// The calculator walks stages in SequenceOrder, generating a checkpoint
    /// grid per dimension by repeatedly adding each stage's interval up to its
    /// band-end (see ComponentLifeStatusCalculator — this is a FIXED grid from
    /// zero, not "last-done + interval"; flagged as a design choice, confirm
    /// with Dadda if real parts need a floating/last-done-relative schedule
    /// instead for SinceNew-basis profiles).
    ///
    /// A dimension with no ComponentLifeLimitStageDimension row at this stage
    /// is simply not constrained during that band (e.g. a stage that only
    /// limits FH, not cycles) — same meaning the old null columns had.
    ///
    /// Revision 13: the per-dimension Interval/BandEnd/Tolerance columns
    /// (used to be 15 fixed FH/Cycles/CalendarDays/TgoLandings/FullStopLandings
    /// columns) moved to the generic ComponentLifeLimitStageDimension child
    /// table — see that file. Adding a new dimension (APU starts, drop count)
    /// no longer touches this entity at all.
    /// </summary>
    [Table("ComponentLifeLimitStages", Schema = "dbo")]
    public class ComponentLifeLimitStage
    {
        public int Id { get; set; }

        public int ComponentLifeLimitProfileId { get; set; }
        [ForeignKey(nameof(ComponentLifeLimitProfileId))]
        public virtual ComponentLifeLimitProfile? ComponentLifeLimitProfile { get; set; }

        public int SequenceOrder { get; set; }

        public ComponentLifeLimitStageType StageType { get; set; } = ComponentLifeLimitStageType.Overhaul;

        /// <summary>Absolute: same unit as Interval on each dimension row (minutes for an Hours-unit dimension). PercentOfInterval: whole-number percent (20 = 20%) of that dimension's own Interval at this stage. Stage-level (not per-dimension) — same tolerance MODE applies to every dimension configured on this stage; only the tolerance VALUE is per-dimension (ComponentLifeLimitStageDimension.Tolerance).</summary>
        public ComponentToleranceType ToleranceType { get; set; } = ComponentToleranceType.Absolute;

        public virtual ICollection<ComponentLifeLimitStageDimension> Dimensions { get; set; } = new List<ComponentLifeLimitStageDimension>();
    }
}
