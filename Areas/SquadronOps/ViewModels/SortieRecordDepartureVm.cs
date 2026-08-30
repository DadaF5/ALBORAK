using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    // ATC/Tower pre-flight step. Per Dadda's sequence, ATC's real
    // responsibility is TOFF (the one ground/airborne transition tower can
    // actually attest to via radar/visual/radio). Only allowed while
    // Sortie.Status == AircraftAssigned; advances it to Airborne. See
    // SortiesController.RecordDeparture.
    //
    // CHANGED (Batch 12, 2026-08-30) — EngineStartTime REMOVED from this VM.
    // Per Dadda's question on who feeds Block-Off/Block-On/Engine Start/Stop
    // in real practice: ATC has no reliable way to observe engine ignition
    // (it happens on the ramp, often before the aircraft is even on
    // frequency) — that's ground-crew territory in virtually every air
    // force, same as chocks-off/chocks-on. EngineStartTime moved to
    // CrewChief's RecordPostFlight (see SortieRecordPostFlightVm), recorded
    // retrospectively alongside EngineStopTime/BlockOffTime/BlockOnTime.
    public class SortieRecordDepartureVm
    {
        public int SortieId { get; set; }

        // Display only
        public string? SortieCode { get; set; }
        public string? AcTypeName { get; set; }

        [Required(ErrorMessage = "Take-off time (TOFF) is required.")]
        [Display(Name = "Real TOFF (wheels-up)")]
        [DataType(DataType.DateTime)]
        public DateTime? RealTOFF { get; set; }
    }
}
