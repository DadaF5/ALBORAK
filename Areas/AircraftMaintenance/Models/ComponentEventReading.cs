using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// NEW (Revision 13) — one row per dimension snapshotted at this
    /// ComponentEvent. Replaces the 4 fixed AircraftFHAtEventMinutes/
    /// AircraftCyclesAtEvent/AircraftTgoLandingsAtEvent/
    /// AircraftFullStopLandingsAtEvent columns that used to live directly on
    /// ComponentEvent. Only set for Install/Remove/AttachToParent/
    /// DetachFromParent (same events the old fixed columns were set for) —
    /// never for calendar-based dimensions (CALENDAR_DAYS), which are
    /// derived from dates, not snapshotted here. Same immutable,
    /// never-edited-after-creation rule as ComponentEvent itself.
    /// </summary>
    [Table("ComponentEventReadings", Schema = "dbo")]
    public class ComponentEventReading
    {
        public int Id { get; set; }

        public int ComponentEventId { get; set; }
        [ForeignKey(nameof(ComponentEventId))]
        public virtual ComponentEvent? ComponentEvent { get; set; }

        public int DimensionTypeId { get; set; }
        [ForeignKey(nameof(DimensionTypeId))]
        public virtual ComponentLifeLimitDimensionType? DimensionType { get; set; }

        public int ValueAtEvent { get; set; }
    }
}
