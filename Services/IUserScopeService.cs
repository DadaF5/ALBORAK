// Services/IUserScopeService.cs
using System.Security.Claims;

namespace FRAProject.Services
{
    public class UserScope
    {
        public bool IsUnrestricted { get; set; } // Admin or IsBaseAdmin-anywhere with no group filter
        public List<int> AllowedBaseIds { get; set; } = [];
        public List<int> AllowedAcMainGroupIds { get; set; } = []; // empty = no group restriction within allowed bases
    }

    public interface IUserScopeService
    {
        Task<UserScope> GetScopeAsync(ClaimsPrincipal user, string moduleCode);
    }
}