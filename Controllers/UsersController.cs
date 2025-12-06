using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FRAProject.Models;
using FRAProject.ViewModels;
using FRAProject.Data;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly FRAContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            FRAContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            // load users (no tracking)
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var vmList = new List<UserListViewModel>(users.Count);

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                string rolesCsv = roles.Any() ? string.Join(", ", roles) : string.Empty;

                string? baseName = null;
                if (u.BaseId.HasValue)
                {
                    var b = await _context.Set<Base>().FindAsync(u.BaseId.Value);
                    baseName = b?.BaseName;
                }

                vmList.Add(new UserListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    DisplayName = u.DisplayName,
                    Roles = rolesCsv,
                    BaseName = baseName,
                    IsActive = u.IsActive,
                    LastLoginUtc = u.LastLoginUtc,
                    CreatedAtUtc = u.CreatedAtUtc
                });
            }

            return View(vmList);
        }

        // You can add Details/Edit/Delete actions below as needed (Admin-only)
    }
}