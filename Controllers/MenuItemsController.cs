using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Models;

namespace FRAProject.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly FRAContext _context;

        public MenuItemsController(FRAContext context)
        {
            _context = context;
        }

        // GET: /MenuItems
        public async Task<IActionResult> Index()
        {
            var items = await _context.MenuItems
                .AsNoTracking()
                .OrderBy(m => m.ParentId)
                .ThenBy(m => m.SortOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();

            var dict = items.ToDictionary(m => m.Id, m => m.Title);

            var vm = items.Select(m => new ViewModels.MenuItemAdminViewModel
            {
                MenuItem = m,
                ParentTitle = m.ParentId.HasValue && dict.ContainsKey(m.ParentId.Value) ? dict[m.ParentId.Value] : null
            }).ToList();

            return View(vm);
        }

        // GET: /MenuItems/Create
        public async Task<IActionResult> Create()
        {
            var parents = await _context.MenuItems
                .AsNoTracking()
                .OrderBy(m => m.Title)
                .ToListAsync();

            var parentList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "(root)" }
            };
            parentList.AddRange(parents.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title }));

            ViewData["ParentList"] = parentList;
            return View();
        }

        // POST: /MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,IconClass,Controller,Action,Url, Area ,ParentId,SortOrder,DepartmentId,BaseId,Roles")] MenuItem menuItem)
        {
            if (!ModelState.IsValid)
            {
                var parents = await _context.MenuItems.AsNoTracking().OrderBy(m => m.Title).ToListAsync();
                var parentList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "(root)" } };
                parentList.AddRange(parents.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title }));
                ViewData["ParentList"] = parentList;
                return View(menuItem);
            }

            // if sortorder omitted, set to 0
            if (menuItem.SortOrder == 0)
                menuItem.SortOrder = 100;

            _context.Add(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id.Value);
            if (menuItem == null) return NotFound();

            var parents = await _context.MenuItems
                .AsNoTracking()
                .Where(m => m.Id != id.Value) // exclude self
                .OrderBy(m => m.Title)
                .ToListAsync();

            var parentList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "(root)" } };
            parentList.AddRange(parents.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Title,
                Selected = menuItem.ParentId.HasValue && menuItem.ParentId.Value == p.Id
            }));

            ViewData["ParentList"] = parentList;
            return View(menuItem);
        }

        // POST: /MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IconClass,Controller,Action,Url, Area ,ParentId,SortOrder,DepartmentId,BaseId,Roles")] MenuItem menuItem)
        {
            if (id != menuItem.Id) return NotFound();

            // prevent self parenting
            if (menuItem.ParentId.HasValue && menuItem.ParentId.Value == menuItem.Id)
            {
                ModelState.AddModelError("ParentId", "A menu item cannot be its own parent.");
            }

            if (!ModelState.IsValid)
            {
                var parents = await _context.MenuItems.AsNoTracking().Where(m => m.Id != id).OrderBy(m => m.Title).ToListAsync();
                var parentList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "(root)" } };
                parentList.AddRange(parents.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Title,
                    Selected = menuItem.ParentId.HasValue && menuItem.ParentId.Value == p.Id
                }));
                ViewData["ParentList"] = parentList;
                return View(menuItem);
            }

            try
            {
                var dbEntity = await _context.MenuItems.FindAsync(id);
                if (dbEntity == null) return NotFound();

                // update fields
                dbEntity.Title = menuItem.Title;
                dbEntity.IconClass = menuItem.IconClass;
                dbEntity.Controller = menuItem.Controller;
                dbEntity.Action = menuItem.Action;
                dbEntity.Url = menuItem.Url;
                dbEntity.Area = menuItem.Area;
                dbEntity.ParentId = menuItem.ParentId;
                dbEntity.SortOrder = menuItem.SortOrder;
                dbEntity.DepartmentId = menuItem.DepartmentId;
                dbEntity.BaseId = menuItem.BaseId;
                dbEntity.Roles = menuItem.Roles;

                _context.Update(dbEntity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(menuItem.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (menuItem == null) return NotFound();

            var hasChildren = await _context.MenuItems.AnyAsync(m => m.ParentId == id.Value);
            ViewData["HasChildren"] = hasChildren;

            return View(menuItem);
        }

        // POST: /MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            var hasChildren = await _context.MenuItems.AnyAsync(m => m.ParentId == id);
            if (hasChildren)
            {
                // Block deleting items that have children
                ModelState.AddModelError(string.Empty, "Cannot delete a menu item that has child menu items. Reassign or delete children first.");
                ViewData["HasChildren"] = true;
                return View("Delete", menuItem);
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}