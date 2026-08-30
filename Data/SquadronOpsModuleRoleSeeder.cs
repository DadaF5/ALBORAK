// Data/Seeders/SquadronOpsModuleRoleSeeder.cs
//
// NEW (Batch 11, 2026-08-29). Seeds the three SQUADRONOPS ModuleRole rows
// needed for DATA-SCOPE filtering (via UserAssignment/UserScope) — separate
// from, and in addition to, the ACTION-GATING Identity roles
// (SquadronPlanner/CrewChief/Tower) already seeded by IdentitySeed.cs. See
// SortiesController.cs's class-level comment for the full split.
//
// Follows the same pattern as every other real reference seeder called from
// Program.cs's seeding pipeline (PhaseSeeder, MissionSeeder, etc.) — static
// class, idempotent (checks for existing rows by RoleCode before inserting),
// takes FRAContext directly.
//
// Module "SQUADRONOPS" itself is already confirmed seeded (Module.cs's own
// doc comment lists it) — this only adds the ModuleRole children.
using FRAProject.Data;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Data.Seeders
{
    public static class SquadronOpsModuleRoleSeeder
    {
        private const string ModuleCode = "SQUADRONOPS";

        public static async Task SeedAsync(FRAContext context)
        {
            var existing = await context.Set<ModuleRole>()
                .Where(r => r.ModuleCode == ModuleCode)
                .Select(r => r.RoleCode)
                .ToListAsync();

            var toAdd = new List<ModuleRole>();

            // Squadron Planner — "plans odv with sortie(s) for a given
            // squadron within Wing within a base for aircraftMaingroup".
            // Full three-dimension scope: Base + AcMainGroup + Wing.
            if (!existing.Contains("SQUADRON_PLANNER"))
            {
                toAdd.Add(new ModuleRole
                {
                    ModuleCode = ModuleCode,
                    RoleCode = "SQUADRON_PLANNER",
                    RoleName = "Planificateur Escadron",
                    Description = "Plans ODVs/Sorties for their own Squadron; scoped by AcMainGroup and Wing within a Base.",
                    CanWrite = true,
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = true,
                    SortOrder = 10
                });
            }

            // ATC / Tower — Base-only scope, per Dadda's confirmed shape.
            // RoleCode kept as "ATC" for data-scope clarity even though the
            // Identity role it pairs with for action-gating is seeded as
            // "Tower" (IdentitySeed.cs) — Dadda confirmed these are the
            // same real-world role, just named differently in the two
            // systems. Do not rename either without updating both.
            if (!existing.Contains("ATC"))
            {
                toAdd.Add(new ModuleRole
                {
                    ModuleCode = ModuleCode,
                    RoleCode = "ATC",
                    RoleName = "Contrôle Aérien (Tower)",
                    Description = "Records engine start/TOFF (departure) and real landing time/airfield activity (arrival). Base-only scope — sees every squadron/group at their assigned base(s).",
                    CanWrite = true,
                    ShowBaseScope = true,
                    ShowGroupScope = false,
                    ShowWingScope = false,
                    SortOrder = 20
                });
            }

            // CrewChief — Base + AcMainGroup scope (their aircraft group),
            // no Wing dimension.
            if (!existing.Contains("CREWCHIEF"))
            {
                toAdd.Add(new ModuleRole
                {
                    ModuleCode = ModuleCode,
                    RoleCode = "CREWCHIEF",
                    RoleName = "Chef d'Équipe",
                    Description = "Assigns aircraft to sorties and records post-flight data (fuel/oil/snag). Scoped by AcMainGroup within a Base.",
                    CanWrite = true,
                    ShowBaseScope = true,
                    ShowGroupScope = true,
                    ShowWingScope = false,
                    SortOrder = 30
                });
            }

            if (toAdd.Count > 0)
            {
                context.Set<ModuleRole>().AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
