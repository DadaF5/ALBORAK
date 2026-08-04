using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class MissionRoleSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<MissionRole>().AnyAsync())
                return;

            await AcCategorySeeder.SeedAsync(context);

            var avionId = await context.Set<AcCategory>()
                .Where(c => c.Code == "AVION").Select(c => c.Id).SingleAsync();
            var heliId = await context.Set<AcCategory>()
                .Where(c => c.Code == "HELI").Select(c => c.Id).SingleAsync();

            // Seed data per MissionRole.cs XML doc — matches Form 5a Step 2 dropdown.
            var roles = new List<MissionRole>
            {
                new() { Code = "CHASSE",  Name = "Chasse / Interception",     AcCategoryId = avionId, SortOrder = 1, IsActive = true },
                new() { Code = "TRANS",   Name = "Transport",                 AcCategoryId = null,    SortOrder = 2, IsActive = true },
                new() { Code = "ENTR",    Name = "Entraînement",              AcCategoryId = null,    SortOrder = 3, IsActive = true },
                new() { Code = "RECO",    Name = "Reconnaissance / ISR",      AcCategoryId = null,    SortOrder = 4, IsActive = true },
                new() { Code = "SAR",     Name = "SAR / CSAR",                AcCategoryId = null,    SortOrder = 5, IsActive = true },
                new() { Code = "ASSAULT", Name = "Hélicoptère d'assaut",      AcCategoryId = heliId,  SortOrder = 6, IsActive = true },
                new() { Code = "UAV-ISR", Name = "Drone ISR",                 AcCategoryId = null,    SortOrder = 7, IsActive = true },
            };

            await context.Set<MissionRole>().AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }
}