using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class EmployingAuthoritySeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<EmployingAuthority>().AnyAsync())
                return;

            // Seed data per EmployingAuthority.cs XML doc comment — fixed by regulation.
            var authorities = new List<EmployingAuthority>
            {
                new() { Code = "FRA", Name = "Forces Royales Air",  SortOrder = 1, IsActive = true },
                new() { Code = "MR",  Name = "Marine Royale",       SortOrder = 2, IsActive = true },
                new() { Code = "GR",  Name = "Gendarmerie Royale",  SortOrder = 3, IsActive = true },
                new() { Code = "FT",  Name = "Forces Terrestres",   SortOrder = 4, IsActive = true },
                new() { Code = "AUT", Name = "Autre",               SortOrder = 5, IsActive = true },
            };

            await context.Set<EmployingAuthority>().AddRangeAsync(authorities);
            await context.SaveChangesAsync();
        }
    }
}