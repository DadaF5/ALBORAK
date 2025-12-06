using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FRAProject.Models;

namespace FRAProject.Data
{
    public static class MenuSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // create a scope here so callers can pass app.Services or a scope.ServiceProvider
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FRAContext>();

            // Quick short-circuit: if there are many rows you consider "seeded", skip.
            // But we still do an upsert below to cover partially-seeded DBs.
            if (await context.MenuItems.AnyAsync(m => m.Title == "Squadron"))
            {
                // If the main seed marker exists, assume seeded already.
                return;
            }

            // Use a transaction to reduce race-conditions during concurrent startups
            using var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // Helper: find-or-create a parent by title
                async Task<MenuItem> EnsureParentAsync(string title, string iconClass, int sortOrder)
                {
                    var existing = await context.MenuItems.FirstOrDefaultAsync(m => m.Title == title && m.ParentId == null);
                    if (existing != null) return existing;

                    var parent = new MenuItem
                    {
                        Title = title,
                        IconClass = iconClass,
                        SortOrder = sortOrder
                    };
                    context.MenuItems.Add(parent);
                    await context.SaveChangesAsync();
                    return parent;
                }

                // Helper: find-or-create a child by title+parent
                async Task<MenuItem> EnsureChildAsync(MenuItem parent, string title, string? controller, string? action, int sortOrder)
                {
                    var existing = await context.MenuItems.FirstOrDefaultAsync(m =>
                        m.Title == title && m.ParentId == parent.Id);
                    if (existing != null) return existing;

                    var child = new MenuItem
                    {
                        Title = title,
                        Controller = controller,
                        Action = action,
                        ParentId = parent.Id,
                        SortOrder = sortOrder
                    };
                    context.MenuItems.Add(child);
                    await context.SaveChangesAsync();
                    return child;
                }

                // Create parents (if missing)
                var squadron = await EnsureParentAsync("Squadron", "fa fa-fighter-jet", 100);
                var crewChief = await EnsureParentAsync("CrewChief", "fas fa-user-cog", 200);
                var aircraft = await EnsureParentAsync("Aircraft", "fa fa-plane", 300);

                // Create children (if missing)
                await EnsureChildAsync(squadron, "Create ODV", "Odv", "Create", 10);
                await EnsureChildAsync(squadron, "Pilot Logbook", "PilotLog", "Index", 20);
                await EnsureChildAsync(squadron, "Update Sortie", "Sortie", "Edit", 30);

                await EnsureChildAsync(crewChief, "Assign Aircraft", "CrewChief", "AssignAircraft", 10);
                await EnsureChildAsync(crewChief, "Report Malfunction", "CrewChief", "ReportMalfunction", 20);
                await EnsureChildAsync(crewChief, "Maintenance Log", "CrewChief", "MaintenanceLog", 30);

                await EnsureChildAsync(aircraft, "List", "Aircraft", "Index", 10);
                await EnsureChildAsync(aircraft, "Create", "Aircraft", "Create", 20);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}