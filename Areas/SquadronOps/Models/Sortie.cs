
using FRAProject.Areas.Settings.Models;
using FRAProject.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.SquadronOps.Models
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
        // ✅ Aircraft TYPE requirement (planning phase)
        public int AcTypeId { get; set; }
        public AcType AcType { get; set; }

        public int? AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }


        public string SortieCode { get; set; } = "";
        // aircraft & configuration for this sortie

        public string? Configuration { get; set; } // free text or FK to a config table
        public int Sequence { get; set; }
        // fuel in chosen unit (store with precision)
        public decimal? FuelQuantity { get; set; }

        // times - DateTime for full timestamp
        public DateTime? StartTime { get; set; }
        public DateTime? LandingTime { get; set; }
        public TimeSpan? TOFF { get; set; }

        // Workflow fields
        public SortieStatus Status { get; set; } = SortieStatus.Planned;

        // ════════════════════════════════════════════════════════════════
        //  Real times — CONVENTION LOCKED THIS PASS (see
        //  ALBORAK_Handoff_Derogations_DetailsHub.md, "OPS vs Maintenance
        //  time bases" for the full discussion). Three genuinely different
        //  windows exist per sortie and must never be conflated:
        //    - RealTOFF / RealLandingTime  -> AIRFRAME flight hours (FH).
        //      True wheels-up to true touchdown ONLY — never block-off,
        //      never engine-start. A ground hold at the holding point
        //      (weather/runway/traffic) happens strictly BEFORE RealTOFF
        //      and is correctly excluded here by definition.
        //    - EngineStartTime / EngineStopTime (NEW, below) -> ENGINE_HOURS.
        //      Engine-run time, ground running included — genuinely
        //      different from FH for engine LLPs that accrue by run time,
        //      not airborne time.
        //    - BlockOffTime / BlockOnTime (NEW, below) -> OPS's own
        //      reporting/crew-duty-time basis. Chocks-off to chocks-on.
        //      Maintenance NEVER reads these two fields.
        //  Whatever finalizes a Sortie must feed each basis from its own
        //  dedicated pair — never derive one basis from another's fields.
        // ════════════════════════════════════════════════════════════════
        public DateTime? RealTOFF { get; set; }
        public DateTime? RealLandingTime { get; set; }

        /// <summary>
        /// NEW — engine-run window (start to shutdown), ground running
        /// included. Feeds the new ENGINE_HOURS dimension via
        /// IAircraftReadingProvider.IncrementReadingAsync, exactly the same
        /// generic AircraftReadings mechanism TGO_LANDINGS already uses —
        /// no new table needed for this. ONE pair per sortie: assumes every
        /// engine on the aircraft starts/stops together (matches how an
        /// installed engine Component already inherits the aircraft's
        /// shared FH/CYCLES today). Does NOT yet support single-engine taxi
        /// or staggered per-engine shutdown — flagged as a real limitation,
        /// not an oversight, if that ever needs to be tracked precisely for
        /// a multi-engine type like the C130.
        /// </summary>
        public DateTime? EngineStartTime { get; set; }
        public DateTime? EngineStopTime { get; set; }

        /// <summary>
        /// NEW — OPS's own block-to-block window (chocks-off to chocks-on),
        /// captured as raw timestamps for crew-duty-time/regulatory logging.
        /// Purely an OPS-side concern — Maintenance's FH/ENGINE_HOURS
        /// accrual must never read these two fields. Kept separate from
        /// DurationMinutes/DayHours/NightHours below, which remain OPS's
        /// existing finalized/computed reporting numbers, unchanged.
        /// </summary>
        public DateTime? BlockOffTime { get; set; }
        public DateTime? BlockOnTime { get; set; }

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
        [Column(TypeName = "decimal(12,2)")]
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
