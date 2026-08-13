// Areas/AircraftMaintenance/Models/Snag.cs (revised)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    [Table("Snags", Schema = "dbo")]
    public class Snag
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string SnagNumber { get; set; } = null!;   // AVA-YYYY-NNNN, global sequence

        [Required]
        public int AircraftId { get; set; }
        [ForeignKey(nameof(AircraftId))]
        public virtual Aircraft? Aircraft { get; set; }

        [Required]
        public int AtaId { get; set; }
        [ForeignKey(nameof(AtaId))]
        public virtual Ata? Ata { get; set; }

        [Required]
        public SnagSeverity Severity { get; set; }

        [Required]
        public SnagStatus Status { get; set; } = SnagStatus.OPEN;

        [Required]
        public AirworthinessImpact Impact { get; set; } = AirworthinessImpact.GROUNDING;

        [Required]
        public ReportedBy ReportedBy { get; set; }

        [Required]
        public DiscoveryPhase DiscoveryPhase { get; set; }

        public int? DiscoveredDuringWorkOrderId { get; set; }
        [ForeignKey(nameof(DiscoveredDuringWorkOrderId))]
        public virtual WorkOrder? DiscoveredDuringWorkOrder { get; set; }

        // Position-at-discovery snapshot — immutable
        [Required] public int DiscoveryFH { get; set; }        // minutes
        public int? DiscoveryCycles { get; set; }
        [Required] public DateOnly DiscoveryDate { get; set; }
        [Required] public int DiscoveryBaseId { get; set; }
        [ForeignKey(nameof(DiscoveryBaseId))]
        public virtual Base? DiscoveryBase { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; } = null!;

        // --- Deferral block (EASA M.A.403 / MEL-style authorization) ---
        public bool IsDeferred { get; set; }
        [StringLength(100)]
        public string? DeferralReference { get; set; }   // MEL item / T.O. limit paragraph — verbatim, same rule as JobCardApplicability.Reason
        public string? DeferralAuthorizedByUserId { get; set; }
        public DateTime? DeferralAuthorizedAt { get; set; }
        public int? DeferralLimitFH { get; set; }
        public int? DeferralLimitCycles { get; set; }
        public DateOnly? DeferralLimitDate { get; set; }

        public int? LinkedWorkOrderId { get; set; }
        [ForeignKey(nameof(LinkedWorkOrderId))]
        public virtual WorkOrder? LinkedWorkOrder { get; set; }

        // --- Closure / CRS-equivalent ---
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByUserId { get; set; }        // certifying signature, mirrors CRS

        public virtual ICollection<WorkOrderSnag> WorkOrderSnags { get; set; } = new HashSet<WorkOrderSnag>();
    }
}