using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FRAProject.Models;

namespace FRAProject.Services
{
    // Simple in-memory menu provider: replace with EF queries later.
    public class SampleMenuService : IMenuService
    {
        public Task<IEnumerable<MenuItem>> GetMenuForUserAsync(ClaimsPrincipal user)
        {
            // Build hierarchical menu
            var squadron = new MenuItem
            {
                Title = "Squadron",
                IconClass = "fa fa-fighter-jet",
                Children = new List<MenuItem>
                {
                    new MenuItem { Title = "Create ODV", Controller = "Odv", Action = "Create" },
                    new MenuItem { Title = "Pilot Logbook", Controller = "PilotLog", Action = "Index" },
                    new MenuItem { Title = "Update Sortie", Controller = "Sortie", Action = "Edit" },
                }
            };

            var crewchief = new MenuItem
            {
                Title = "CrewChief",
                IconClass = "fas fa-user-cog",
                Children = new List<MenuItem>
                {
                    new MenuItem { Title = "Assign Aircraft", Controller = "CrewChief", Action = "AssignAircraft" },
                    new MenuItem { Title = "Report Malfunction", Controller = "CrewChief", Action = "ReportMalfunction" },
                    new MenuItem { Title = "Maintenance Log", Controller = "CrewChief", Action = "MaintenanceLog" }
                }
            };

            // other top-level menus (examples)
            var aircraft = new MenuItem
            {
                Title = "Aircraft",
                IconClass = "fa fa-plane",
                Children = new List<MenuItem>
                {
                    new MenuItem { Title = "List", Controller = "Aircraft", Action = "Index" },
                    new MenuItem { Title = "Create", Controller = "Aircraft", Action = "Create" }
                }
            };

            var admin = new MenuItem
            {
                Title = "Administration",
                IconClass = "fa fa-cogs",
                Roles = "Admin", // sample role gating
                Children = new List<MenuItem>
                {
                    new MenuItem { Title = "Users", Controller = "Admin", Action = "Users" },
                    new MenuItem { Title = "Roles", Controller = "Admin", Action = "Roles" }
                }
            };

            var items = new[] { squadron, crewchief, aircraft, admin };

            // Filter by roles if Roles property set (simple comma-separated match)
            var filtered = items.Where(mi =>
            {
                if (string.IsNullOrWhiteSpace(mi.Roles)) return true;
                var needed = mi.Roles!.Split(',').Select(r => r.Trim());
                return needed.Any(r => user.IsInRole(r));
            });

            return Task.FromResult(filtered.AsEnumerable());
        }
    }
}