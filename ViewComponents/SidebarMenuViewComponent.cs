
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync()
        {
            return Task.FromResult<IViewComponentResult>(View());
        }
    }
}