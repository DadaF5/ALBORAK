// Models/UserAssignment.cs — same namespace as Module/ModuleRole
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;

namespace FRAProject.Models
{
    [Table("UserAssignments", Schema = "dbo")]
    public class UserAssignment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }

        // NULL only when IsBaseAdmin = true — Base Admin needs no single
        // ModuleRole since it grants cross-module access.
        public int? ModuleRoleId { get; set; }
        public ModuleRole? ModuleRole { get; set; }

        [Required]
        public int BaseId { get; set; }
        public Base? Base { get; set; }

        public bool IsBaseAdmin { get; set; } = false;

        // NULL = unrestricted within the module/base (e.g. Maintenance BGNT).
        // Set = scoped to that group (e.g. F5 Tech). NOTE: the chosen
        // AcMainGroup already carries its own BaseId — service layer must
        // validate it matches this assignment's BaseId.
        public int? AcMainGroupId { get; set; }
        public AcMainGroup? AcMainGroup { get; set; }

        public int? WingId { get; set; }
        public Wing? Wing { get; set; }

        // Lifecycle — never delete, always revoke + create new
        // (same pattern as the Wing career-move example in the Phase 1 handoff)
        public bool IsActive { get; set; } = true;
        public DateTime GrantedAtUtc { get; set; } = DateTime.UtcNow;
        public string? GrantedByUserId { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByUserId { get; set; }
        public string? RevokeReason { get; set; }
    }
}