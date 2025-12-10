using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public enum AircraftStatus
    {
        Available = 0,
        Assigned = 10,      // assigned to a sortie but still on ground
        Airborne = 20,      // airborne (TWR should release back to Available)
        Unserviceable = 30  // aircraft declared unserviceable / maintenance
    }
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

        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; }
        // --------------------------
        // collection navigation properties
        public ICollection<Sortie>? Sorties { get; set; }
        public ICollection<MaintenanceComponent>? Components { get; set; }
        public ICollection<FlightLog>? FlightLogs { get; set; }
        
    }

}
