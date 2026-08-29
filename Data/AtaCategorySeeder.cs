using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AtaCategorySeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            var wanted = new List<AtaCategory>
            {
                new() { Code = "GEN",   Name = "Aircraft General",  SortOrder = 1, IsActive = true },
                new() { Code = "AFS",   Name = "Airframe Systems",  SortOrder = 2, IsActive = true },
                new() { Code = "STRUC", Name = "Structure",         SortOrder = 3, IsActive = true },
                new() { Code = "PWR",   Name = "Power Plant",       SortOrder = 4, IsActive = true },
            };

            var existingCodes = await context.Set<AtaCategory>()
                .Select(x => x.Code)
                .ToListAsync();

            var missing = wanted
                .Where(x => !existingCodes.Contains(x.Code))
                .ToList();

            if (missing.Any())
            {
                await context.Set<AtaCategory>().AddRangeAsync(missing);
                await context.SaveChangesAsync();
            }
        }
    }
}