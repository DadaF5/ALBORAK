// Services/IUserScopeService.cs
using System.Security.Claims;

namespace FRAProject.Services
{
    public class UserScope
    {
        public bool IsUnrestricted { get; set; } // Admin or IsBaseAdmin-anywhere with no group filter
        public List<int> AllowedBaseIds { get; set; } = [];
        public List<int> AllowedAcMainGroupIds { get; set; } = []; // empty = no group restriction within allowed bases
        public List<int> AllowedWingIds { get; set; } = []; // empty = no wing restriction within allowed bases — only meaningful for modules whose roles set ShowWingScope=true (e.g. SquadronOps Pilot/Instructor/Scheduler)
    }

    public interface IUserScopeService
    {
        Task<UserScope> GetScopeAsync(ClaimsPrincipal user, string moduleCode);
    }
}
