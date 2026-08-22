using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (Revision 13) — one row per dimension captured on a
    /// ComponentInitialReading (opening balance at Receipt). Replaces the
    /// fixed InitialFHMinutes/InitialCycles/InitialTgoLandings/
    /// InitialFullStopLandings/PriorSinceOverhaulFHMinutes/
    /// PriorSinceOverhaulCycles/PriorSinceOverhaulTgoLandings/
    /// PriorSinceOverhaulFullStopLandings columns that used to live directly
    /// on ComponentInitialReading.
    /// </summary>
    [Table("ComponentInitialReadingValues", Schema = "dbo")]
    public class ComponentInitialReadingValue
    {
        public int Id { get; set; }

        public int ComponentInitialReadingId { get; set; }
        [ForeignKey(nameof(ComponentInitialReadingId))]
        public virtual ComponentInitialReading? ComponentInitialReading { get; set; }

        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        /// <summary>Since-new opening baseline. 0 = no prior usage on this dimension.</summary>
        public int InitialValue { get; set; }

        /// <summary>Baseline as of the most recent prior overhaul (only meaningful when the parent ComponentInitialReading.PriorLastOverhaulDate is set) — null means "no prior overhaul known for this dimension", NOT zero.</summary>
        public int? PriorSinceOverhaulValue { get; set; }
    }
}
