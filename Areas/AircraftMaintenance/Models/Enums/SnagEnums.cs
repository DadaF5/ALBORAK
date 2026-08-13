// Areas/AircraftMaintenance/Models/Enums/SnagEnums.cs
namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public enum SnagSeverity 
    { 
        CRITICAL, 
        HIGH, 
        MEDIUM, 
        LOW 
    }

    public enum SnagStatus 
    {
        OPEN, 
        DEFERRED, 
        IN_PROGRESS, 
        LINKED, 
        CLOSED 
    }

    public enum DiscoveryPhase
    {
        FLIGHT,
        SCHEDULED_INSPECTION,
        DURING_OTHER_WO,
        AD_HOC_GROUND_REPORT
    }

    // Reported-by distinction — mirrors AFTO 781A's own field
    // ("discovered by aircrew or maintenance personnel")
    public enum ReportedBy 
    { 
        AIRCREW, 
        MAINTENANCE 
    }

    // The Red X / Red Dash / Red Diagonal symbol convention —
    // separate axis from SnagStatus: this is the flight-safety
    // impact flag your QA/audit team will expect to see at a glance.
    public enum AirworthinessImpact
    {
        NONE,        // informational, no operational impact
        GROUNDING,   // Red X    — aircraft not airworthy until closed
        RESTRICTED,  // Red Dash — deferred, mission-capable with limits
        IN_WORK      // Red Diagonal — actively being worked
    }
}