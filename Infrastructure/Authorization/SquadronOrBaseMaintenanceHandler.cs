using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FRAProject.Infrastructure.Authorization
{
    public class SquadronOrBaseMaintenanceRequirement : IAuthorizationRequirement { }

    public class SquadronOrBaseMaintenanceHandler : AuthorizationHandler<SquadronOrBaseMaintenanceRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SquadronOrBaseMaintenanceRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // route-based checks (fallback)
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                if (mvcContext.RouteData.Values.TryGetValue("squadronId", out var sq) && sq != null)
                {
                    var routeSq = sq.ToString();
                    var claimSq = context.User.FindFirst("SquadronId")?.Value;
                    if (!string.IsNullOrEmpty(claimSq) && claimSq == routeSq)
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }

                if (mvcContext.RouteData.Values.TryGetValue("baseId", out var b) && b != null)
                {
                    var routeBase = b.ToString();
                    var claimBase = context.User.FindFirst("BaseId")?.Value;
                    if (!string.IsNullOrEmpty(claimBase) && claimBase == routeBase && context.User.IsInRole("Maintenance"))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }

            // resource-as-entity checks could be added here if controllers pass entities to AuthorizeAsync
            return Task.CompletedTask;
        }
    }
}