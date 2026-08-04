using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class BaseSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<Base>().AnyAsync())
                return;

            var bases = new List<Base>
            {
                new() { BaseCode = "2BAFRA", BaseName = "2ème Base Aérienne", Location = "Meknès", IsActive = true },
                new() { BaseCode = "3BAFRA",    BaseName = "3ère Base Aérienne",Location = "Kénitra", IsActive = true },
                new() { BaseCode = "BEFRA",    BaseName = "Base Ecole",         Location = "Marrakech", IsActive = true },
            };

            await context.Set<Base>().AddRangeAsync(bases);
            await context.SaveChangesAsync();
        }
    }
}