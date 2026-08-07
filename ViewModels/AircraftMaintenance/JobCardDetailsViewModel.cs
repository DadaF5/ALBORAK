namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardDetailsViewModel
    {
        public int Id { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;

        public int? AtaId { get; set; }
        public string? AtaLabel { get; set; }

        public string? Specialty { get; set; }
        public int AllocatedTimeMinutes { get; set; }

        public string? WorkAreas { get; set; }
        public int? MechNo { get; set; }
        public string? ElectricalPowerRequired { get; set; }
        public string? FigureRef { get; set; }

        public string? ToReference { get; set; }
        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public List<MaintenanceProgramListItemViewModel> MaintenancePrograms { get; set; } = [];
    }
}