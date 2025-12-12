using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public enum SortieStatus
    {
        Planned = 0,
        AircraftAssigned = 10,
        Airborne = 20,
        Landed = 30,
        Finalized = 40
    }

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

        // Workflow fields
        public SortieStatus Status { get; set; } = SortieStatus.Planned;

        // Real times
        public DateTime? RealTOFF { get; set; }
        public DateTime? RealLandingTime { get; set; }

        public string? Notes { get; set; }

        // Post flight
        [Display(Name = "Day Hours")]
        public double? DayHours { get; set; }

        [Display(Name = "Night Hours")]
        public double? NightHours { get; set; }

        // Computed Duration in hours (safe: handle nulls)
        [Display(Name = "Duration (hours)")]
        public double? DurationHours => (DayHours ?? 0.0) + (NightHours ?? 0.0);

        // Persisted duration in minutes (Squadron-finalized)
        // This is the field you asked for: e.g. 1:05 -> 65
        public int? DurationMinutes { get; set; }

        [Display(Name = "Approachs")]
        public int? Approachs { get; set; }
        public int? Landings { get; set; }

        [Display(Name = "T/G O's Landings")]
        public int? TGOsLandings { get; set; }
        public double? HobbsStart { get; set; }
        public double? HobbsEnd { get; set; }
        public double? HobbsUsed { get; set; }
        public double? TachStart { get; set; }
        public double? TachEnd { get; set; }
        public double? TachUsed { get; set; }
        public double? AirframeHours { get; set; }
        public double? AirframeCycles { get; set; }
        [Display(Name = "Inst Simulated")]
        public double? InstSimulated { get; set; }
        [Display(Name = "Inst Actual")]
        public double? InstActual { get; set; }

        [Display(Name = "IFR Hours")]
        public double? IFRHours { get; set; }
        public int? Cycles { get; set; }

        [Display(Name = "Fuel Used (Liters)")]
        [Column(TypeName ="decimal(12,2)")]
        public decimal? FuelUsedLiters { get; set; }
        public string? Malfunctions { get; set; }
        public bool IsCompleted { get; set; }
        public bool? IsFinalized { get; set; }
        public bool? BrakeChuteUsed { get; set; }


        // Squadron final report metrics (nullable to indicate "not provided")
        [Display(Name = "Interceptions")]
        public int? Interceptions { get; set; }

        [Display(Name = "Radar Contacts")]
        public int? RadarContacts { get; set; }

        [Display(Name = "Approach Contacts")]
        public int? AppContacts { get; set; }

        [Display(Name = "Squadron Notes")]
        public string? SquadronReportNotes { get; set; }

        // Audit fields(recommended)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }            // user id or username
        public DateTime? UpdatedAtUtc { get; set; }
        public string? UpdatedBy { get; set; }            // user id or username

        // Completion audit       
        public DateTime? CompletedAtUtc { get; set; }
        public string? CompletedBy { get; set; }

        public DateTime? FinalizedAtUtc { get; set; }
        public string? FinalizedBy { get; set; }


        // Concurrency token for EF - make sure this has [Timestamp]
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // navigation - crew assigned to this sortie
        public List<SortieCrew> SortieCrews { get; set; } = new List<SortieCrew>();
    }
}