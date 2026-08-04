using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class CountrySeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<Country>().AnyAsync())
                return;

            // NOTE: Country uses IsoCode, not Code — different from every other lookup.
            var countries = new List<Country>
            {
                new() { IsoCode = "MA", Name = "Maroc",          Continent = "Afrique",       SortOrder = 1, IsActive = true },
                new() { IsoCode = "FR", Name = "France",         Continent = "Europe",        SortOrder = 2, IsActive = true },
                new() { IsoCode = "US", Name = "États-Unis",     Continent = "Amérique",      SortOrder = 3, IsActive = true },
                new() { IsoCode = "DE", Name = "Allemagne",      Continent = "Europe",        SortOrder = 4, IsActive = true },
                new() { IsoCode = "IT", Name = "Italie",         Continent = "Europe",        SortOrder = 5, IsActive = true },
                new() { IsoCode = "ES", Name = "Espagne",        Continent = "Europe",        SortOrder = 6, IsActive = true },
                new() { IsoCode = "GB", Name = "Royaume-Uni",    Continent = "Europe",        SortOrder = 7, IsActive = true },
                new() { IsoCode = "BR", Name = "Brésil",         Continent = "Amérique",      SortOrder = 8, IsActive = true },
            };

            await context.Set<Country>().AddRangeAsync(countries);
            await context.SaveChangesAsync();
        }
    }
}