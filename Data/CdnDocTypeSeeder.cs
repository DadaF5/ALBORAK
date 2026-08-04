using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class CdnDocTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<CdnDocType>().AnyAsync())
                return;

            // Seed data per CdnDocType.cs XML doc comment — fixed by DAM regulation.
            var docTypes = new List<CdnDocType>
            {
                new() { Code = "CDN", Name = "Certificat de navigabilité", SortOrder = 1, IsActive = true },
                new() { Code = "ADV", Name = "Autorisation de vol",        SortOrder = 2, IsActive = true },
                new() { Code = "AUT", Name = "Autre",                     SortOrder = 3, IsActive = true },
            };

            await context.Set<CdnDocType>().AddRangeAsync(docTypes);
            await context.SaveChangesAsync();
        }
    }
}