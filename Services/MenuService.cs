using FRAProject.Data;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FRAProject.Services
{
    public class MenuService : IMenuService
    {
        private readonly FRAContext _context;

        public MenuService(FRAContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuItem>> GetMenuForUserAsync(ClaimsPrincipal user)
        {
            // Load all items fresh
            var items = await _context.MenuItems
                .AsNoTracking()
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();

            // Optionally apply basic role filtering at the item-level.
            bool AllowedByRoles(MenuItem mi)
            {
                if (string.IsNullOrWhiteSpace(mi.Roles)) return true;
                var needed = mi.Roles.Split(',').Select(r => r.Trim());
                return needed.Any(r => user.IsInRole(r));
            }

            // Build a tree of MenuItem DTOs (create new objects so ViewModel isn't tracked)
            List<MenuItem> BuildTree(int? parentId)
            {
                return items
                    .Where(m => m.ParentId == parentId && AllowedByRoles(m))
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Id)
                    .Select(m => new MenuItem
                    {
                        Id = m.Id,
                        Title = m.Title,
                        IconClass = m.IconClass,
                        Controller = m.Controller,
                        Action = m.Action,
                        Url = m.Url,
                        ParentId = m.ParentId,
                        SortOrder = m.SortOrder,
                        DepartmentId = m.DepartmentId,
                        BaseId = m.BaseId,
                        Roles = m.Roles,
                        Children = BuildTree(m.Id)
                    })
                    .ToList();
            }

            return BuildTree(null);
        }
    }
}
