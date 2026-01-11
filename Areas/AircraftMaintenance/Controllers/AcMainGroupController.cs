using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels.AcMainGroup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class AcMainGroupController : Controller
    {
        private readonly FRAContext _context;

        public AcMainGroupController(FRAContext context)
        {
            _context = context;
        }

        // Get: AcMainGroup
        public async Task<IActionResult> Index(int? baseId, int? categoryId)
        {
            var query = _context.AcMainGroups
                .Include(m => m.AcCategory)
                .Include(m => m.Base)
                .AsQueryable();

            if (baseId.HasValue)
                query = query.Where(m => m.BaseId == baseId);

            if (categoryId.HasValue)
                query = query.Where(m => m.AcCategoryId == categoryId);

            // Populate dropdowns
            ViewBag.Bases = new SelectList(await _context.Bases.OrderBy(b => b.BaseName).ToListAsync(), "Id", "BaseName", baseId);
            ViewBag.Categories = new SelectList(await _context.AcCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);

            var mainGroups = await query.OrderBy(m => m.Name).ToListAsync();
            return View(mainGroups);
        }
        // GET: AcMainGroup/Create
        public async Task<IActionResult> Create()
        {
            var model = new AcMainGroupViewModel
            {
                Categories = await _context.AcCategories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync(),

                Bases = await _context.Bases
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync()
            };

            return View(model);
        }

        // POST: AcMainGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcMainGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns on error
                model.Categories = await _context.AcCategories
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync();

                model.Bases = await _context.Bases
                    .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                    .ToListAsync();

                return View(model);
            }

            var entity = new AcMainGroup
            {
                Name = model.Name,
                AcCategoryId = model.AcCategoryId,
                BaseId = model.BaseId,
                Description = model.Description,
                Active = model.IsActive
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: AcMainGroup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var entity = await _context.AcMainGroups.FindAsync(id);

            if (entity == null)
                return NotFound();

            var vm = new AcMainGroupViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                AcCategoryId = entity.AcCategoryId,
                BaseId = entity.BaseId,
                IsActive = entity.Active
            };

            vm.Categories = await _context.AcCategories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = (c.Id == vm.AcCategoryId)  // FIXED
                })
                .ToListAsync();

            vm.Bases = await _context.Bases
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BaseName,
                    Selected = (b.Id == vm.BaseId) // FIXED
                })
                .ToListAsync();

            return View(vm);
        }


        // POST: AcMainGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcMainGroupViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Rebuild dropdowns when validation fails
                vm.Categories = await _context.AcCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToListAsync();

                vm.Bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.BaseName
                    }).ToListAsync();

                return View(vm);
            }

            // Load entity to update
            var entity = await _context.AcMainGroups.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound();

            // Update the entity
            entity.Name = vm.Name;
            entity.Description = vm.Description;
            entity.AcCategoryId = vm.AcCategoryId;
            entity.BaseId = vm.BaseId;
            entity.Active = vm.IsActive;

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AcMainGroups.Any(e => e.Id == vm.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AcMainGroupExists(int id) => _context.AcMainGroups.Any(e => e.Id == id);
    }
}