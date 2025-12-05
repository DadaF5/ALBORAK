using System;

namespace FRAProject.Models
{
    // Usage-based threshold that triggers a work order when exceeded
    public class MaintenanceThreshold
    {
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public MaintenanceComponent? Component { get; set; }

        // Threshold types: Minutes, Cycles, CalendarDays etc.
        public string ThresholdType { get; set; } = "Minutes"; // "Minutes"|"Cycles"|"Days"

        // Threshold value (e.g. 500 flight minutes)
        public int Value { get; set; }

        // Optional repeat interval (after reaching, add Value again to nextDue)
        public bool Repeatable { get; set; } = true;

        // Last time threshold was evaluated / last created workorder
        public DateTime? LastTriggeredUtc { get; set; }
    }
}