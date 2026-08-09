using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Junction: which InspectionType(s) does closing this WorkOrder satisfy.
    // Needed because a single dock visit commonly covers several coinciding
    // periodic inspections at once (e.g. at 1200 airframe hours, PE1 + PE2 +
    // PE4 are all due together — see Table 2-1 in the F-5E maintenance
    // manual). A single WorkOrder.InspectionTypeId FK cannot represent that.
    //
    // Separate from WorkOrderJobCard.MaintenanceProgramId, which answers a
    // different question ("which program did this specific card line come
    // from") — this junction answers "which periodic due-dates does closing
    // this WO clear."
    //
    // Corrective work orders (WOKind = CORRECTIVE, driven by a Snag) simply
    // have zero rows here — no InspectionType involved.
    public class WorkOrderInspectionType
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public int InspectionTypeId { get; set; }
        public InspectionType? InspectionType { get; set; }
    }
}