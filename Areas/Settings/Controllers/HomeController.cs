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
    }
}