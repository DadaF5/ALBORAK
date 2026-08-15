// Services/UserAssignmentService.cs
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;

namespace FRAProject.Services
{
    public class UserAssignmentService : IUserAssignmentService
    {
        private readonly IUnitOfWork _uow;
        public UserAssignmentService(IUnitOfWork uow) => _uow = uow;

        public async Task<(bool, string, int?)> GrantAsync(UserAssignmentGrantDto dto, string grantedByUserId)
        {
            // ── Guard 1: IsBaseAdmin and ModuleRoleId are mutually exclusive ──
            if (dto.IsBaseAdmin && dto.ModuleRoleId.HasValue)
                return (false, "Un Base Admin ne peut pas avoir un rôle de module spécifique — accès transversal uniquement.", null);

            if (!dto.IsBaseAdmin && !dto.ModuleRoleId.HasValue)
                return (false, "Un rôle de module est requis, sauf pour un Base Admin.", null);

            // ── Guard 2: AcMainGroup.BaseId must match the assignment's BaseId ──
            if (dto.AcMainGroupId.HasValue)
            {
                var group = await _uow.AcMainGroups.GetByIdAsync(dto.AcMainGroupId.Value);
                if (group == null)
                    return (false, "Groupe principal introuvable.", null);

                if (group.BaseId != dto.BaseId)
                    return (false, $"Le groupe sélectionné appartient à une autre base (incohérence Base/AcMainGroup).", null);
            }

            // ── Guard 3: if a ModuleRole is set, respect its scope flags ──
            if (dto.ModuleRoleId.HasValue)
            {
                var role = await _uow.ModuleRoles.GetByIdAsync(dto.ModuleRoleId.Value);
                if (role == null)
                    return (false, "Rôle de module introuvable.", null);

                if (!role.ShowGroupScope && dto.AcMainGroupId.HasValue)
                    return (false, $"Le rôle '{role.RoleName}' ne peut pas être restreint par groupe d'aéronefs.", null);

                if (!role.ShowWingScope && dto.WingId.HasValue)
                    return (false, $"Le rôle '{role.RoleName}' ne peut pas être restreint par escadre.", null);
            }

            var assignment = new UserAssignment
            {
                UserId = dto.UserId,
                ModuleRoleId = dto.ModuleRoleId,
                BaseId = dto.BaseId,
                IsBaseAdmin = dto.IsBaseAdmin,
                AcMainGroupId = dto.AcMainGroupId,
                WingId = dto.WingId,
                IsActive = true,
                GrantedAtUtc = DateTime.UtcNow,
                GrantedByUserId = grantedByUserId
            };

            await _uow.UserAssignments.AddAsync(assignment);
            await _uow.CompleteAsync();

            return (true, "Affectation créée.", assignment.Id);
        }

        public async Task<(bool, string)> RevokeAsync(int assignmentId, string revokedByUserId, string? reason)
        {
            var assignment = await _uow.UserAssignments.GetByIdAsync(assignmentId);
            if (assignment == null) return (false, "Affectation introuvable.");
            if (!assignment.IsActive) return (false, "Affectation déjà révoquée.");

            assignment.IsActive = false;
            assignment.RevokedAtUtc = DateTime.UtcNow;
            assignment.RevokedByUserId = revokedByUserId;
            assignment.RevokeReason = reason;

            _uow.UserAssignments.Update(assignment);
            await _uow.CompleteAsync();

            return (true, "Affectation révoquée.");
        }

        public async Task<(bool, string, int?)> ChangeAssignmentAsync(
            int oldAssignmentId, UserAssignmentGrantDto newAssignment, string changedByUserId, string? reason)
        {
            var revokeResult = await RevokeAsync(oldAssignmentId, changedByUserId, reason ?? "Modification d'affectation");
            if (!revokeResult.Item1) return (false, revokeResult.Item2, null);

            var grantResult = await GrantAsync(newAssignment, changedByUserId);
            return grantResult;
        }
    }
}