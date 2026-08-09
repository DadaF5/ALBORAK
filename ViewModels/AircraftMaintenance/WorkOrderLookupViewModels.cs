namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeLookupViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AcTypeId { get; set; }
        public string AcTypeCode { get; set; } = string.Empty;
        public string DisplayLabel => $"{AcTypeCode} — {Code} — {Name}";
    }

    public class MaintenanceProgramLookupViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AcTypeId { get; set; }
        public string DisplayLabel => $"{Code} — {Name}";
    }
}