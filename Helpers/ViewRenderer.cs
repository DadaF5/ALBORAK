using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class ViewRendererExtensions
{
    public static async Task<string> RenderViewAsync(this Controller controller, string viewName, object model, bool partial = false)
    {
        var svc = controller.HttpContext.RequestServices;
        var viewEngine = (ICompositeViewEngine)svc.GetService(typeof(ICompositeViewEngine));
        var tempDataProvider = (ITempDataProvider)svc.GetService(typeof(ITempDataProvider));
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);
        if (!viewResult.Success) viewResult = viewEngine.FindView(controller.ControllerContext, viewName, true);
        using var sw = new StringWriter();
        var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, new ViewDataDictionary(controller.ViewData) { Model = model }, new TempDataDictionary(controller.HttpContext, tempDataProvider), sw, new HtmlHelperOptions());
        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
