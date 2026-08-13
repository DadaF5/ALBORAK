// Areas/AircraftMaintenance/Services/SnagCreateDto.cs
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.Services
{
    public class SnagCreateDto
    {
        public int AircraftId { get; set; }
        public int AtaId { get; set; }
        public SnagSeverity Severity { get; set; }
        public AirworthinessImpact Impact { get; set; }
        public ReportedBy ReportedBy { get; set; }
        public DiscoveryPhase DiscoveryPhase { get; set; }
        public int? DiscoveredDuringWorkOrderId { get; set; }
        public int DiscoveryFH { get; set; }
        public int? DiscoveryCycles { get; set; }
        public DateOnly DiscoveryDate { get; set; }
        public int DiscoveryBaseId { get; set; }
        public string Description { get; set; } = null!;
    }

    public class SnagUpdateDto
    {
        public SnagSeverity? Severity { get; set; }
        public AirworthinessImpact? Impact { get; set; }
        public string? Description { get; set; }
    }

    public class SnagDeferralDto
    {
        public string DeferralReference { get; set; } = null!;   // MEL item / T.O. limit para — verbatim
        public int? DeferralLimitFH { get; set; }
        public int? DeferralLimitCycles { get; set; }
        public DateOnly? DeferralLimitDate { get; set; }
    }
}