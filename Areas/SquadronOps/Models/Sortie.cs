
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
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

        // NEW (Batch 11, 2026-08-29) — CrewChief's post-flight report
        // (fuel/oil/snag/other info) has been recorded, per Dadda's
        // confirmed sequence: "...ATC real landing time + airfield
        // activities data...Crewchief post flight data (fuel and oil or
        // snag or any other info)...at squadron => ...Mission(sortie)
        // closed." Deliberately placed between Landed (30) and Finalized
        // (40) — it IS the next real step in the 0/10/20/30/40
        // progression, unlike Canceled below. Finalize now BLOCKS unless
        // Status == CrewChiefReported — see SortiesController.Finalize's
        // own comment for why, and how to relax this if it turns out to
        // be too strict for some sortie types.
        CrewChiefReported = 35,

        Finalized = 40,

        // NEW (2026-08-29, Dadda's own instruction) — a Sortie can be
        // cancelled either individually (its own reason) or as part of a
        // whole-Odv cancellation cascade (same reason as the Odv). This is
        // a divergent TERMINAL state, not the "next" stage after Finalized
        // — do not assume linear ordering (e.g. `status >= Finalized`)
        // still means "done and reportable"; check for Canceled explicitly
        // wherever that kind of comparison is used. Deliberately given a
        // value that does not fit the existing 0/10/20/30/40 progression,
        // as a visual reminder it is not part of that sequence.
        Canceled = 999
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
        //
        //  NEW (Batch 11) — RealTOFF/EngineStartTime are now set together
        //  by SortiesController.RecordDeparture (ATC/Tower action), and
        //  RealLandingTime by RecordArrival (also ATC/Tower) — per Dadda's
        //  confirmed sequence "ATC engine start and TOFF time...ATC real
        //  landing time". Previously these fields existed with no action
        //  writing to them at all.
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

        /// <summary>
        /// NEW (2026-08-29, Dadda's F16 block-off/holding-point example) —
        /// OPS's own block-to-block duration in minutes (BlockOnTime -
        /// BlockOffTime), for crew duty-time/regulatory reporting. Purely
        /// OPS-side, same rule as BlockOffTime/BlockOnTime themselves:
        /// Maintenance NEVER reads this field. Deliberately kept separate
        /// from DurationMinutes below (which is squadron-entered airframe
        /// flight duration and is what Maintenance's FH accrual actually
        /// sums) — the two numbers are legitimately different by design
        /// (ground taxi/holding-point delay is real duty time but not
        /// flight time) and must never be confused for one another. See
        /// the Finalize validation note on DurationMinutes just below.
        /// </summary>
        [Display(Name = "Block Duration (minutes)")]
        public int? BlockDurationMinutes { get; set; }

        public string? Notes { get; set; }

        // Post flight
        [Display(Name = "Day Hours")]
        public double? DayHours { get; set; }

        [Display(Name = "Night Hours")]
        public double? NightHours { get; set; }

        // Computed Duration in hours (safe: handle nulls)
        [Display(Name = "Duration (hours)")]
        public double? DurationHours => (DayHours ?? 0.0) + (NightHours ?? 0.0);

        // ════════════════════════════════════════════════════════════════
        // Persisted duration in minutes (Squadron-finalized), e.g. 1:05 ->
        // 65. Stays MANUALLY ENTERED by Squadron per Dadda's decision
        // (2026-08-29) — NOT auto-computed from RealTOFF/RealLandingTime.
        //
        // This is also the exact field Maintenance's real
        // SortieRepository.GetAccumulatedFHByAcTypeAsync sums for
        // component-life FH accrual per AcType — Dadda also decided NOT to
        // change that method to compute FH independently from
        // RealTOFF/RealLandingTime. Both decisions together mean the
        // safeguard against the block-off/holding-point conflict lives
        // entirely in whatever finalizes a Sortie:
        //
        //   At Finalize time, when RealTOFF and RealLandingTime are both
        //   set, compute expectedMinutes = (RealLandingTime -
        //   RealTOFF).TotalMinutes and compare it to the entered
        //   DurationMinutes. If they differ by more than a small tolerance
        //   (proposed default: 5 minutes — NOT yet confirmed by Dadda),
        //   show a non-blocking warning naming the discrepancy (e.g. "Entered
        //   duration is 30 minutes longer than computed airframe time —
        //   ground delays before TOFF should not be included here; did you
        //   mean to enter block time in Block Duration instead?") rather
        //   than silently accepting it or hard-blocking the save.
        //
        // IMPLEMENTED — see SortiesController.Finalize (Batch 6 onward).
        // ════════════════════════════════════════════════════════════════
        public int? DurationMinutes { get; set; }

        [Display(Name = "Approachs")]
        public int? Approachs { get; set; }

        // ════════════════════════════════════════════════════════════════
        // Landings / TGOsLandings — NEW OWNERSHIP (Batch 11, 2026-08-29).
        // Per Dadda's corrected sequence, these are ATC-recorded post-
        // flight "airfield activities data", NOT Squadron-entered at
        // Finalize as Batch 8 originally assumed. Now set by
        // SortiesController.RecordArrival (ATC/Tower action). Finalize no
        // longer writes these two fields from SortieFinalizeVm — it only
        // reads whatever ATC already recorded here. SortieFinalizeVm
        // itself (a real, pre-existing file) still HAS Landings/
        // TGOsLandings properties; Finalize simply ignores them now. If
        // any real view still posts values into those VM properties,
        // remove/hide those two inputs there — not something this batch
        // can fix without seeing that view.
        // ════════════════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════════════════
        // NEW OWNERSHIP (Batch 11) — FuelUsedLiters is now CrewChief's
        // post-flight entry (SortiesController.RecordPostFlight), per
        // Dadda's sequence: "Crewchief post flight data (fuel and oil...)".
        // Previously existed on the model with nothing setting it.
        //
        // KNOWN UNIT MISMATCH, flagged not silently converted: this field
        // is Liters; FlightLog.FuelUsedKg (set at Finalize) is Kg. Finalize
        // does NOT convert between them (no confirmed density/conversion
        // factor) — FlightLog.FuelUsedKg is left unset. Needs a decision:
        // either add FlightLog.FuelUsedLiters instead of reusing
        // FuelUsedKg, or supply a real conversion factor for the aircraft
        // type's fuel.
        // ════════════════════════════════════════════════════════════════
        [Display(Name = "Fuel Used (Liters)")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal? FuelUsedLiters { get; set; }

        /// <summary>
        /// NEW (Batch 11) — CrewChief's post-flight oil entry, matching
        /// FuelUsedLiters's staging convention: set by RecordPostFlight,
        /// copied into FlightLog.OilUsedLiters (new field, same batch) at
        /// Finalize. Same units end to end — no conversion needed, unlike
        /// the fuel Liters/Kg mismatch above.
        /// </summary>
        [Display(Name = "Oil Used (Liters)")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PostFlightOilUsedLiters { get; set; }

        /// <summary>
        /// NEW (Batch 11) — CrewChief's post-flight "any other info" free
        /// text, per Dadda's sequence. Copied into FlightLog.Notes at
        /// Finalize. Deliberately separate from the pre-existing Malfunctions
        /// field below (which is unused elsewhere and left alone) and from
        /// SquadronReportNotes (Squadron's own, separate note field).
        /// </summary>
        [Display(Name = "CrewChief Post-Flight Notes")]
        [StringLength(1000)]
        public string? PostFlightNotes { get; set; }

        public DateTime? PostFlightReportedAtUtc { get; set; }
        public string? PostFlightReportedBy { get; set; }

        /// <summary>
        /// NEW (Batch 11) — set when CrewChief's post-flight report
        /// includes a snag. Links to the REAL, existing AircraftMaintenance
        /// Snag record created via ISnagService.ReportAsync — per Dadda's
        /// confirmed decision to reuse that system rather than build a
        /// separate SquadronOps-only snag field. One snag per Sortie's
        /// post-flight report today (matches how RecordPostFlight is
        /// written this batch); a sortie needing multiple snags reported
        /// is out of scope for now — flagged, not silently limited without
        /// a comment.
        /// </summary>
        public int? SnagId { get; set; }
        public Snag? Snag { get; set; }

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

        // ════════════════════════════════════════════════════════════════
        // NEW (2026-08-29) — cancellation. Set when Status == Canceled,
        // either from an individual per-Sortie cancel (SortiesController)
        // or cascaded from the parent Odv's cancellation
        // (OdvPlanningController) — in the cascade case this carries the
        // SAME reason text as the Odv's own CancellationReason, per
        // Dadda's instruction. Mirrors Sortie's existing
        // Completed*/Finalized* audit-pair convention.
        // ════════════════════════════════════════════════════════════════
        [Display(Name = "Cancellation Reason")]
        [StringLength(500)]
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string? CancelledBy { get; set; }

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
