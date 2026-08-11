using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class WorkSectionSeeder
    {
        // Starter set only — confirmed 4 sections (Electric, Electronic,
        // Hydraulic, GTR), applied across every currently-seeded AcType.
        // "C145" was requested but isn't seeded yet, so per fallback
        // instruction it's skipped — falls back to the 5 AcTypes that do
        // exist (F16C, C130H, F5E, F5F, AJET). This list is intentionally
        // incomplete — add more via the WorkSections UI as they're
        // identified from real Formule 13 scans.
        private static readonly (string Code, string Name)[] SectionData =
        {
            ("ELEC",    "Électrique"),
            ("ELECTRO", "Électronique"),
            ("HYD",     "Hydraulique"),
            ("GTR",     "Groupe Turboréacteur"),
        };

        public static async Task SeedAsync(FRAContext context)
        {
            await AcTypeSeeder.SeedAsync(context);

            var acTypeIds = await context.Set<AcType>()
                .Where(t => new[] { "F16C", "C130H", "F5E", "F5F", "AJET" }.Contains(t.Code))
                .Select(t => new { t.Id, t.Code })
                .ToListAsync();

            var existing = await context.Set<WorkSection>()
                .Select(x => new { x.AcTypeId, x.Code })
                .ToListAsync();
            var existingSet = existing.Select(x => (x.AcTypeId, x.Code)).ToHashSet();

            var toAdd = new List<WorkSection>();
            var sortOrder = 1;

            foreach (var acType in acTypeIds)
            {
                foreach (var section in SectionData)
                {
                    if (existingSet.Contains((acType.Id, section.Code)))
                        continue;

                    toAdd.Add(new WorkSection
                    {
                        AcTypeId = acType.Id,
                        Code = section.Code,
                        Name = section.Name,
                        SortOrder = (byte)sortOrder,
                        IsActive = true
                    });
                }
                sortOrder++;
            }

            if (toAdd.Any())
            {
                await context.Set<WorkSection>().AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
