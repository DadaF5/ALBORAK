namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class AircraftLookupViewModel
    {
        public int Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AcTypeCode { get; set; } = string.Empty;
        public int AcTypeId { get; set; }
    }
}