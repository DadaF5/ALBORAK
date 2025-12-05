using System;
using System.Collections.Generic;

namespace FRAProject.Models
{
    public class Sortie
    {
        public int SortieId { get; set; }

        // FK to ODV
        public int OdvID { get; set; }
        public Odv? Odv { get; set; }

        // aircraft & configuration for this sortie
        public int? AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }
        public string? Configuration { get; set; } // free text or FK to a config table

        // fuel in kg/lb (use one unit in your domain)
        public decimal? FuelQuantity { get; set; }

        // times - use DateTime for full timestamp or TimeSpan for time-of-day
        public DateTime? StartTime { get; set; }    // planned/actual start
        public DateTime? LandingTime { get; set; }  // planned/actual landing

        // optional per-sortie TOFF if you allow multiple sorties with their own TOFFs
        public TimeSpan? TOFF { get; set; }

        public string? Notes { get; set; }

        // navigation - crew assigned to this sortie
        public ICollection<SortieCrew> CrewMembers { get; set; } = new HashSet<SortieCrew>();

        // Completion audit - set when sortie is finalized
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }
        public string? CompletedBy { get; set; }
    }
}
