using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<Aircraft>().AnyAsync())
                return;

            // FK dependencies
            await AcTypeSeeder.SeedAsync(context);
            await AcStatusTypeSeeder.SeedAsync(context);
            await BaseSeeder.SeedAsync(context);

            var f16Id = await context.Set<AcType>().Where(t => t.Code == "F16C").Select(t => t.Id).SingleAsync();
            var c130Id = await context.Set<AcType>().Where(t => t.Code == "C130H").Select(t => t.Id).SingleAsync();

            var oprId = await context.Set<AcStatusType>().Where(s => s.Code == "OPR").Select(s => s.Id).SingleAsync();
            var aogId = await context.Set<AcStatusType>().Where(s => s.Code == "AOG").Select(s => s.Id).SingleAsync();

            var baseId = await context.Set<Base>().Where(b => b.BaseCode == "2BAFRA").Select(b => b.Id).SingleAsync();

            var aircrafts = new List<Aircraft>
            {
                // Reference test aircraft from project handoff doc —
                // used to validate "LM 4436 and up" applicability rules.
                new()
                {
                    TailNo = 4713, Registration = "CN-AOG", SerialNumber = "LM 4713",
                    Manufacturer = "Lockheed Martin", Model = "C-130H",
                    AcTypeId = c130Id, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    ServiceEntryDate = new DateOnly(1979, 1, 4),
                    TotalFlightMinutes = 21575 * 60,
                    SortOrder = 1
                },
                new()
                {
                    TailNo = 4436, Registration = "CN-AKM", SerialNumber = "LM 4436",
                    Manufacturer = "Lockheed Martin", Model = "C-130H",
                    AcTypeId = c130Id, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 2
                },
                new()
                {
                    TailNo = 1001, Registration = "CN-TRN", SerialNumber = "LM 1001",
                    Manufacturer = "Lockheed Martin", Model = "C-130H",
                    AcTypeId = c130Id, AcStatusTypeId = aogId, BaseId = baseId,
                    Status = AircraftStatus.Unserviceable, IsActive = true, Serviceable = false,
                    SortOrder = 3
                },
                new()
                {
                    TailNo = 2001, Registration = "CN-FGH", SerialNumber = "F16-2001",
                    Manufacturer = "Lockheed Martin", Model = "F-16C",
                    AcTypeId = f16Id, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 4
                },
                new()
                {
                    TailNo = 2002, Registration = "CN-ABD", SerialNumber = "F16-2002",
                    Manufacturer = "Lockheed Martin", Model = "F-16C",
                    AcTypeId = f16Id, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 5
                },
            };

            await context.Set<Aircraft>().AddRangeAsync(aircrafts);
            await context.SaveChangesAsync();
        }
    }
}