using FRAProject.Data;
using FRAProject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly FRAContext _context;

        public HomeController(FRAContext context)
        {
            _context = context;
        }

        // Main settings page
        public async Task<IActionResult> Index(string? tab = "bases")
        {
            tab = string.IsNullOrWhiteSpace(tab) ? "bases" : tab.ToLowerInvariant();
            ViewData["ActiveTab"] = tab;

            var bases = new List<BaseDto>();
            if (tab == "bases")
            {
                bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new BaseDto
                    {
                        Id = b.Id,
                        BaseName = b.BaseName,
                        Longitude = b.Longitude,
                        Latitude = b.Latitude,
                        BaseNameLocal = b.BaseCode + " - " + b.Location
                    })
                    .ToListAsync();
            }

            return View(bases);
        }
    }
}
