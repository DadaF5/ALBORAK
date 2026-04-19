using FRAProject.Areas.AircraftMaintenance.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public static class AircraftDocumentTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            // Desired reference set (extend anytime)
            var desired = new List<AircraftDocumentType>
            {
                new() { Code = "CDN",  Name = "Certificat de navigabilité (CdN)", IsActive = true },
                new() { Code = "CEN",  Name = "Certificat d’examen de navigabilité (CEN)", IsActive = true },
                new() { Code = "PEA",  Name = "Programme d’entretien aéronef (PEA)", IsActive = true },
                new() { Code = "LME",  Name = "Liste minimale d’équipements (LME / LMER)", IsActive = true },
                new() { Code = "LTTE", Name = "Liste type de tolérance d’entretien (LTTE)", IsActive = true },
                new() { Code = "CDL",  Name = "Configuration Deviation List (CDL)", IsActive = true },

                // Optional buckets for later
                new() { Code = "CN",   Name = "Consigne de navigabilité (CN)", IsActive = true },
                new() { Code = "SB",   Name = "Service Bulletin (SB) / Modifications", IsActive = true },
            };

            // Load existing types once
            var existing = await context.AircraftDocumentTypes
                .ToDictionaryAsync(t => t.Code);

            var anyChanges = false;

            foreach (var item in desired)
            {
                if (existing.TryGetValue(item.Code, out var current))
                {
                    // Update Name/IsActive if changed (keeps same Id)
                    if (!string.Equals(current.Name, item.Name, StringComparison.Ordinal))
                    {
                        current.Name = item.Name;
                        anyChanges = true;
                    }

                    if (current.IsActive != item.IsActive)
                    {
                        current.IsActive = item.IsActive;
                        anyChanges = true;
                    }
                }
                else
                {
                    await context.AircraftDocumentTypes.AddAsync(item);
                    anyChanges = true;
                }
            }

            if (anyChanges)
                await context.SaveChangesAsync();
        }
    }
}