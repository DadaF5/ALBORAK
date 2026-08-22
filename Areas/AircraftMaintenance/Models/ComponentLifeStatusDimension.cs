using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (Revision 13) — one row per dimension the calculator actually
    /// tracked for this Component's ComponentLifeStatus. Replaces the 15
    /// fixed Cumulative*/SinceOverhaul*/Remaining* columns that used to live
    /// directly on ComponentLifeStatus. Fully recomputed/overwritten on every
    /// ComponentLifeStatusCalculator.RecomputeAsync call — same "materialized,
    /// never hand-edited" rule as ComponentLifeStatus itself.
    /// </summary>
    [Table("ComponentLifeStatusDimensions", Schema = "dbo")]
    public class ComponentLifeStatusDimension
    {
        public int Id { get; set; }

        public int ComponentLifeStatusId { get; set; }
        [ForeignKey(nameof(ComponentLifeStatusId))]
        public virtual ComponentLifeStatus? ComponentLifeStatus { get; set; }

        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        public int Cumulative { get; set; }
        public int SinceOverhaul { get; set; }
        public int? Remaining { get; set; }
    }
}
