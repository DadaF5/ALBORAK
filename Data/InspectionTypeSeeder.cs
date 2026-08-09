using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class InspectionTypeSeeder
    {
        // Real data from Table 2-1 (periodicity table, TO XX1F-5E-6WC-3 —
        // this is the F-5E manual, NOT C-130). PE1 every 300h, PE2 every
        // 600h, etc. PE7/PE8 intervals were partially obscured in the
        // source photo — not seeded, add later once confirmed.
        private static readonly (string Code, string Name, int IntervalHours, int SortOrder)[] PeData =
        {
            ("PE1", "1st Periodic Inspection", 300,  1),
            ("PE2", "2nd Periodic Inspection", 600,  2),
            ("PE3", "3rd Periodic Inspection", 900,  3),
            ("PE4", "4th Periodic Inspection", 1200, 4),
            ("PE5", "5th Periodic Inspection", 1500, 5),
            ("PE6", "6th Periodic Inspection", 1800, 6),
        };

        public static async Task SeedAsync(FRAContext context)
        {
            // AcTypeSeeder is now the single authoritative source for
            // AcType creation (per-code idempotent) — no more private
            // GetOrCreateF5EAcTypeAsync helper here.
            await AcTypeSeeder.SeedAsync(context);

            var f5eId = await context.Set<AcType>()
                .Where(t => t.Code == "F5E").Select(t => t.Id).SingleAsync();

            // ── 1) InspectionType (PE1-PE6) ─────────────────────────────
            if (!await context.Set<InspectionType>().AnyAsync())
            {
                var inspectionTypes = PeData.Select(p => new InspectionType
                {
                    AcTypeId = f5eId,
                    Code = p.Code,
                    Name = p.Name,
                    Kind = "PLANNED",
                    IntervalHours = p.IntervalHours,
                    SortOrder = (byte)p.SortOrder,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                }).ToList();

                await context.Set<InspectionType>().AddRangeAsync(inspectionTypes);
                await context.SaveChangesAsync();
            }

            // ── 2) MaintenanceProgram (PE1-PE6) ─────────────────────────
            var existingPeProgramCodes = await context.Set<MaintenanceProgram>()
                .Where(p => p.Code.StartsWith("PE"))
                .Select(p => p.Code)
                .ToListAsync();

            var missingPePrograms = PeData
                .Where(p => !existingPeProgramCodes.Contains(p.Code))
                .Select(p => new MaintenanceProgram
                {
                    AcTypeId = f5eId,
                    Code = p.Code,
                    Name = p.Name,
                    Description = $"Programme d'entretien périodique — {p.Name}",
                    SortOrder = (byte)p.SortOrder,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .ToList();

            if (missingPePrograms.Any())
            {
                await context.Set<MaintenanceProgram>().AddRangeAsync(missingPePrograms);
                await context.SaveChangesAsync();
            }

            // ── 3) InspectionTypeProgram junction (1:1 PE <-> PE) ───────
            if (!await context.Set<InspectionTypeProgram>().AnyAsync())
            {
                var inspectionTypeIds = await context.Set<InspectionType>()
                    .Where(it => it.Code.StartsWith("PE"))
                    .ToDictionaryAsync(it => it.Code, it => it.Id);

                var programIds = await context.Set<MaintenanceProgram>()
                    .Where(p => p.Code.StartsWith("PE"))
                    .ToDictionaryAsync(p => p.Code, p => p.Id);

                var links = PeData
                    .Where(p => inspectionTypeIds.ContainsKey(p.Code) && programIds.ContainsKey(p.Code))
                    .Select(p => new InspectionTypeProgram
                    {
                        InspectionTypeId = inspectionTypeIds[p.Code],
                        MaintenanceProgramId = programIds[p.Code],
                        SortOrder = p.SortOrder
                    })
                    .ToList();

                await context.Set<InspectionTypeProgram>().AddRangeAsync(links);
                await context.SaveChangesAsync();
            }
        }
    }
}