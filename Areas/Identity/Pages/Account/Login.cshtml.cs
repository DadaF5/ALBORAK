using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using FRAProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace FRAProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        // Platform-administration pages — a small, fixed set by design
        // (distinct from per-module business areas like AircraftMaintenance/
        // SquadronOps/HR/Healthcare, which are gated by UserAssignment
        // policies and correctly show AccessDenied when a real non-admin
        // user lacks access — that's informative, telling them to request
        // access). These pages should never even be *offered* to a
        // non-admin, so a stale ReturnUrl pointing at one (e.g. left over
        // from a previous Admin session that logged out mid-browse) must
        // not be honored for whoever logs in next.
        private static readonly string[] AdminOnlyPrefixes =
        {
            "/Users", "/Roles", "/UserAssignments", "/Settings"
        };

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // ⚠ Previously never written anywhere — UsersController's
                    // Create/Edit don't touch it (it's not an editable field),
                    // and this Razor Page's OnPostAsync stopped at the
                    // PasswordSignInAsync call without recording the login.
                    // Real consequence: the Users list/Details "Last login"
                    // column always showed "never", even for users who had
                    // logged in repeatedly, making it useless for auditing
                    // or confirming a scoped-user test actually happened.
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        user.LastLoginUtc = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    _logger.LogInformation("User logged in: {Email}", Input.Email);

                    // ⚠ Bug fix: PasswordSignInAsync succeeding says nothing
                    // about whether THIS user can reach returnUrl — it's
                    // whatever page triggered the original 401 challenge,
                    // which may have been requested by a different,
                    // previously-logged-in (e.g. Admin) session. Blindly
                    // honoring it bounced a freshly-logged-in non-admin
                    // straight to AccessDenied instead of Home.
                    var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
                    var targetsAdminOnlyPage = !isAdmin &&
                        AdminOnlyPrefixes.Any(p => returnUrl.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                    if (targetsAdminOnlyPage)
                    {
                        _logger.LogInformation(
                            "Redirecting non-admin {Email} to Home instead of stale admin ReturnUrl {ReturnUrl}",
                            Input.Email, returnUrl);
                        return LocalRedirect(Url.Content("~/"));
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out: {Email}", Input.Email);
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
