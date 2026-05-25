namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderJobCardItemViewModel
    {
        public int Id { get; set; }

        public int JobCardId { get; set; }
        public string JobCardLabel { get; set; } = string.Empty;

        public int MaintenanceProgramId { get; set; }
        public string MaintenanceProgramLabel { get; set; } = string.Empty;

        public int SortOrder { get; set; }
        public bool IsMandatory { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? NAJustification { get; set; }
        public string? Observations { get; set; }

        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        public List<WorkOrderJobCardSignOffItemViewModel> SignOffs { get; set; } = [];
    }
}