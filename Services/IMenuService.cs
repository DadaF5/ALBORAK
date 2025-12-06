
using System.Security.Claims;
using FRAProject.Models;

namespace FRAProject.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuItem>> GetMenuForUserAsync(ClaimsPrincipal user);
        // If you later add caching, you can add InvalidateCacheAsync here.
    }
}