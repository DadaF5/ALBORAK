using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (Revision 13) — one row per dimension actually configured at a
    /// given ComponentLifeLimitStage. Replaces the 15 fixed Interval*/
    /// BandEnd*/Tolerance* columns that used to live directly on
    /// ComponentLifeLimitStage. A stage that only limits FH now has exactly
    /// one row here instead of 12 unused null columns out of 15.
    /// Values are stored in the DimensionType's own unit convention (minutes
    /// for Hours, whole numbers for Count/Days) — same as the old *FHMinutes
    /// fields, just generalized.
    /// </summary>
    [Table("ComponentLifeLimitStageDimensions", Schema = "dbo")]
    public class ComponentLifeLimitStageDimension
    {
        public int Id { get; set; }

        public int ComponentLifeLimitStageId { get; set; }
        [ForeignKey(nameof(ComponentLifeLimitStageId))]
        public virtual ComponentLifeLimitStage? ComponentLifeLimitStage { get; set; }

        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        /// <summary>Recurring interval within this band.</summary>
        public int? Interval { get; set; }

        /// <summary>Cumulative value where this band ends and the next stage's interval takes over.</summary>
        public int? BandEnd { get; set; }

        /// <summary>Absolute: same unit as Interval. PercentOfInterval (see ComponentLifeLimitStage.ToleranceType, still stage-level): whole-number percent of Interval.</summary>
        public int? Tolerance { get; set; }
    }
}
