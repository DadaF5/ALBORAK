// Data/AircraftDocumentSeeder.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public static class AircraftDocumentSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // Require reference data first
            var docTypes = await context.Set<AircraftDocumentType>()
                .AsNoTracking()
                .ToDictionaryAsync(t => t.Code, t => t.Id);

            if (docTypes.Count == 0)
                return;

            // Need aircrafts to attach documents to
            var aircraftIds = await context.Aircrafts
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .Take(4) // seed for first 4 aircrafts
                .ToListAsync();

            if (aircraftIds.Count == 0)
                return;

            // Avoid duplicating seed
            var anyDocs = await context.Set<AircraftDocument>().AnyAsync();
            if (anyDocs)
                return;

            DateTime utcNow = DateTime.UtcNow;

            // helper local function
            AircraftDocument Doc(int aircraftId, string typeCode, string referenceNo, string title, DateTime? validUntilUtc)
                => new()
                {
                    AircraftId = aircraftId,
                    DocumentTypeId = docTypes[typeCode],
                    ReferenceNo = referenceNo,
                    Title = title,
                    IsCurrent = true,
                    Status = "Current",
                    IssuedAtUtc = utcNow.AddMonths(-3),
                    ValidFromUtc = utcNow.AddMonths(-3),
                    ValidUntilUtc = validUntilUtc,
                    CreatedAtUtc = utcNow,
                    CreatedBy = "Seeder"
                };

            var docs = new List<AircraftDocument>();

            // Example data similar to your HTML
            // (You can later adjust to use Registration/TailNo if you want.)
            foreach (var acId in aircraftIds)
            {
                docs.Add(Doc(acId, "CDN", $"DAM/CN/2026/{acId:0000}", "Certificat de navigabilité", utcNow.AddMonths(12)));
                docs.Add(Doc(acId, "CEN", $"DAM/CEN/2026/{acId:0000}", "Certificat d'examen de navigabilité", utcNow.AddMonths(6)));
                docs.Add(Doc(acId, "PEA", $"PEA-FRA-{acId:0000}-Rev01", "Programme d'entretien", null));
                docs.Add(Doc(acId, "LME", $"LMER-FRA-{acId:0000}", "LME/LTTE applicable", null));
            }

            context.AddRange(docs);
            await context.SaveChangesAsync();
        }
    }
}