namespace FRAProject.ViewModels.AircraftMaintenance
{
    // Purpose-built for the WorkOrder Create checkbox list — carries
    // AcTypeId so the client can filter by selected aircraft, which the
    // generic LookupOptionViewModel (Id/Label only) can't do. Additive —
    // doesn't replace WorkOrderFormViewModel.InspectionTypes.
    public class InspectionTypeCheckItemViewModel
    {
        public int Id { get; set; }
        public int AcTypeId { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}