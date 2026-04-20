using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    public class HomeController : Controller
    {
        // GET /Settings
        public IActionResult Index()
        {
            ViewData["Title"] = "Paramètres système";
            return View();
        }
    }
}
