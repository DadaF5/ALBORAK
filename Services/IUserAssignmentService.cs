// Services/IUserAssignmentService.cs
namespace FRAProject.Services
{
    public interface IUserAssignmentService
    {
        Task<(bool Success, string Message, int? AssignmentId)> GrantAsync(UserAssignmentGrantDto dto, string grantedByUserId);
        Task<(bool Success, string Message)> RevokeAsync(int assignmentId, string revokedByUserId, string? reason);

        // Revoke-and-recreate in one call — the Wing-career-move pattern
        // from the Phase 1 handoff, generalized to any assignment change.
        Task<(bool Success, string Message, int? NewAssignmentId)> ChangeAssignmentAsync(
            int oldAssignmentId, UserAssignmentGrantDto newAssignment, string changedByUserId, string? reason);
    }
}