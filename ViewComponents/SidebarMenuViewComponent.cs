
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FRAProject.Services;

namespace FRAProject.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly ILogger<SidebarMenuViewComponent> _logger;

        public SidebarMenuViewComponent(IMenuService menuService, ILogger<SidebarMenuViewComponent> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<FRAProject.Models.MenuItem> menu;
            try
            {
                menu = await _menuService.GetMenuForUserAsync(HttpContext.User);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sidebar menu items.");
                menu = Enumerable.Empty<FRAProject.Models.MenuItem>();
            }
            return View(menu);
        }
    }
}