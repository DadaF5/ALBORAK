namespace FRAProject.ViewModels
{
    public class SortieVm
    {
        // optional when editing existing sortie
        public int? SortieId { get; set; }

        // FK to Aircraft (nullable if not selected)
        public int? AircraftId { get; set; }

        // Free-text configuration or config identifier
        public string? Configuration { get; set; }

        // Fuel quantity (use same unit across domain)
        public decimal? FuelQuantity { get; set; }

        // Planned/actual times
        public DateTime? StartTime { get; set; }
        public DateTime? LandingTime { get; set; }

        // Optional per-sortie TOFF (time of day)
        public TimeSpan? TOFF { get; set; }
        // Real times
        public DateTime? RealTOFF { get; set; }
        public DateTime? RealLandingTime { get; set; }
        


        public string? Notes { get; set; }

        // Audit / completion
        public bool IsCompleted { get; set; } = false;

        // Collection of crew assignments for this sortie
        public List<SortieCrewVm> Crew { get; set; } = new List<SortieCrewVm>();
    }
}
