using FRAProject.Areas.AircraftMaintenance.Models;
using System;
using System.Collections.Generic;

namespace FRAProject.Models
{
    // Component or Line-Replaceable Unit (LRU) installed on an aircraft
    public class MaintenanceComponent
    {
        public int Id { get; set; }
        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        // Common identifiers (part no, serial etc.)
        public string PartNumber { get; set; } = "";
        public string SerialNumber { get; set; } = "";

        // Cumulative usage counters (minutes, cycles)
        public int TotalMinutes { get; set; } = 0;
        public int TotalCycles { get; set; } = 0;

        // Last updated
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<MaintenanceThreshold>? Thresholds { get; set; }
    }
}