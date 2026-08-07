namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class AtaLookupViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayLabel => $"{Code} — {Name}";
    }
}