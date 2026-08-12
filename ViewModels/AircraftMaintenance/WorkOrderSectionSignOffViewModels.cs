namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderSectionSignOffPageViewModel
    {
        public int WorkOrderSectionId { get; set; }
        public string SectionLabel { get; set; } = string.Empty;
        public string? FormNumber { get; set; }

        public List<WorkOrderSectionSignOffItemViewModel> SignOffs { get; set; } = [];

        public bool AllSigned => SignOffs.All(s => s.IsSigned);
    }

    public class WorkOrderSectionSignOffItemViewModel
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string LevelLabel { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public string? SignedByName { get; set; }
        public string? StampReference { get; set; }
        public DateTime? SignedAtUtc { get; set; }
        public string? Remarks { get; set; }

        public bool IsSigned => SignedAtUtc.HasValue;
    }
}