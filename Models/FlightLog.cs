

namespace FRAProject.Models
{
    // Authoritative record created once a Sortie is completed (or confirmed).
    public class FlightLog
    {
        public int Id { get; set; }

        // Link back to Sortie and Aircraft
        public int SortieId { get; set; }
        public Sortie? Sortie { get; set; }

        // FK to Aircraft — ensure the type matches Aircraft.Id
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; }

        // Times (UTC recommended). Derive Duration from these.
        public DateTime? TakeOffUtc { get; set; }
        public DateTime? LandingUtc { get; set; }

        // Computed duration in minutes (nullable until both times set)
        public int? DurationMinutes { get; set; }

        // Cycles/landings count
        public int Cycles { get; set; } = 1;

        // Optional hobbs / tach readings (if cockpit provides)
        public decimal? HobbsStart { get; set; }
        public decimal? HobbsEnd { get; set; }
        public decimal? TachStart { get; set; }
        public decimal? TachEnd { get; set; }

        // Extra data: fuel used, mission snapshot, notes
        public decimal? FuelUsedKg { get; set; }
        public string? MissionSnapshot { get; set; }
        public string? Notes { get; set; }

        // Audit
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}