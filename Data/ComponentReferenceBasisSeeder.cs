using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;

namespace FRAProject.Data.Seeders
{
    /// <summary>
    /// NEW. Seeds the 4 reference-basis rows every dimension can be measured
    /// from — see ComponentReferenceBasis.cs for what each Code means.
    /// SINCE_NEW is the implicit default (matches every profile's pre-existing
    /// behavior) — a stage-dimension row with no ReferenceBasisId set behaves
    /// exactly as it did before this feature existed, so this seeder does not
    /// need to run before existing profiles keep working; it only needs to
    /// run before anyone picks a NON-default basis in the profile editor.
    ///
    /// Per-code idempotent (never a table-wide AnyAsync() guard — see the
    /// "seeders must be per-code idempotent" lesson from the WorkOrder
    /// session / ComponentPositionSeeder / ComponentLifeLimitDimensionTypeSeeder).
    /// </summary>
    public static class ComponentReferenceBasisSeeder
    {
        private static readonly (string Code, string Name, byte Sort)[] Bases = new[]
        {
            ("SINCE_NEW", "Depuis neuf", (byte)1),
            ("SINCE_OVERHAUL", "Depuis révision", (byte)2),
            ("SINCE_INSTALL", "Depuis installation (en cours)", (byte)3),
            ("SINCE_FIRST_INSTALL", "Depuis première mise en service", (byte)4),
        };

        public static async Task SeedAsync(FRAContext context)
        {
            foreach (var (code, name, sort) in Bases)
            {
                var exists = await context.Set<ComponentReferenceBasis>()
                    .AnyAsync(b => b.Code == code);
                if (exists) continue;

                context.Set<ComponentReferenceBasis>().Add(new ComponentReferenceBasis
                {
                    Code = code,
                    Name = name,
                    IsActive = true,
                    SortOrder = sort
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
