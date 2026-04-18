using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public enum AircraftStatus
    {
        Available = 0,
        Assigned = 10,      // assigned to a sortie but still on ground
        Airborne = 20,      // airborne (TWR should release back to Available)
        Unserviceable = 30  // aircraft declared unserviceable / maintenance
    }

    /// <summary>
    /// DOMAIN: Aircraft Maintenance
    /// Represents an aircraft in the fleet inventory.
    /// Educational Purpose: Central entity linking maintenance tracking to flight operations.
    /// 
    /// Key Relationships:
    /// - Aircraft → AcType (Many-to-One): Aircraft model/type (F-16, C-130, etc.)
    /// - Aircraft → AcStatusType (Many-to-One): Operational status (Mission Capable, etc.)
    /// - Aircraft → MaintenanceComponents (One-to-Many): Trackable aircraft components
    /// - Aircraft → MaintenanceWorkOrders (One-to-Many): Maintenance actions and repairs
    /// - Aircraft → Sorties (One-to-Many): Flight missions this aircraft has flown
    /// - Aircraft → FlightLogs (One-to-Many): Flight hour/cycle tracking for maintenance
    /// 
    /// The Serviceable flag and AircraftStatus determine if aircraft can be assigned to sorties.
    /// This demonstrates how maintenance domain directly impacts squadron operations scheduling.
    /// </summary>
    public class Aircraft
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // --------------------------
        // Basic Properties
        // --------------------------

        [Required]
        public int TailNo { get; set; }

        [Required, StringLength(50)]
        public string Registration { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ManufactureDate { get; set; }

        [StringLength(10)]
        public string? IntCode { get; set; }

        public string? Obs { get; set; }

        // --------------------------
        // Status
        // --------------------------
        public bool Active { get; set; } = true;
        public bool Serviceable { get; set; } = true;

        // --------------------------
        // Foreign Keys + Navigation
        // --------------------------
        public int AcTypeId { get; set; }
        public AcType AcType { get; set; }

        public int AcStatusTypeId { get; set; }
        public AcStatusType AcStatusType { get; set; } = default!;

        // --------------------------
        // Convenience / Display
        // --------------------------
        [NotMapped]
        public string DisplayName => $"{Registration} ({TailNo})";

        public AircraftStatus Status { get; set; } = AircraftStatus.Available;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
        // --------------------------
        // collection navigation properties
        public ICollection<Sortie> Sorties { get; set; } = new List<Sortie>();
        public ICollection<MaintenanceComponent>? Components { get; set; }
        public ICollection<FlightLog>? FlightLogs { get; set; }

        // Documents related to this aircraft (e.g. maintenance records, certifications)
        public ICollection<AircraftDocument> Documents { get; set; } = new HashSet<AircraftDocument>();
    }

}
