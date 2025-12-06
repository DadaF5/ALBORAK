
using Microsoft.AspNetCore.Mvc;
using FRAProject.Services;

namespace FRAProject.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public SidebarMenuViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = await _menuService.GetMenuForUserAsync(HttpContext.User);
            return View(menu);
        }
    }
}