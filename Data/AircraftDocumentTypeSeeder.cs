// Data/AircraftDocumentTypeSeeder.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public static class AircraftDocumentTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // If you want to be stricter: check for a specific code instead of Any()
            if (await context.Set<AircraftDocumentType>().AnyAsync())
                return;

            var types = new List<AircraftDocumentType>
            {
                new() { Code = "CDN", Name = "Certificat de navigabilité (CdN)", IsActive = true },
                new() { Code = "CEN", Name = "Certificat d'examen de navigabilité (CEN)", IsActive = true },
                new() { Code = "PEA", Name = "Programme d'entretien aéronef (PEA)", IsActive = true },
                new() { Code = "LME", Name = "Liste minimale d'équipements (LME/LTTE)", IsActive = true },
                new() { Code = "CDL", Name = "Configuration Deviation List (CDL)", IsActive = true },

                // Optional extras (useful later)
                new() { Code = "SB",  Name = "Service Bulletin (SB)", IsActive = true },
                new() { Code = "CN",  Name = "Consigne de Navigabilité (CN)", IsActive = true },
            };

            context.AddRange(types);
            await context.SaveChangesAsync();
        }
    }
}