using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public static class AircraftDocumentSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // Keep this optional: if you don't want demo data, just don't call it.
            if (await context.AircraftDocuments.AnyAsync())
                return;

            var aircraft = await context.Aircrafts
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();

            if (aircraft == null)
                return;

            var types = await context.AircraftDocumentTypes
                .Where(t => t.IsActive)
                .ToListAsync();

            int? TypeId(string code) => types.FirstOrDefault(t => t.Code == code)?.Id;

            var now = DateTime.UtcNow;

            var docs = new List<AircraftDocument>();

            // CdN
            var cdnTypeId = TypeId("CDN");
            if (cdnTypeId.HasValue)
            {
                docs.Add(new AircraftDocument
                {
                    AircraftId = aircraft.Id,
                    DocumentTypeId = cdnTypeId.Value,
                    ReferenceNo = "DAM/CN/2026/0001",
                    Revision = "00",
                    Title = "CdN — Initial",
                    IssuedAtUtc = now.AddMonths(-6),
                    ValidFromUtc = now.AddMonths(-6),
                    ValidUntilUtc = now.AddMonths(6),
                    IsCurrent = true,
                    Status = "Valid",
                    Notes = "Seeded demo document (no file attached).",
                    CreatedAtUtc = now,
                    CreatedBy = "Seeder"
                });
            }

            // CEN
            var cenTypeId = TypeId("CEN");
            if (cenTypeId.HasValue)
            {
                docs.Add(new AircraftDocument
                {
                    AircraftId = aircraft.Id,
                    DocumentTypeId = cenTypeId.Value,
                    ReferenceNo = "DAM/CEN/2026/0001",
                    Revision = "00",
                    Title = "CEN — Annual",
                    IssuedAtUtc = now.AddMonths(-2),
                    ValidFromUtc = now.AddMonths(-2),
                    ValidUntilUtc = now.AddMonths(10),
                    IsCurrent = true,
                    Status = "Valid",
                    Notes = "Seeded demo document (no file attached).",
                    CreatedAtUtc = now,
                    CreatedBy = "Seeder"
                });
            }

            // PEA
            var peaTypeId = TypeId("PEA");
            if (peaTypeId.HasValue)
            {
                docs.Add(new AircraftDocument
                {
                    AircraftId = aircraft.Id,
                    DocumentTypeId = peaTypeId.Value,
                    ReferenceNo = "PEA-FRA-DEFAULT",
                    Revision = "01",
                    Title = "Programme d’entretien (PEA)",
                    IssuedAtUtc = now.AddYears(-1),
                    ValidFromUtc = now.AddYears(-1),
                    ValidUntilUtc = null,
                    IsCurrent = true,
                    Status = "Current",
                    Notes = "Seeded demo document (no file attached).",
                    CreatedAtUtc = now,
                    CreatedBy = "Seeder"
                });
            }

            if (docs.Count == 0)
                return;

            await context.AircraftDocuments.AddRangeAsync(docs);
            await context.SaveChangesAsync();
        }
    }
}