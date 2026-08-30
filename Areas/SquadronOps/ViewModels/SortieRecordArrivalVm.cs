using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    // ATC/Tower post-flight step. Only allowed while Sortie.Status ==
    // Airborne; advances it to Landed. See SortiesController.RecordArrival.
    // This is now the ONLY place Landings/TGOsLandings get set — Finalize
    // no longer accepts them from Squadron's SortieFinalizeVm (see
    // Sortie.cs's comment on those two fields).
    //
    // CHANGED (Batch 12, 2026-08-30) — EngineStartTime display field
    // removed. It's no longer set by this point in the workflow —
    // EngineStartTime moved to CrewChief's RecordPostFlight (recorded
    // AFTER arrival, not before) — so showing it here would always be
    // blank. See SortieRecordDepartureVm's own comment for the full
    // reasoning.
    public class SortieRecordArrivalVm
    {
        public int SortieId { get; set; }

        // Display only
        public string? SortieCode { get; set; }
        public string? AcTypeName { get; set; }
        public DateTime? RealTOFF { get; set; }

        [Required(ErrorMessage = "Real landing time is required.")]
        [Display(Name = "Real Landing Time (touchdown)")]
        [DataType(DataType.DateTime)]
        public DateTime? RealLandingTime { get; set; }

        [Display(Name = "Full-Stop Landings")]
        public int? Landings { get; set; }

        [Display(Name = "Touch-and-Go Landings")]
        public int? TGOsLandings { get; set; }
    }
}
