using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftDocumentSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // If already seeded, skip (you can tighten this check if you want)
            if (await context.Set<AircraftDocument>().AnyAsync())
                return;

            // Ensure types exist first (FK dependency)
            await AircraftDocumentTypeSeeder.SeedAsync(context);

            // Load document types into a dictionary for easy use
            var docTypes = await context.Set<AircraftDocumentType>()
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Code);

            // Helper: resolve aircraft by registration/immat
            // IMPORTANT: adjust "Registration" to your real property name
            async Task<int?> FindAircraftIdAsync(string immat)
            {
                var ac = await context.Set<dynamic>() // placeholder, replaced below
                    .FirstOrDefaultAsync();

                return null;
            }

            // Replace the helper above with a strongly typed one once you confirm Aircraft model property:
            // Example (most likely):
            // var ac = await context.Aircrafts.SingleOrDefaultAsync(a => a.Registration == immat);
            // return ac?.Id;

            // Since you didn't paste Aircraft model, I'm providing the seeder in a way
            // that clearly shows what you need to edit:
            //
            // 1) Replace context.Set<Aircraft>() with your DbSet property if you have one (context.Aircrafts)
            // 2) Replace a.Registration with your actual field (a.Immatriculation, a.RegistrationNo, etc.)

            // ---- EDIT THIS SECTION ----
            async Task<int?> FindAircraftIdStrongAsync(string immat)
            {
                var ac = await context.Set<Aircraft>() // or context.Aircrafts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Registration == immat); // <-- change property name!

                return ac?.Id;
            }
            // ---- END EDIT SECTION ----

            async Task AddDocIfAircraftExistsAsync(string immat, string docTypeCode, Action<AircraftDocument> fill)
            {
                var aircraftId = await FindAircraftIdStrongAsync(immat);
                if (aircraftId is null) return;

                var doc = new AircraftDocument
                {
                    AircraftId = aircraftId.Value,
                    DocumentTypeId = docTypes[docTypeCode].Id,
                    IsCurrent = true,
                    Status = "Active",
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "Seeder"
                };

                fill(doc);

                context.Set<AircraftDocument>().Add(doc);
            }

            // Based on your HTML "doc-chip" references:
            await AddDocIfAircraftExistsAsync("CN-AKM", "CDN", d =>
            {
                d.ReferenceNo = "DAM/CN/2024/0087";
                d.Title = "CdN — CN-AKM";
                d.ValidUntilUtc = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-AKM", "CEN", d =>
            {
                d.ReferenceNo = "DAM/CEN/2026/0012";
                d.Title = "CEN — CN-AKM";
                d.IssuedAtUtc = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);
                d.ValidUntilUtc = new DateTime(2027, 1, 20, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-AKM", "PEA", d =>
            {
                d.ReferenceNo = "PEA-FRA-AJ-Rev05";
                d.Title = "PEA — Alpha Jet";
            });

            await AddDocIfAircraftExistsAsync("CN-AKM", "LME", d =>
            {
                d.ReferenceNo = "LMER-AJ-2022";
                d.Title = "LME/LTTE — Alpha Jet";
            });

            await AddDocIfAircraftExistsAsync("CN-TRN", "CDN", d =>
            {
                d.ReferenceNo = "DAM/CN/2024/0031";
                d.Title = "CdN — CN-TRN";
                d.ValidUntilUtc = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-TRN", "CEN", d =>
            {
                d.ReferenceNo = "DAM/CEN/2026/0003";
                d.Title = "CEN — CN-TRN";
                d.IssuedAtUtc = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc);
                d.ValidUntilUtc = new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-TRN", "PEA", d =>
            {
                d.ReferenceNo = "PEA-FRA-C130-Rev04";
                d.Title = "PEA — C-130H";
            });

            await AddDocIfAircraftExistsAsync("CN-TRN", "LME", d =>
            {
                d.ReferenceNo = "LMER-C130-2021";
                d.Title = "LME/LTTE — C-130H";
            });

            await AddDocIfAircraftExistsAsync("CN-TRN", "CDL", d =>
            {
                d.ReferenceNo = "CDL-FRA-C130-2023";
                d.Title = "CDL — C-130H";
            });

            await AddDocIfAircraftExistsAsync("CN-FGH", "CDN", d =>
            {
                d.ReferenceNo = "DAM/CN/2025/0114";
                d.Title = "CdN — CN-FGH";
                d.ValidUntilUtc = new DateTime(2027, 2, 28, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-FGH", "CEN", d =>
            {
                d.ReferenceNo = "DAM/CEN/2026/0021";
                d.Title = "CEN — CN-FGH";
                d.IssuedAtUtc = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
                d.ValidUntilUtc = new DateTime(2027, 4, 10, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-FGH", "PEA", d =>
            {
                d.ReferenceNo = "PEA-FRA-F5E-Rev03";
                d.Title = "PEA — F-5E";
            });

            await AddDocIfAircraftExistsAsync("CN-FGH", "LME", d =>
            {
                d.ReferenceNo = "LMER-F5E-2022";
                d.Title = "LME/LTTE — F-5E";
            });

            await AddDocIfAircraftExistsAsync("CN-FGH", "CDL", d =>
            {
                d.ReferenceNo = "CDL-FRA-F5E-2023";
                d.Title = "CDL — F-5E";
            });

            await AddDocIfAircraftExistsAsync("CN-ABD", "CDN", d =>
            {
                d.ReferenceNo = "DAM/CN/2023/0056";
                d.Title = "CdN — CN-ABD";
                d.ValidUntilUtc = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-ABD", "CEN", d =>
            {
                d.ReferenceNo = "DAM/CEN/2026/0008";
                d.Title = "CEN — CN-ABD";
                d.IssuedAtUtc = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
                d.ValidUntilUtc = new DateTime(2027, 2, 15, 0, 0, 0, DateTimeKind.Utc);
            });

            await AddDocIfAircraftExistsAsync("CN-ABD", "PEA", d =>
            {
                d.ReferenceNo = "PEA-FRA-SA330-Rev03";
                d.Title = "PEA — SA 330";
            });

            await AddDocIfAircraftExistsAsync("CN-ABD", "LME", d =>
            {
                d.ReferenceNo = "LTTE-FRA-SA330-01";
                d.Title = "LTTE — SA 330";
            });

            await context.SaveChangesAsync();
        }
    }
}