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

            vm.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
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

            // Ensure BaseId claim is present if you rely on claims in other parts of the app
            if (model.BaseId.HasValue)
            {
                // add or replace claim
                var existing = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "BaseId");
                if (existing != null)
                {
                    await _userManager.RemoveClaimAsync(user, existing);
                }
                await _userManager.AddClaimAsync(user, new Claim("BaseId", model.BaseId.Value.ToString()));
            }

            _logger.LogInformation("Admin {Admin} created user {User}.", User.Identity?.Name, user.UserName);

            return RedirectToAction("Index", "Users");
        }

        // Login/Logout actions omitted for brevity; keep existing SignInManager<ApplicationUser> usage
    }
}