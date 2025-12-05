using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using FRAProject.Models;

namespace FRAProject.Infrastructure.Identity
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (user.SquadronId.HasValue)
                identity.AddClaim(new Claim("SquadronId", user.SquadronId.Value.ToString()));

            if (user.BaseId.HasValue)
                identity.AddClaim(new Claim("BaseId", user.BaseId.Value.ToString()));

            if (user.WingId.HasValue)
                identity.AddClaim(new Claim("WingId", user.WingId.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(user.JobTitle))
                identity.AddClaim(new Claim("JobTitle", user.JobTitle));

            // add other claims as needed
            return identity;
        }
    }
}
