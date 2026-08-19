using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class WorkSectionSeeder
    {
        // Starter set only — confirmed 4 sections (Electric, Electronic,
        // Hydraulic, GTR), applied once per AcMainGroup (real aircraft
        // family), NOT per individual AcType variant. F16C+F16D share the
        // same sections (same F16-2B family), as do F5E+F5F (same F5-2B
        // family) — a section only earns its own row when the FAMILY
        // itself genuinely differs. This list is intentionally
        // incomplete — add more via the WorkSections UI as they're
        // identified from real Formule 13 scans.
        //
        // NOTE: originally seeded per-AcType (a separate duplicate row per
        // F16C AND per F16D, etc.), back when AcMainGroup's seeded data
        // had drifted and wasn't safe to key on. Moved to AcMainGroup
        // after the RBAC session fixed that data and WorkSection itself
        // was migrated off AcTypeId (see WorkSection.cs).
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

            // F16-2B (F16C+F16D), F5-2B (F5E+F5F), TRANS-2B (C130H),
            // AJET-2B (AJET) — the four families with WorkSections seeded
            // today, per the real seeded AcMainGroup codes.
            var acMainGroupIds = await context.Set<AcMainGroup>()
                .Where(g => new[] { "F16-2B", "F5-2B", "TRANS-2B", "AJET-2B" }.Contains(g.Code))
                .Select(g => new { g.Id, g.Code })
                .ToListAsync();

            var existing = await context.Set<WorkSection>()
                .Select(x => new { x.AcMainGroupId, x.Code })
                .ToListAsync();
            var existingSet = existing.Select(x => (x.AcMainGroupId, x.Code)).ToHashSet();

            var toAdd = new List<WorkSection>();
            var sortOrder = 1;

            foreach (var acMainGroup in acMainGroupIds)
            {
                foreach (var section in SectionData)
                {
                    if (existingSet.Contains((acMainGroup.Id, section.Code)))
                        continue;

                    toAdd.Add(new WorkSection
                    {
                        AcMainGroupId = acMainGroup.Id,
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
