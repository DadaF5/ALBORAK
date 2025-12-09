using System;
using System.Collections.Generic;

namespace FRAProject.Models
{
    public class Sortie
    {
        public int Id { get; set; }

        // FK to ODV
        public int OdvId { get; set; }
        public Odv? Odv { get; set; }

        // aircraft & configuration for this sortie
        public int? AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }
        public string? Configuration { get; set; } // free text or FK to a config table

        // fuel in chosen unit (store with precision)
        public decimal? FuelQuantity { get; set; }

        // times - DateTime for full timestamp
        public DateTime? StartTime { get; set; }    // planned/actual start
        public DateTime? LandingTime { get; set; }  // planned/actual landing

        // optional per-sortie TOFF if you allow multiple sorties with their own TOFFs
        public TimeSpan? TOFF { get; set; }

        public string? Notes { get; set; }

        // navigation - crew assigned to this sortie
        public List<SortieCrew> SortieCrews { get; set; } = new List<SortieCrew>();

        // Completion audit - set when sortie is finalized
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }
        public string? CompletedBy { get; set; }
    }
}