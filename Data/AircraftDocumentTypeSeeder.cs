using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AircraftDocumentTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // If already seeded, skip
            if (await context.Set<AircraftDocumentType>().AnyAsync())
                return;

            var types = new List<AircraftDocumentType>
            {
                new() { Code = "CDN", Name = "Certificat de navigabilité (CdN)", IsActive = true },
                new() { Code = "CEN", Name = "Certificat d'examen de navigabilité (CEN)", IsActive = true },
                new() { Code = "PEA", Name = "Programme d'entretien aéronef (PEA)", IsActive = true },
                new() { Code = "LME", Name = "Liste minimale d'équipements (LME / LTTE)", IsActive = true },
                new() { Code = "CDL", Name = "Configuration Deviation List (CDL)", IsActive = true },

                // Optional: useful future expansion
                new() { Code = "CN",  Name = "Consigne de navigabilité (CN / AD)", IsActive = true },
                new() { Code = "SB",  Name = "Service Bulletin / Modification", IsActive = true },
            };

            await context.Set<AircraftDocumentType>().AddRangeAsync(types);
            await context.SaveChangesAsync();
        }
    }
}