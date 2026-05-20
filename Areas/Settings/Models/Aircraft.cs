
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    public enum AircraftStatus
    {
        Available = 0,
        Assigned = 10,
        Airborne = 20,
        Unserviceable = 30
    }

    public class Aircraft
    {
        // ════════════════════════════════════════════════════════════════
        //  ORIGINAL FIELDS — preserved exactly
        // ════════════════════════════════════════════════════════════════

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        // ── Status ───────────────────────────────────────────────────────────
        /// <summary>
        /// LEGACY — kept during refactor, synced by TR_Aircrafts_SyncActive.
        /// Phase out: replace .Active references with .IsActive then drop column.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// SOURCE OF TRUTH — use in all new code.
        /// DB trigger keeps Active in sync automatically.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public bool Serviceable { get; set; } = true;

        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }

        public int AcStatusTypeId { get; set; }
        public AcStatusType? AcStatusType { get; set; }

        public AircraftStatus Status { get; set; } = AircraftStatus.Available;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<Sortie> Sorties { get; set; } = [];
        public ICollection<MaintenanceComponent>? Components { get; set; }
        public ICollection<FlightLog>? FlightLogs { get; set; }
        public ICollection<AircraftDocument> Documents { get; set; } = [];

        // ════════════════════════════════════════════════════════════════
        //  ADDED FIELDS — additive migration only
        // ════════════════════════════════════════════════════════════════

        public int? AircraftVersionId { get; set; }
        public int? ManufacturerId { get; set; }
        public int? BaseId { get; set; }
        public int? MissionRoleId { get; set; }
        public int? OriginCountryId { get; set; }

        public DateOnly? ServiceEntryDate { get; set; }
        public DateOnly? RegistrationDate { get; set; }

        public int? DossierId { get; set; }

        public int TotalFlightMinutes { get; set; } = 0;
        public int TotalCycles { get; set; } = 0;
        public int TotalLandings { get; set; } = 0;

        public byte SortOrder { get; set; } = 99;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // ── Navigation (added) ────────────────────────────────────────────
        public AircraftVersion? AircraftVersion { get; set; }
        public AircraftManufacturer? AircraftManufacturerNav { get; set; }
        public Base? Base { get; set; }
        public MissionRole? MissionRole { get; set; }
        public Country? OriginCountry { get; set; }
        public ImmatriculationDossier? Dossier { get; set; }

        // ── Computed aliases [NotMapped] ─────────────────────────────────────
        // ⚠ DO NOT use in LINQ Where/OrderBy — EF cannot translate [NotMapped].
        // Use real field names (IsActive, TailNo, Obs) in queries.
        [NotMapped] public string? Description { get => Obs; set => Obs = value; }
        [NotMapped] public int TailNumber { get => TailNo; set => TailNo = value; }

        [NotMapped]
        public string DisplayName => $"{Registration} ({TailNo})";

        /// <summary>
        /// HH:MM — no N0 format to avoid thousands separator.
        /// 75090 minutes → "1251:30" not "1,251:30"
        /// </summary>
        [NotMapped]
        public string FlightHoursDisplay
        {
            get
            {
                var h = TotalFlightMinutes / 60;
                var m = TotalFlightMinutes % 60;
                return $"{h}:{m:D2}";
            }
        }

        [NotMapped]
        public string StatusBadgeClass => AcStatusType?.Code switch
        {
            "OPR" => "bg-success",
            "MNT" => "bg-warning text-dark",
            "AOG" => "bg-danger",
            "STK" => "bg-secondary",
            "RAD" => "bg-dark border border-secondary",
            _ => "bg-secondary"
        };

        [NotMapped]
        public string ShortLabel =>
            $"{Registration} · {AcType?.Code ?? "?"} · {TailNo}";
    }
}