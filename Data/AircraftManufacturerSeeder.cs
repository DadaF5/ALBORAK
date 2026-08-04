using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    // ⚠ ASSUMED SHAPE — AircraftManufacturer.cs was not provided in this session.
    // Assumed properties: Code, Name, Description, SortOrder, IsActive — confirmed
    // to at least have a unique Code index (see FRAContext.OnModelCreating).
    // Adjust if your real model has additional fields (e.g. CountryId per
    // Country.cs comment: "AircraftManufacturer.CountryId — manufacturer country (future)").
    public class AircraftManufacturerSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AircraftManufacturer>().AnyAsync())
                return;

            var manufacturers = new List<AircraftManufacturer>
            {
                new() { Code = "LM",      Name = "Lockheed Martin",     SortOrder = 1, IsActive = true },
                new() { Code = "BOEING",  Name = "Boeing",              SortOrder = 2, IsActive = true },
                new() { Code = "AIRBUS",  Name = "Airbus",              SortOrder = 3, IsActive = true },
                new() { Code = "NORTHR",  Name = "Northrop Grumman",    SortOrder = 4, IsActive = true },
                new() { Code = "SUD",     Name = "Sud Aviation",        SortOrder = 5, IsActive = true },
            };

            await context.Set<AircraftManufacturer>().AddRangeAsync(manufacturers);
            await context.SaveChangesAsync();
        }
    }
}