using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class MaintenanceProgramSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<MaintenanceProgram>().AnyAsync())
                return;

            await AcTypeSeeder.SeedAsync(context);

            var c130Id = await context.Set<FRAProject.Areas.Settings.Models.AcType>()
                .Where(t => t.Code == "C130H").Select(t => t.Id).SingleAsync();

            // Real data referenced in the project handoff doc (SP-9 / SP-29 dual threshold example).
            var programs = new List<MaintenanceProgram>
            {
                new()
                {
                    AcTypeId = c130Id, Code = "SP-9", Name = "Special Inspection 9 — ATA 27",
                    Description = "Programme d'entretien spécial — commandes de vol",
                    DocReference = "AMM-27-SP9", Edition = "Rev. 8",
                    SortOrder = 1, IsActive = true
                },
                new()
                {
                    AcTypeId = c130Id, Code = "SP-29", Name = "Special Inspection 29 — ATA 53",
                    Description = "Programme d'entretien spécial — structure fuselage",
                    DocReference = "AMM-53-SP29", Edition = "Rev. 5",
                    SortOrder = 2, IsActive = true
                },
            };

            await context.Set<MaintenanceProgram>().AddRangeAsync(programs);
            await context.SaveChangesAsync();
        }
    }
}