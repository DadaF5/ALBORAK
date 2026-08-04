using FRAProject.Areas.Settings.Models;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AcMainGroupSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AcMainGroup>().AnyAsync())
                return;

            // FK dependencies — ensure parents exist first.
            await AcCategorySeeder.SeedAsync(context);
            await BaseSeeder.SeedAsync(context);

            var avionCategoryId = await context.Set<AcCategory>()
                .Where(c => c.Code == "AVION").Select(c => c.Id).SingleAsync();

            var baseId = await context.Set<Base>()
                .Where(b => b.BaseCode == "2BAFRA").Select(b => b.Id).SingleAsync();

            var groups = new List<AcMainGroup>
            {
                new() { Code = "CHASSE-2B", Name = "Chasse 2BAFRA",    Description = "Groupe chasse — 2ème BAFRA",     AcCategoryId = avionCategoryId, BaseId = baseId, SortOrder = 1, IsActive = true },
                new() { Code = "TRANS-2B",  Name = "Transport 2BAFRA", Description = "Groupe transport — 2ème BAFRA",  AcCategoryId = avionCategoryId, BaseId = baseId, SortOrder = 2, IsActive = true },
            };

            await context.Set<AcMainGroup>().AddRangeAsync(groups);
            await context.SaveChangesAsync();
        }
    }
}