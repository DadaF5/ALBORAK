using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Extensions
{
    public static class EnumExtensions
    {
        // Get the DisplayAttribute Name if present, otherwise the enum name
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member == null) return value.ToString();
            var display = member.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? value.ToString();
        }

        // Create a SelectList suitable for asp-items from an enum type TEnum.
        public static IEnumerable<SelectListItem> ToSelectList<TEnum>(bool includeEmpty = false, string emptyText = "-- Select --")
            where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            var items = values.Select(v => new SelectListItem
            {
                Text = (v as Enum)!.GetDisplayName(),
                Value = Convert.ToInt32(v).ToString()
            }).ToList();

            if (includeEmpty)
            {
                items.Insert(0, new SelectListItem { Text = emptyText, Value = "" });
            }

            return items;
        }
    }
}