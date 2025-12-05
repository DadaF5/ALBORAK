namespace FRAProject.ViewModels
{
    public class SortieVm
    {
        public int? SortieId { get; set; } // empty for new sorts
        public int? AircraftId { get; set; }
        public string? Configuration { get; set; }
        public decimal? FuelQuantity { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? LandingTime { get; set; }
        public TimeSpan? TOFF { get; set; }
        public string? Notes { get; set; }

        public List<SortieCrewVm> Crew { get; set; } = new List<SortieCrewVm>();
    }
}
