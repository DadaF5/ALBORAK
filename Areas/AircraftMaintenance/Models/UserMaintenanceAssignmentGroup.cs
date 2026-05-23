using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Tracks temporary additional AcMainGroup scopes for a user's maintenance assignment.
    ///
    /// Use case: A Base Supervisor temporarily covers a second aircraft group
    /// while its permanent supervisor is away. This row grants extra read/write scope
    /// without changing the permanent assignment record.
    ///
    /// Active additional-group resolution:
    ///   EffectiveTo IS NULL  OR  EffectiveTo >= today  =>  currently active
    /// </summary>
    [Table("UserMaintenanceAssignmentGroups", Schema = "dbo")]
    public class UserMaintenanceAssignmentGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // =========================================
        // Parent assignment
        // =========================================
        [Required]
        public int AssignmentId { get; set; }
        public UserMaintenanceAssignment Assignment { get; set; } = default!;

        // =========================================
        // Extra group granted
        // =========================================
        [Required]
        public int AcMainGroupId { get; set; }
        public AcMainGroup AcMainGroup { get; set; } = default!;

        // =========================================
        // Validity window for this extra scope
        // =========================================
        [Required]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>NULL means "no end date" (open-ended temporary scope).</summary>
        public DateTime? EffectiveTo { get; set; }

        // =========================================
        // Reason / reference (e.g. temporary duty order #)
        // =========================================
        [StringLength(300)]
        public string? Reason { get; set; }

        // =========================================
        // Audit
        // =========================================
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // =========================================
        // Computed helper (not mapped)
        // =========================================
        [NotMapped]
        public bool IsCurrentlyActive =>
            (EffectiveTo == null || EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
    }
}
