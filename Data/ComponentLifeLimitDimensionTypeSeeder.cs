using Microsoft.EntityFrameworkCore;
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;

namespace FRAProject.Data.Seeders
{
    /// <summary>
    /// NEW (Revision 13). Seeds the 5 life-limit dimensions the module ships
    /// with — FH / Cycles / CalendarDays / TgoLandings / FullStopLandings —
    /// as rows in the new generic ComponentLifeLimitDimensionType lookup.
    ///
    /// These 5 Codes are a STABLE CONTRACT: IAircraftReadingProvider and
    /// ComponentLifeStatusCalculator switch on Code (never Id) for the one
    /// piece of special-casing that still exists (CALENDAR_DAYS is computed
    /// from dates, not from ComponentEvent/ComponentInitialReading readings —
    /// see IsCalendarBased below). Do not rename these Codes once real data
    /// exists; add a NEW row with a new Code for a future counter instead
    /// (C130 APU starts, Canadair/CL-215-415 "number of Drops", etc.) — that
    /// is the entire point of Revision 13: a new dimension is a seeded row,
    /// not a migration.
    ///
    /// Per-code idempotent (never a table-wide AnyAsync() guard — see the
    /// "seeders must be per-code idempotent" lesson from the WorkOrder
    /// session / ComponentPositionSeeder).
    /// </summary>
    public static class ComponentLifeLimitDimensionTypeSeeder
    {
        private static readonly (string Code, string Name, ComponentLifeLimitDimensionUnit Unit, bool IsCalendarBased, byte Sort)[] Dimensions = new[]
        {
            ("FH", "Heures de vol", ComponentLifeLimitDimensionUnit.Hours, false, (byte)1),
            ("CYCLES", "Cycles", ComponentLifeLimitDimensionUnit.Count, false, (byte)2),
            ("CALENDAR_DAYS", "Jours calendaires", ComponentLifeLimitDimensionUnit.Days, true, (byte)3),
            ("TGO_LANDINGS", "Atterrissages T&G", ComponentLifeLimitDimensionUnit.Count, false, (byte)4),
            ("FULLSTOP_LANDINGS", "Atterrissages complets", ComponentLifeLimitDimensionUnit.Count, false, (byte)5),
        };

        public static async Task SeedAsync(FRAContext context)
        {
            foreach (var (code, name, unit, isCalendarBased, sort) in Dimensions)
            {
                var exists = await context.Set<ComponentLifeLimitDimensionType>()
                    .AnyAsync(d => d.Code == code);
                if (exists) continue;

                context.Set<ComponentLifeLimitDimensionType>().Add(new ComponentLifeLimitDimensionType
                {
                    Code = code,
                    Name = name,
                    Unit = unit,
                    IsCalendarBased = isCalendarBased,
                    IsActive = true,
                    SortOrder = sort
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
