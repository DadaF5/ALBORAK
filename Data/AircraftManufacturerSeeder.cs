using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftManufacturerSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            var wanted = new List<AircraftManufacturer>
            {
                new() { Code = "LM",       Name = "Lockheed Martin",     SortOrder = 1, IsActive = true },
                new() { Code = "BOEING",   Name = "Boeing",              SortOrder = 2, IsActive = true },
                new() { Code = "AIRBUS",   Name = "Airbus",              SortOrder = 3, IsActive = true },
                new() { Code = "NORTHR",   Name = "Northrop Grumman",    SortOrder = 4, IsActive = true },
                new() { Code = "SUD",      Name = "Sud Aviation",        SortOrder = 5, IsActive = true },
                new() { Code = "DASSAULT", Name = "Dassault-Dornier",    SortOrder = 6, IsActive = true },
            };

            var existingCodes = await context.Set<AircraftManufacturer>()
                .Select(m => m.Code)
                .ToListAsync();

            var missing = wanted.Where(m => !existingCodes.Contains(m.Code)).ToList();

            if (missing.Any())
            {
                await context.Set<AircraftManufacturer>().AddRangeAsync(missing);
                await context.SaveChangesAsync();
            }
        }
    }
}