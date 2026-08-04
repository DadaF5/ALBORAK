using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AcCategorySeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AcCategory>().AnyAsync())
                return;

            // Seed data per AcCategory.cs XML doc comment.
            var categories = new List<AcCategory>
            {
                new() { Code = "AVION", Name = "Avion",              Description = "Aéronef à voilure fixe",     IconKey = "✈", SortOrder = 1, IsActive = true },
                new() { Code = "HELI",  Name = "Hélicoptère",        Description = "Aéronef à voilure tournante", IconKey = "🚁", SortOrder = 2, IsActive = true },
                new() { Code = "UAS",   Name = "UAS / Drone",        Description = "Aéronef sans équipage",       IconKey = "◈", SortOrder = 3, IsActive = true },
            };

            await context.Set<AcCategory>().AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}