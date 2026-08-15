// Services/UserScopeService.cs
using System.Security.Claims;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Services
{
    public class UserScopeService : IUserScopeService
    {
        private readonly IUnitOfWork _uow;
        public UserScopeService(IUnitOfWork uow) => _uow = uow;

        public async Task<UserScope> GetScopeAsync(ClaimsPrincipal user, string moduleCode)
        {
            if (user.IsInRole("Admin"))
                return new UserScope { IsUnrestricted = true };

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new UserScope(); // no assignments = sees nothing

            var assignments = (await _uow.UserAssignments.GetActiveByUserIdAsync(userId))
                .Where(a => a.IsBaseAdmin || (a.ModuleRole != null && a.ModuleRole.ModuleCode == moduleCode))
                .ToList();

            var scope = new UserScope();

            foreach (var a in assignments)
            {
                scope.AllowedBaseIds.Add(a.BaseId);

                // Base Admin, or a role with ShowGroupScope=false, sees every
                // group within their allowed bases — don't add a group filter.
                if (!a.IsBaseAdmin && a.AcMainGroupId.HasValue)
                    scope.AllowedAcMainGroupIds.Add(a.AcMainGroupId.Value);
            }

            scope.AllowedBaseIds = scope.AllowedBaseIds.Distinct().ToList();
            scope.AllowedAcMainGroupIds = scope.AllowedAcMainGroupIds.Distinct().ToList();

            return scope;
        }
    }
}