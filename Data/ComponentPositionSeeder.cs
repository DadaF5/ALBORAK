using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models; // AcType
using FRAProject.Data;

namespace FRAProject.Data.Seeders
{
    /// <summary>
    /// Generic placeholder positions — NOT verified against your real fleet's
    /// terminology (per your own choice to seed a few common ones rather than
    /// leave this empty). Same spirit as the placeholder AJET/F5F aircraft
    /// registrations seeded in the WorkOrder session: expected to be corrected
    /// via the UI once you confirm real position names/codes per AcType.
    /// Per-code idempotent (never a table-wide AnyAsync() guard — see the
    /// "seeders must be per-code idempotent" lesson from the WorkOrder session).
    /// </summary>
    public static class ComponentPositionSeeder
    {
        private static readonly (string Code, string Name, byte Sort)[] Common = new[]
        {
            ("ENG1", "Moteur #1", (byte)1),
            ("ENG2", "Moteur #2", (byte)2),   // skip for single-engine AcTypes below
            ("APU", "APU", (byte)3),
            ("NLG", "Train avant", (byte)4),
            ("LH-MLG", "Train principal gauche", (byte)5),
            ("RH-MLG", "Train principal droit", (byte)6),
        };

        // ASSUMPTION: adjust this set to your real AcType codes (per
        // AcTypeSeeder: F16C, F16D, C130H, F5E, F5F, AJET). Single-engine types
        // (F16*, F5*, AJET) skip ENG2; C130H (4 engines) is intentionally left
        // for manual correction rather than guessing ENG3/ENG4 here.
        private static readonly Dictionary<string, bool> SingleEngineByAcTypeCode = new()
        {
            ["F16C"] = true,
            ["F16D"] = true,
            ["F5E"] = true,
            ["F5F"] = true,
            ["AJET"] = true,
            ["C130H"] = false,
        };

        public static async Task SeedAsync(FRAContext context)
        {
            var acTypes = await context.Set<AcType>().ToListAsync();

            foreach (var acType in acTypes)
            {
                var isSingleEngine = SingleEngineByAcTypeCode.TryGetValue(acType.Code ?? "", out var single) && single;

                foreach (var (code, name, sort) in Common)
                {
                    if (isSingleEngine && code == "ENG2") continue;

                    var exists = await context.Set<ComponentPosition>()
                        .AnyAsync(p => p.AcTypeId == acType.Id && p.Code == code);
                    if (exists) continue;

                    context.Set<ComponentPosition>().Add(new ComponentPosition
                    {
                        AcTypeId = acType.Id,
                        Code = code,
                        Name = name,
                        IsActive = true,
                        SortOrder = sort
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
