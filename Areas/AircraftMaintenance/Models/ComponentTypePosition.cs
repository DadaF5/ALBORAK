using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Junction: which positions a given Part Number is eligible to be
    /// installed in. Same PN can fit more than one position (e.g. LH/RH main
    /// gear often share a PN) — a plain FK on ComponentType would not allow that,
    /// same "type that can occur in combination needs a junction" lesson as
    /// WorkOrderInspectionType.
    /// </summary>
    [Table("ComponentTypePositions", Schema = "dbo")]
    public class ComponentTypePosition
    {
        public int Id { get; set; }

        [ForeignKey(nameof(ComponentType))]
        public int ComponentTypeId { get; set; }
        public virtual ComponentType? ComponentType { get; set; }

        [ForeignKey(nameof(ComponentPosition))]
        public int ComponentPositionId { get; set; }
        public virtual ComponentPosition? ComponentPosition { get; set; }
    }
}
