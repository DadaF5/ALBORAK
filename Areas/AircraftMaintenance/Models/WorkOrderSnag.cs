// Areas/AircraftMaintenance/Models/WorkOrderSnag.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Junction — many-to-many, same lesson as WorkOrderInspectionType:
    // one corrective WO can resolve several Snags at once.
    [Table("WorkOrderSnags", Schema = "dbo")]
    public class WorkOrderSnag
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        [ForeignKey(nameof(WorkOrderId))]
        public virtual WorkOrder? WorkOrder { get; set; }

        public int SnagId { get; set; }
        [ForeignKey(nameof(SnagId))]
        public virtual Snag? Snag { get; set; }

        // True once WO closes and this Snag was auto-closed as a result
        public bool ResolvedOnClose { get; set; }
    }
}