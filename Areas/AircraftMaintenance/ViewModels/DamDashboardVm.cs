namespace FRAProject.Areas.Settings.ViewModels
{
    // ════════════════════════════════════════════════════════════════
    //  KPI STRIP — 4 info boxes
    // ════════════════════════════════════════════════════════════════
    public class DamKpiVm
    {
        public int TotalAircraft     { get; set; }
        public int TotalNavigable    { get; set; }   // OPR status
        public int TotalDueSoon      { get; set; }   // within 7 days
        public int TotalRestrictions { get; set; }   // active restrictions
    }

    // ════════════════════════════════════════════════════════════════
    //  CERTIFICATE — one row per certificate type per aircraft
    //  Stubbed until AircraftCertificate table is built.
    // ════════════════════════════════════════════════════════════════
    public class AircraftCertificateVm
    {
        public string  CertType      { get; set; } = string.Empty;
        // "CdN" | "CEN" | "PEA" | "LME" | "CDL"

        public string? Reference     { get; set; }
        public DateOnly? ExpiryDate  { get; set; }

        /// <summary>
        /// Days until expiry — negative = already expired.
        /// </summary>
        public int DaysRemaining =>
            ExpiryDate.HasValue
                ? ExpiryDate.Value.DayNumber -
                  DateOnly.FromDateTime(DateTime.Today).DayNumber
                : int.MaxValue;

        public string StatusLabel =>
            DaysRemaining < 0   ? "Expiré"        :
            DaysRemaining <= 30 ? "Expire Bientôt" :
                                  "Valide";

        public string StatusClass =>
            DaysRemaining < 0   ? "cert-expired" :
            DaysRemaining <= 30 ? "cert-warning"  :
                                  "cert-valid";
    }

    // ════════════════════════════════════════════════════════════════
    //  DUE ITEM — one row in the Échéances Proches card
    //  Stubbed until MaintenanceDue table is built.
    // ════════════════════════════════════════════════════════════════
    public class DueSoonVm
    {
        public string  AircraftCode  { get; set; } = string.Empty;
        public string  AcTypeName    { get; set; } = string.Empty;
        public string  TaskName      { get; set; } = string.Empty;

        // FH-based item
        public int?    DueFh         { get; set; }   // threshold in hours
        public int?    CurrentFh     { get; set; }   // current in hours

        // Calendar-based item
        public DateOnly? DueDate     { get; set; }

        public int CompliancePct =>
            DueFh.HasValue && DueFh.Value > 0 && CurrentFh.HasValue
                ? Math.Min(100, CurrentFh.Value * 100 / DueFh.Value)
                : 0;

        public string AlertClass =>
            CompliancePct >= 95 ? "overdue-alert" : "due-alert";
    }

    // ════════════════════════════════════════════════════════════════
    //  RESTRICTION — one row in the Restrictions Critiques card
    //  Stubbed until AircraftRestriction table is built.
    // ════════════════════════════════════════════════════════════════
    public class RestrictionVm
    {
        public string  AircraftCode  { get; set; } = string.Empty;
        public string  AcTypeName    { get; set; } = string.Empty;
        public string  Reference     { get; set; } = string.Empty;
        public string  Description   { get; set; } = string.Empty;
        public DateOnly? ExpiryDate  { get; set; }
        public bool    IsCritical    { get; set; } = true;

        public string AlertClass => IsCritical ? "alert-critical" : "alert-high";
    }

    // ════════════════════════════════════════════════════════════════
    //  FLEET ROW — one row in the État Certificats table
    // ════════════════════════════════════════════════════════════════
    public class FleetStatusRowVm
    {
        // From Aircraft
        public int     AircraftId    { get; set; }
        public string  TailNumber    { get; set; } = string.Empty;
        public string  AcTypeName    { get; set; } = string.Empty;
        public string  StatusCode    { get; set; } = string.Empty;
        public string  StatusLabel   { get; set; } = string.Empty;

        // Counters — displayed as hours (minutes/60)
        //public int     FlightHours   { get; set; }
        public int TotalFlightMinutes { get; set; }
        public int     Cycles        { get; set; }
        public int     Landings      { get; set; }

        // Certificates — null until AircraftCertificate is built
        public AircraftCertificateVm? CdN { get; set; }
        public AircraftCertificateVm? CEN { get; set; }
        public AircraftCertificateVm? PEA { get; set; }
        public string FlightHoursDisplay
        {
            get
            {
                var h = TotalFlightMinutes / 60;
                var m = TotalFlightMinutes % 60;
                return $"{h}:{m:D2}";
            }
        }
        // Computed badge
        public string StatusBadgeClass => StatusCode switch
        {
            "OPR" => "badge-valid",
            "MNT" => "badge-warning",
            "AOG" => "badge-expired",
            "STK" => "badge-inactive",
            "RAD" => "badge-inactive",
            _     => "badge-inactive"
        };

        public string StatusBadgeLabel => StatusCode switch
        {
            "OPR" => "Navigable",
            "MNT" => "Maintenance",
            "AOG" => "AOG",
            "STK" => "Stocké",
            "RAD" => "Radié",
            _     => "—"
        };
    }

    // ════════════════════════════════════════════════════════════════
    //  FULL DASHBOARD VM
    // ════════════════════════════════════════════════════════════════
    public class DamDashboardVm
    {
        public DamKpiVm           Kpi          { get; set; } = new();
        public List<DueSoonVm>    DueSoon      { get; set; } = [];
        public List<RestrictionVm> Restrictions { get; set; } = [];
        public List<FleetStatusRowVm> Fleet    { get; set; } = [];

        // Stub flags — shown in UI when data not yet available
        public bool HasCertificateData  => Fleet.Any(r =>
            r.CdN != null || r.CEN != null || r.PEA != null);
        public bool HasDueData          => DueSoon.Any();
        public bool HasRestrictionData  => Restrictions.Any();
    }
}
