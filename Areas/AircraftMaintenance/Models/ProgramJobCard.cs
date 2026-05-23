namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class ProgramJobCard
    {
        public int Id { get; set; }

        public int MaintenanceProgramId { get; set; }
        public MaintenanceProgram? MaintenanceProgram { get; set; }

        public int JobCardId { get; set; }
        public JobCard? JobCard { get; set; }

        public int SortOrder { get; set; } = 100;
        public bool IsMandatory { get; set; } = true;
    }
}