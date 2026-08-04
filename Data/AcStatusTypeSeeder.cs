using FRAProject.Areas.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class AcStatusTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<AcStatusType>().AnyAsync())
                return;

            // Seed data per AcStatusType.cs XML doc comment.
            // Codes also drive Aircraft.StatusBadgeClass — keep these exact codes.
            var statuses = new List<AcStatusType>
            {
                new() { Code = "OPR", Name = "Opérationnel",       Description = "Aéronef disponible pour la mission", SortOrder = 1, IsActive = true },
                new() { Code = "MNT", Name = "En maintenance",     Description = "Aéronef en cours d'entretien",       SortOrder = 2, IsActive = true },
                new() { Code = "AOG", Name = "Aircraft on Ground", Description = "Aéronef immobilisé — panne critique", SortOrder = 3, IsActive = true },
                new() { Code = "STK", Name = "En stockage",        Description = "Aéronef stocké — hors service actif", SortOrder = 4, IsActive = true },
                new() { Code = "RAD", Name = "Radié",              Description = "Aéronef radié du registre",          SortOrder = 5, IsActive = true },
            };

            await context.Set<AcStatusType>().AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }
    }
}