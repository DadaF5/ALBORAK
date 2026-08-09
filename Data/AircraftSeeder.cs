using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // FK dependencies
            await AcTypeSeeder.SeedAsync(context);
            await AcStatusTypeSeeder.SeedAsync(context);
            await BaseSeeder.SeedAsync(context);

            var f16Id = await context.Set<AcType>().Where(t => t.Code == "F16C").Select(t => t.Id).SingleAsync();
            var c130Id = await context.Set<AcType>().Where(t => t.Code == "C130H").Select(t => t.Id).SingleAsync();
            var f5eId = await context.Set<AcType>().Where(t => t.Code == "F5E").Select(t => t.Id).SingleAsync();
            var f5fId = await context.Set<AcType>().Where(t => t.Code == "F5F").Select(t => t.Id).SingleAsync();
            var ajetId = await context.Set<AcType>().Where(t => t.Code == "AJET").Select(t => t.Id).SingleAsync();

            var oprId = await context.Set<AcStatusType>().Where(s => s.Code == "OPR").Select(s => s.Id).SingleAsync();
            var aogId = await context.Set<AcStatusType>().Where(s => s.Code == "AOG").Select(s => s.Id).SingleAsync();

            var baseId = await context.Set<Base>().Where(b => b.BaseCode == "2BAFRA").Select(b => b.Id).SingleAsync();

            var wanted = new List<Aircraft>
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

                // ── NEW — F5E / F5F / Alpha Jet ─────────────────────────
                // ⚠ Registration/Serial numbers below are PLACEHOLDER
                // values, not real fleet data — adjust to your actual
                // tail numbers before relying on this for anything beyond
                // dev/testing.
                new()
                {
                    TailNo = 5001, Registration = "CN-AVA", SerialNumber = "NG 5001",
                    Manufacturer = "Northrop Grumman", Model = "F-5E",
                    AcTypeId = f5eId, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 6
                },
                new()
                {
                    TailNo = 5002, Registration = "CN-AVB", SerialNumber = "NG 5002",
                    Manufacturer = "Northrop Grumman", Model = "F-5E",
                    AcTypeId = f5eId, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 7
                },
                new()
                {
                    TailNo = 5101, Registration = "CN-AVF", SerialNumber = "NG 5101",
                    Manufacturer = "Northrop Grumman", Model = "F-5F",
                    AcTypeId = f5fId, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 8
                },
                new()
                {
                    TailNo = 6001, Registration = "CN-AJA", SerialNumber = "DD 6001",
                    Manufacturer = "Dassault-Dornier", Model = "Alpha Jet",
                    AcTypeId = ajetId, AcStatusTypeId = oprId, BaseId = baseId,
                    Status = AircraftStatus.Available, IsActive = true, Serviceable = true,
                    SortOrder = 9
                },
            };

            var existingRegistrations = await context.Set<Aircraft>()
                .Select(a => a.Registration)
                .ToListAsync();

            var missing = wanted.Where(a => !existingRegistrations.Contains(a.Registration)).ToList();

            if (missing.Any())
            {
                await context.Set<Aircraft>().AddRangeAsync(missing);
                await context.SaveChangesAsync();
            }
        }
    }
}