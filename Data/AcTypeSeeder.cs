using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AcTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AcType>().AnyAsync())
                return;

            // FK dependencies
            await AcMainGroupSeeder.SeedAsync(context);
            await AircraftManufacturerSeeder.SeedAsync(context);

            var chasseGroupId = await context.Set<AcMainGroup>()
                .Where(g => g.Code == "CHASSE-2B").Select(g => g.Id).SingleAsync();

            var transGroupId = await context.Set<AcMainGroup>()
                .Where(g => g.Code == "TRANS-2B").Select(g => g.Id).SingleAsync();

            var lmId = await context.Set<AircraftManufacturer>()
                .Where(m => m.Code == "LM").Select(m => m.Id).SingleAsync();

            var types = new List<AcType>
            {
                new()
                {
                    Code = "F16C", Name = "F-16C Fighting Falcon",
                    Description = "Chasseur monoplace",
                    AcMainGroupId = chasseGroupId, AircraftManufacturerId = lmId,
                    MaxGrossWeight = 19187, MaxEngines = 1, SeatCount = 1, MaxPassengers = 0,
                    SortOrder = 1, IsActive = true
                },
                new()
                {
                    Code = "C130H", Name = "C-130H Hercules",
                    Description = "Transport tactique quadrimoteur",
                    AcMainGroupId = transGroupId, AircraftManufacturerId = lmId,
                    MaxGrossWeight = 70307, MaxEngines = 4, SeatCount = 5, MaxPassengers = 92,
                    SortOrder = 2, IsActive = true
                },
            };

            await context.Set<AcType>().AddRangeAsync(types);
            await context.SaveChangesAsync();
        }
    }
}