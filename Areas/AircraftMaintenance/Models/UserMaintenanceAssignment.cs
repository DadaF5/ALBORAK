using FRAProject.Areas.HR.Models;
using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Historical record of a user's maintenance assignment.
    ///
    /// Design notes:
    /// - A user may have many assignments over time (moves between bases, groups, roles).
    /// - Only one assignment per user should be active at any point in time
    ///   (enforced at the application layer via IsActive flag and EffectiveTo = null check).
    /// - To reassign a user: set EffectiveTo = today on the current assignment, create a new one.
    /// - AdditionalGroups: temporary extra AcMainGroup scopes (see UserMaintenanceAssignmentGroup).
    ///
    /// Active assignment resolution:
    ///   IsActive = true  AND  EffectiveTo IS NULL  =>  currently active
    ///   IsActive = true  AND  EffectiveTo >= today =>  still in range (active)
    ///   IsActive = false OR   EffectiveTo &lt; today  =>  historical
    /// </summary>
    [Table("UserMaintenanceAssignments", Schema = "dbo")]
    public class UserMaintenanceAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // =========================================
        // User reference (Identity user)
        // =========================================
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;

        // =========================================
        // Organizational scope at time of assignment
        // =========================================
        [Required]
        public int BaseId { get; set; }
        public Base Base { get; set; } = default!;

        [Required]
        public int AcMainGroupId { get; set; }
        public AcMainGroup AcMainGroup { get; set; } = default!;

        // =========================================
        // Role at time of assignment
        // =========================================
        [Required]
        public int MaintenanceRoleId { get; set; }
        public MaintenanceRole MaintenanceRole { get; set; } = default!;

        // =========================================
        // Validity window
        // =========================================
        [Required]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// NULL means "no scheduled end" (currently active).
        /// Set to a past date or use IsActive = false to deactivate.
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================================
        // Optional notes (reason for assignment, transfer order #, etc.)
        // =========================================
        [StringLength(500)]
        public string? Notes { get; set; }

        // =========================================
        // Audit
        // =========================================
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        // =========================================
        // Additional temporary group scopes
        // =========================================
        public ICollection<UserMaintenanceAssignmentGroup> AdditionalGroups { get; set; } = new HashSet<UserMaintenanceAssignmentGroup>();

        // =========================================
        // Computed helpers (not mapped)
        // =========================================
        [NotMapped]
        public bool IsCurrentlyActive =>
            IsActive &&
            (EffectiveTo == null || EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
    }
}
