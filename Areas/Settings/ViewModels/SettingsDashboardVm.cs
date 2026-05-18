namespace FRAProject.Areas.Settings.ViewModels
{
    /// <summary>
    /// One card on the Settings dashboard.
    /// Represents a single lookup table's health stats.
    /// </summary>
    public class LookupTableStatVm
    {
        public string Title       { get; set; } = string.Empty;
        public string Controller  { get; set; } = string.Empty;   // for nav links
        public string Icon        { get; set; } = string.Empty;   // FA icon class
        public string Section     { get; set; } = string.Empty;   // sidebar section
        public int    TotalCount  { get; set; }
        public int    ActiveCount { get; set; }
        public int    InactiveCount => TotalCount - ActiveCount;

        /// <summary>Percent of active rows — used for progress bar width.</summary>
        public int ActivePercent => TotalCount == 0
            ? 0
            : (int)Math.Round(ActiveCount * 100.0 / TotalCount);

        /// <summary>Bootstrap color class based on active ratio.</summary>
        public string HealthClass => ActivePercent switch
        {
            100          => "success",
            >= 80        => "warning",
            >= 50        => "orange",
            _            => "danger"
        };
    }

    /// <summary>
    /// Full Settings dashboard ViewModel.
    /// </summary>
    public class SettingsDashboardVm
    {
        // ── Grouped by sidebar section ────────────────────────────────────

        /// <summary>DONNÉES DE BASE — aircraft hierarchy</summary>
        public List<LookupTableStatVm> DonneesDeBase { get; set; } = [];

        /// <summary>RÉFÉRENTIELS — regulatory lookups</summary>
        public List<LookupTableStatVm> Referentiels { get; set; } = [];

        /// <summary>IMMATRICULATION — dossier lookups</summary>
        public List<LookupTableStatVm> Immatriculation { get; set; } = [];

        // ── Grand totals ──────────────────────────────────────────────────
        public int TotalTables  => DonneesDeBase.Count +
                                   Referentiels.Count  +
                                   Immatriculation.Count;

        public int TotalRecords => DonneesDeBase.Sum(x => x.TotalCount) +
                                   Referentiels.Sum(x => x.TotalCount)  +
                                   Immatriculation.Sum(x => x.TotalCount);

        public int TotalActive  => DonneesDeBase.Sum(x => x.ActiveCount) +
                                   Referentiels.Sum(x => x.ActiveCount)  +
                                   Immatriculation.Sum(x => x.ActiveCount);

        public int TotalInactive => TotalRecords - TotalActive;
    }
}
