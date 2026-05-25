namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderJobCardSignOffItemViewModel
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }

        public string? SignedByUserName { get; set; }
        public DateTime? SignedAtUtc { get; set; }
        public bool? Accepted { get; set; }
        public string? Remarks { get; set; }
    }
}