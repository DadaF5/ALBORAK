namespace FRAProject.ViewModels
{
    // Squadron final report VM - duration is in minutes (e.g. 65)
    public class SortieFinalizeVm
    {
        public int SortieId { get; set; }

        // Duration in minutes (e.g. 65)
        public int? DurationMinutes { get; set; }

        // If you need to allow Squad to enter day/night split, add these:
        public int? DayMinutes { get; set; }   // optional
        public int? NightMinutes { get; set; } // optional
        public int? Landings { get; set; }
        public int? Approachs { get; set; }
        public int? TGOsLandings { get; set; }
        public double? InstSimulated { get; set; }
        public double? InstActual { get; set; }
        public double? IFRHours { get; set; }

        public int? Interceptions { get; set; }
        public int? RadarContacts { get; set; }
        public int? AppContacts { get; set; }
        public string? SquadronReportNotes { get; set; }
        public DateTime? FinalizedAtUtc { get; set; }
        public string? FinalizedBy { get; set; } = string.Empty;
    }
}
