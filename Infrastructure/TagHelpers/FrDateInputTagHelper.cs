using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace FRAProject.Infrastructure.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    public class FrDateInputTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression AspFor { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var t = AspFor.ModelExplorer.ModelType;
            var isDate =
                t == typeof(DateOnly) || t == typeof(DateOnly?) ||
                t == typeof(DateTime) || t == typeof(DateTime?);

            if (!isDate) return;

            output.Attributes.SetAttribute("type", "text");
            output.Attributes.SetAttribute("data-fr-date", "true");
            output.Attributes.SetAttribute("placeholder", "dd-MM-yyyy");
            output.Attributes.SetAttribute("autocomplete", "off");
            output.Attributes.SetAttribute("inputmode", "numeric");

            // Render existing model value in dd-MM-yyyy
            var model = AspFor.Model;
            string? formatted = model switch
            {
                DateOnly d => d.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                DateTime dt => dt.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(formatted))
            {
                output.Attributes.SetAttribute("value", formatted);
            }
        }
    }
}