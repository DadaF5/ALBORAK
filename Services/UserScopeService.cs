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

                // Base Admin, or a role with ShowGroupScope=false / ShowWingScope=false,
                // sees every group/wing within their allowed bases — don't add a filter
                // for that dimension in that case.
                if (!a.IsBaseAdmin && a.AcMainGroupId.HasValue)
                    scope.AllowedAcMainGroupIds.Add(a.AcMainGroupId.Value);

                if (!a.IsBaseAdmin && a.WingId.HasValue)
                    scope.AllowedWingIds.Add(a.WingId.Value);
            }

            scope.AllowedBaseIds = scope.AllowedBaseIds.Distinct().ToList();
            scope.AllowedAcMainGroupIds = scope.AllowedAcMainGroupIds.Distinct().ToList();
            scope.AllowedWingIds = scope.AllowedWingIds.Distinct().ToList();

            return scope;
        }
    }
}
