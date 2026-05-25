namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class MaintenanceProgramDetailsViewModel
    {
        public int Id { get; set; }

        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public List<JobCardListItemViewModel> JobCards { get; set; } = [];
    }
}