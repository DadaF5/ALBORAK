using Microsoft.CodeAnalysis.Recommendations;
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

        // NEW: denormalized BaseId for fast multi-base queries on sorties
        public int? BaseId { get; set; }

        // aircraft & configuration for this sortie
        public int? AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }
        public string? Configuration { get; set; } // free text or FK to a config table

        // fuel in chosen unit (store with precision)
        public decimal? FuelQuantity { get; set; }

        // times - DateTime for full timestamp
        public DateTime? StartTime { get; set; }    
        public DateTime? LandingTime { get; set; }  
        public TimeSpan? TOFF { get; set; }

        public string? Notes { get; set; }

        // navigation - crew assigned to this sortie
        public List<SortieCrew> SortieCrews { get; set; } = new List<SortieCrew>();

        // Audit fields(recommended)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }            // user id or username
        public DateTime? UpdatedAtUtc { get; set; }
        public string? UpdatedBy { get; set; }            // user id or username

        // Completion audit
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }
        public string? CompletedBy { get; set; }

        // Optional concurrency token
        public byte[]? RowVersion { get; set; }
    }
}