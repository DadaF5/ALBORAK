namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardListItemViewModel
    {
        public int Id { get; set; }

        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;

        public int? AtaId { get; set; }
        public string? AtaLabel { get; set; }

        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Specialty { get; set; }

        public int AllocatedTimeMinutes { get; set; }

        public string? ToReference { get; set; }
        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}