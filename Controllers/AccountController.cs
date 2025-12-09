using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Data;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly FRAContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            FRAContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        // GET: /Account/Create (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new RegisterUserViewModel();

            vm.AvailableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            // populate all organizational lists so admin can choose defaults for the new user
            vm.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            vm.WingList = await _context.Set<Wing>()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            vm.DepartmentList = await _context.Set<Department>()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();

            vm.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();

            return View(vm);
        }

        // POST: /Account/Create (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RegisterUserViewModel model)
        {
            // repopulate lists for redisplay if needed
            model.AvailableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            model.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            model.WingList = await _context.Set<Wing>()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            model.DepartmentList = await _context.Set<Department>()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();

            model.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            model.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            // create ApplicationUser from view model
            var user = new ApplicationUser
            {
                UserName = model.Email, // or use a different username strategy
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                BaseId = model.BaseId,
                WingId = model.WingId,
                DepartmentId = model.DepartmentId,
                SquadronId = model.SquadronId,
                AcMainGroupId = model.AcMainGroupId,
                EmailConfirmed = true, // adjust per your policy
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            // Assign roles selected in the model
            if (model.SelectedRoles?.Any() == true)
            {
                foreach (var role in model.SelectedRoles.Distinct())
                {
                    if (await _roleManager.RoleExistsAsync(role))
                        await _userManager.AddToRoleAsync(user, role);
                }
            }

            // Add or replace organization-scoping claims so the user's cookie (and CurrentUserService) will include them
            await AddOrReplaceClaimAsync(user, "BaseId", model.BaseId?.ToString());
            await AddOrReplaceClaimAsync(user, "WingId", model.WingId?.ToString());
            await AddOrReplaceClaimAsync(user, "DepartmentId", model.DepartmentId?.ToString());
            await AddOrReplaceClaimAsync(user, "SquadronId", model.SquadronId?.ToString());
            await AddOrReplaceClaimAsync(user, "AcMainGroupId", model.AcMainGroupId?.ToString());

            _logger.LogInformation("Admin {Admin} created user {User}.", User.Identity?.Name, user.UserName);

            return RedirectToAction("Index", "Users");
        }
        // helper to add or replace a claim on a user (removes any existing claim of the same type)
        private async Task AddOrReplaceClaimAsync(ApplicationUser user, string claimType, string? value)
        {
            var existing = (await _userManager.GetClaimsAsync(user)).Where(c => c.Type == claimType).ToList();
            if (existing.Any())
            {
                foreach (var e in existing) await _userManager.RemoveClaimAsync(user, e);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                await _userManager.AddClaimAsync(user, new Claim(claimType, value));
            }
        }

        // Login/Logout actions omitted for brevity; keep existing SignInManager<ApplicationUser> usage
    }
}