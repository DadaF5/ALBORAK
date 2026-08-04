using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftVersionSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AircraftVersion>().AnyAsync())
                return;

            await AcTypeSeeder.SeedAsync(context);

            var f16Id = await context.Set<AcType>()
                .Where(t => t.Code == "F16C").Select(t => t.Id).SingleAsync();

            var c130Id = await context.Set<AcType>()
                .Where(t => t.Code == "C130H").Select(t => t.Id).SingleAsync();

            var versions = new List<AircraftVersion>
            {
                new() { Code = "BLK50", Name = "Block 50", AcTypeId = f16Id,  SortOrder = 1, IsActive = true },
                new() { Code = "BLK52", Name = "Block 52+", AcTypeId = f16Id, SortOrder = 2, IsActive = true },
                new() { Code = "STD",   Name = "Standard",  AcTypeId = c130Id, SortOrder = 1, IsActive = true },
            };

            await context.Set<AircraftVersion>().AddRangeAsync(versions);
            await context.SaveChangesAsync();
        }
    }
}