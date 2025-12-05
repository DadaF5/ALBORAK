using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FRAProject.Infrastructure.Authorization
{
    public class SameSquadronRequirement : IAuthorizationRequirement { }
    public class SameBaseRequirement : IAuthorizationRequirement { }

    public class SameSquadronHandler : AuthorizationHandler<SameSquadronRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameSquadronRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                if (mvcContext.RouteData.Values.TryGetValue("squadronId", out var routeVal) && routeVal != null)
                {
                    var routeSq = routeVal.ToString();
                    var claim = context.User.FindFirst("SquadronId")?.Value;
                    if (!string.IsNullOrEmpty(claim) && claim == routeSq)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    public class SameBaseHandler : AuthorizationHandler<SameBaseRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameBaseRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                if (mvcContext.RouteData.Values.TryGetValue("baseId", out var routeVal) && routeVal != null)
                {
                    var routeBase = routeVal.ToString();
                    var claim = context.User.FindFirst("BaseId")?.Value;
                    if (!string.IsNullOrEmpty(claim) && claim == routeBase)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
