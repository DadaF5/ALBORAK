using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    // Single authoritative source for AcType creation. Refactored from a
    // table-wide AnyAsync() guard to a per-code idempotent pattern — the
    // old guard silently blocked adding new types once ANY row existed,
    // which forced ad-hoc "GetOrCreate" workarounds in other seeders
    // (InspectionTypeSeeder had its own private F5E helper). Every other
    // seeder should now call AcTypeSeeder.SeedAsync(context) and look up
    // by Code, never create an AcType itself.
    public class AcTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            await AcMainGroupSeeder.SeedAsync(context);
            await AircraftManufacturerSeeder.SeedAsync(context);

            var chasseGroupId = await context.Set<AcMainGroup>()
                .Where(g => g.Code == "CHASSE-2B").Select(g => g.Id).SingleAsync();
            var transGroupId = await context.Set<AcMainGroup>()
                .Where(g => g.Code == "TRANS-2B").Select(g => g.Id).SingleAsync();

            var lmId = await context.Set<AircraftManufacturer>()
                .Where(m => m.Code == "LM").Select(m => m.Id).SingleAsync();
            var northropId = await context.Set<AircraftManufacturer>()
                .Where(m => m.Code == "NORTHR").Select(m => m.Id).SingleAsync();
            var dassaultId = await context.Set<AircraftManufacturer>()
                .Where(m => m.Code == "DASSAULT").Select(m => m.Id).SingleAsync();

            var wanted = new List<AcType>
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
                    Code = "F16D", Name = "F-16D Fighting Falcon",
                    Description = "Chasseur biplace (entraînement) — même famille que F16C",
                    AcMainGroupId = chasseGroupId, AircraftManufacturerId = lmId,
                    MaxGrossWeight = 19200, MaxEngines = 1, SeatCount = 2, MaxPassengers = 0,
                    SortOrder = 2, IsActive = true
                },
                new()
                {
                    Code = "C130H", Name = "C-130H Hercules",
                    Description = "Transport tactique quadrimoteur",
                    AcMainGroupId = transGroupId, AircraftManufacturerId = lmId,
                    MaxGrossWeight = 70307, MaxEngines = 4, SeatCount = 5, MaxPassengers = 92,
                    SortOrder = 2, IsActive = true
                },
                new()
                {
                    Code = "F5E", Name = "F-5E Tiger II",
                    Description = "Chasseur léger biréacteur monoplace",
                    AcMainGroupId = chasseGroupId, AircraftManufacturerId = northropId,
                    MaxGrossWeight = 11214, MaxEngines = 2, SeatCount = 1, MaxPassengers = 0,
                    SortOrder = 3, IsActive = true
                },
                new()
                {
                    Code = "F5F", Name = "F-5F Tiger II",
                    Description = "Chasseur léger biréacteur biplace (entraînement)",
                    AcMainGroupId = chasseGroupId, AircraftManufacturerId = northropId,
                    MaxGrossWeight = 11340, MaxEngines = 2, SeatCount = 2, MaxPassengers = 0,
                    SortOrder = 4, IsActive = true
                },
                new()
                {
                    Code = "AJET", Name = "Alpha Jet",
                    Description = "Avion d'entraînement / appui léger biplace",
                    AcMainGroupId = chasseGroupId, AircraftManufacturerId = dassaultId,
                    MaxGrossWeight = 8000, MaxEngines = 2, SeatCount = 2, MaxPassengers = 0,
                    SortOrder = 5, IsActive = true
                },
            };

            var existingCodes = await context.Set<AcType>()
                .Select(t => t.Code)
                .ToListAsync();

            var missing = wanted.Where(t => !existingCodes.Contains(t.Code)).ToList();

            if (missing.Any())
            {
                await context.Set<AcType>().AddRangeAsync(missing);
                await context.SaveChangesAsync();
            }
        }
    }
}
