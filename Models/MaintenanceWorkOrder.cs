using FRAProject.Areas.AircraftMaintenance.Models;
using System;

namespace FRAProject.Models
{
    public class MaintenanceWorkOrder
    {
        public int Id { get; set; }
        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        // Optionally link to a component and the threshold that triggered it
        public int? ComponentId { get; set; }
        public MaintenanceComponent? Component { get; set; }
        public int? ThresholdId { get; set; }
        public MaintenanceThreshold? Threshold { get; set; }

        public string Title { get; set; } = "";
        public string? Description { get; set; }

        // Status: Open / InProgress / Closed
        public string Status { get; set; } = "Open";

        // Snapshot of usage that caused the WO (for traceability)
        public int TriggeredTotalMinutes { get; set; }
        public int TriggeredTotalCycles { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}