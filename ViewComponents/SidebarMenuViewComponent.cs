
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}