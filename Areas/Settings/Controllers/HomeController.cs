using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    public class HomeController : Controller
    {
        // Main settings page
        public IActionResult Index()
        {
            return View();
        }

        // This action is used for access denied redirection from cookie options
        // No admin can see the access denied message
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}