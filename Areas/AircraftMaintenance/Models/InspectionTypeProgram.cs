namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class InspectionTypeProgram
    {
        public int Id { get; set; }

        public int InspectionTypeId { get; set; }
        public InspectionType? InspectionType { get; set; }

        public int MaintenanceProgramId { get; set; }
        public MaintenanceProgram? MaintenanceProgram { get; set; }

        public int SortOrder { get; set; } = 100;
    }
}