using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data
{
    public class ImmatriculationDocTypeSeeder
    {
        public static async Task SeedAsync(FRAContext context)
        {
            if (await context.Set<ImmatriculationDocType>().AnyAsync())
                return;

            // Seed data per ImmatriculationDocType.cs XML doc — fixed by
            // GUI-DPC-001 Art. 15. Never hard-delete these rows.
            var docTypes = new List<ImmatriculationDocType>
            {
                new() { Code = "DOC01", Name = "Justificatif de propriété",         ArticleReference = "Art. 15.1", IsRequired = true,  AcceptedFormats = "PDF",         SortOrder = 1, IsActive = true },
                new() { Code = "DOC02", Name = "Photo plaque signalétique",         ArticleReference = "Art. 15.2", IsRequired = true,  AcceptedFormats = "JPG,PNG",     SortOrder = 2, IsActive = true },
                new() { Code = "DOC03", Name = "Certificat de radiation étranger",  ArticleReference = "Art. 15.3", IsRequired = false, AcceptedFormats = "PDF",         SortOrder = 3, IsActive = true },
                new() { Code = "DOC04", Name = "Copie contrat d'assurance",         ArticleReference = "Art. 15.4", IsRequired = true,  AcceptedFormats = "PDF",         SortOrder = 4, IsActive = true },
                new() { Code = "DOC05", Name = "Certificat de navigabilité / AdV",  ArticleReference = "Art. 15.5", IsRequired = false, AcceptedFormats = "PDF",         SortOrder = 5, IsActive = true },
                new() { Code = "DOC06", Name = "Documents de dédouanement",         ArticleReference = "Art. 15.6", IsRequired = false, AcceptedFormats = "PDF,JPG,PNG", SortOrder = 6, IsActive = true },
            };

            await context.Set<ImmatriculationDocType>().AddRangeAsync(docTypes);
            await context.SaveChangesAsync();
        }
    }
}