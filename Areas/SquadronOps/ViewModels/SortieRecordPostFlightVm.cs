using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    // CrewChief's post-flight step. Per Dadda's sequence: "Crewchief post
    // flight data (fuel and oil or snag or any other info)". Only allowed
    // while Sortie.Status == Landed; advances it to CrewChiefReported, which
    // is what unblocks Squadron's Finalize. See
    // SortiesController.RecordPostFlight.
    //
    // CHANGED (Batch 12, 2026-08-30) — added BlockOffTime/BlockOnTime/
    // EngineStartTime/EngineStopTime, all optional/free-entry ("keep their
    // text field", per Dadda). Consolidated here rather than split across a
    // new CrewChief pre-flight touchpoint, matching how a real ground crew
    // reconciles their kneeboard/paper log into one report after the
    // mission rather than in real time. Per Dadda's question on who
    // actually feeds these values: ground crew is the one physically
    // present for chocks-off/chocks-on and engine start/stop in virtually
    // every air force's practice (cross-checked against the real USAF AFTO
    // 781 form's own division of labor) — ATC/tower has no reliable way to
    // observe these ramp-side events. EngineStartTime moved here FROM
    // SortieRecordDepartureVm (ATC keeps only RealTOFF now).
    //
    // Snag fields are optional — ReportSnag toggles whether the rest of
    // the snag block is required. When submitted, these map straight onto
    // AircraftMaintenance's real SnagCreateDto/ISnagService.ReportAsync
    // rather than a SquadronOps-only field, per Dadda's confirmed decision
    // to reuse the existing Snag system.
    public class SortieRecordPostFlightVm
    {
        public int SortieId { get; set; }

        // Display only
        public string? SortieCode { get; set; }
        public string? AcTypeName { get; set; }
        public DateTime? RealTOFF { get; set; }
        public DateTime? RealLandingTime { get; set; }

        // ── Ground times (NEW, Batch 12) — all optional, free entry ──
        [Display(Name = "Block-Off (chocks away)")]
        [DataType(DataType.DateTime)]
        public DateTime? BlockOffTime { get; set; }

        [Display(Name = "Engine Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime? EngineStartTime { get; set; }

        [Display(Name = "Engine Stop Time")]
        [DataType(DataType.DateTime)]
        public DateTime? EngineStopTime { get; set; }

        [Display(Name = "Block-On (chocks set)")]
        [DataType(DataType.DateTime)]
        public DateTime? BlockOnTime { get; set; }

        [Display(Name = "Fuel Used (Liters)")]
        [Range(0, 100000)]
        public decimal? FuelUsedLiters { get; set; }

        [Display(Name = "Oil Used (Liters)")]
        [Range(0, 1000)]
        public decimal? OilUsedLiters { get; set; }

        [Display(Name = "Other Info / Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        // ── Snag (optional) ──
        [Display(Name = "Report a snag for this aircraft")]
        public bool ReportSnag { get; set; }

        [Display(Name = "ATA Chapter")]
        public int? AtaId { get; set; }

        public SnagSeverity? Severity { get; set; }
        public AirworthinessImpact? Impact { get; set; }

        // Per Dadda: "snag can be reported by Aircrew or Maintenance as in
        // Form 781 USAF doc" — CrewChief picks which on the form, no
        // silent default.
        [Display(Name = "Reported By")]
        public ReportedBy? ReportedBy { get; set; }

        [Display(Name = "Snag Description")]
        [StringLength(2000)]
        public string? SnagDescription { get; set; }

        // Populated by the controller for the ATA dropdown.
        public List<SelectListItem>? AtaOptions { get; set; }
    }
}
