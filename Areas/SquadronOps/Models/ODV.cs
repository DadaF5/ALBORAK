using FRAProject.Areas.Settings.Models;
using FRAProject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.SquadronOps.Models
{

    public class Odv
    {
        // Primary key (EF convention)
        public int Id { get; set; }

        // Header / relationships
        public int SquadronId { get; set; }
        public Squadron? Squadron { get; set; }

        // NEW: denormalized base FK for fast filtering
        public int? BaseId { get; set; }
        public Base? Base { get; set; } // optional navigation if you have a Base entity

        public int MissionId { get; set; }
        public Mission? Mission { get; set; }

        // date only (configure column type to "date" in DbContext)
        public DateTime OdvDate { get; set; }

        // enum-backed fields (we'll map enums to string columns via value converters in DbContext)
        public Zone Zone { get; set; } = Zone.North;
        public MissionType MissionType { get; set; } = MissionType.Training;

        public string Area { get; set; } = string.Empty;
        public OdvStatus? OdvStatus { get; set; } = Enums.OdvStatus.Planned;

        // Time-of-takeoff (configure column type "time" in DbContext)
        public TimeSpan? TOFF { get; set; }

        public string? Obs { get; set; }

        // FK -> AcMainGroup
        public int AcMainGroupId { get; set; }

        // Navigation - name must match what OdvConfiguration expects
        [ForeignKey(nameof(AcMainGroupId))]
        public virtual AcMainGroup? AcMainGroup { get; set; }

        // call sign / identifier
        [Column("CallSignId")]
        [Required]
        public int CallSignId { get; set; }
        public CallSign? CallSign { get; set; }

        // Preflight approval/validation flag controlled by Squadron
        // When false, CrewChief/TWR actions that change sorties are blocked.
        public bool IsPreflightApproved { get; set; } = false;

        // ════════════════════════════════════════════════════════════════
        // NEW (2026-08-29, Dadda's own instruction) — cancellation reason.
        // Set alongside OdvStatus = OdvStatus.Cancelled (that enum value
        // already existed). Cancelling an Odv cascades this SAME reason
        // text to every related Sortie's own CancellationReason — see
        // OdvPlanningController.Cancel.
        // ════════════════════════════════════════════════════════════════
        [Display(Name = "Cancellation Reason")]
        [StringLength(500)]
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAtUtc { get; set; }

        // optional RowVersion on ODV as well if you plan concurrent edits
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // audit fields (use UTC)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation to sorties
        public virtual ICollection<Sortie>? Sorties { get; set; }
    }
}
