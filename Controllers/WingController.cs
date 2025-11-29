using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace FRAProject.Controllers
{
    public class WingController : Controller
    {
        private readonly FRAContext _context;

        public WingController(FRAContext context)
        {
            _context = context;
        }

        // ===== INDEX =====
        public async Task<IActionResult> Index(int? departmentId, int? acMainGroupId)
        {
            var query = _context.Wings
                .Include(w => w.Department)
                .Include(w => w.AcMainGroup)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(w => w.DepartmentId == departmentId.Value);
            }

            if (acMainGroupId.HasValue)
            {
                query = query.Where(w => w.AcMainGroupId == acMainGroupId.Value);
            }

            var wings = await query.OrderBy(w => w.Name).ToListAsync();

            var model = wings.Select(w => new WingViewModel
            {
                Id = w.Id,
                Name = w.Name,
                WingLong = w.WingLong,
                DepartmentId = w.DepartmentId,
                DepartmentName = w.Department?.Name ?? "",
                AcMainGroupId = w.AcMainGroupId,
                AcMainGroupName = w.AcMainGroup?.Name ?? "",
                Active = w.Active
            }).ToList();

            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", departmentId);
            ViewData["AcMainGroups"] = new SelectList(await _context.AcMainGroups.ToListAsync(), "Id", "Name", acMainGroupId);

            return View(model);
        }


        // ===== INDEX =====
        public async Task<IActionResult> Table(int? departmentId, int? acMainGroupId, string? search)
        {
            var query = _context.Wings
                .Include(w => w.Department)
                .Include(w => w.AcMainGroup)
                .Include(w => w.Squadrons)
                .AsQueryable();

            if (departmentId.HasValue) query = query.Where(w => w.DepartmentId == departmentId.Value);
            if (acMainGroupId.HasValue) query = query.Where(w => w.AcMainGroupId == acMainGroupId.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(w => w.Name.Contains(search) || w.WingLong.Contains(search));

            var wings = await query.OrderBy(w => w.Name).ToListAsync();

            var model = wings.Select(w => new WingViewModel
            {
                Id = w.Id,
                Name = w.Name,
                WingLong = w.WingLong,
                DepartmentId = w.DepartmentId,
                DepartmentName = w.Department?.Name ?? "",
                AcMainGroupId = w.AcMainGroupId,
                AcMainGroupName = w.AcMainGroup?.Name ?? "",
                Active = w.Active,
                Squadrons = w.Squadrons?.Select(s => new SquadronViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    WingId = s.WingId,
                    WingName = w.Name,
                    Active = s.Active
                }).ToList()
            }).ToList();
            ViewData["Search"] = search; // set in controller
            return PartialView("_WingTable", model);
        }


        // ===== CREATE GET =====
        public async Task<IActionResult> Create()
        {
            //var model = new WingViewModel
            //{
            //    Departments = await _context.Departments
            //        .OrderBy(d => d.Name)
            //        .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            //        .ToListAsync(),
            //    AcMainGroups = await _context.AcMainGroups
            //        .OrderBy(a => a.Name)
            //        .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
            //        .ToListAsync()
            //};
            //return View(model);

            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            ViewData["AcMainGroups"] = new SelectList(await _context.AcMainGroups.ToListAsync(), "Id", "Name");
            return View();

        }

        // ===== CREATE POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropDowns(model);
                return View(model);
            }

            // === PREVENT DUPLICATE WINGS (Same Name + Same Base + Same Dept) ===
            bool exists = await _context.Wings
                .AnyAsync(w =>
                    w.Name == model.Name &&
                    w.AcMainGroupId == model.AcMainGroupId &&
                    w.DepartmentId == model.DepartmentId
                );

            if (exists)
            {
                ModelState.AddModelError("", "A Wing with the same name already exists in this Base and Department.");
                await LoadDropDowns(model);
                return View(model);
            }

            var wing = new Wing
            {
                Name = model.Name,
                WingLong = model.WingLong,
                DepartmentId = model.DepartmentId,
                AcMainGroupId = model.AcMainGroupId,
                Active = model.Active
            };

            _context.Wings.Add(wing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        private async Task LoadDropDowns(WingViewModel model)
        {
            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", model.DepartmentId);
            ViewData["AcMainGroups"] = new SelectList(await _context.AcMainGroups.ToListAsync(), "Id", "Name", model.AcMainGroupId);
        }

        // ===== EDIT GET =====
       
        public async Task<IActionResult> Edit(int id)
        {
            var wing = await _context.Wings
                .Include(w => w.Department)
                .Include(w => w.AcMainGroup)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wing == null)
            {
                return NotFound();
            }

            var model = new WingViewModel
            {
                Id = wing.Id,
                Name = wing.Name,
                WingLong = wing.WingLong,
                DepartmentId = wing.DepartmentId,
                AcMainGroupId = wing.AcMainGroupId,
                Active = wing.Active,
               
                Departments = await _context.Departments
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToListAsync(),

                AcMainGroups = await _context.AcMainGroups
                    .OrderBy(a => a.Name)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToListAsync()
                        };

            return View(model);
        }


        // ===== EDIT POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WingViewModel model)
        {
            if (id != model.Id) return NotFound();

            // Duplicate check: no same Wing name in same Base + Department
            bool exists = await _context.Wings.AnyAsync(w =>
                w.Id != model.Id &&
                w.Name == model.Name &&
                w.DepartmentId == model.DepartmentId &&
                w.AcMainGroupId == model.AcMainGroupId
            );

            if (exists)
            {
                ModelState.AddModelError("", "A Wing with the same Name already exists in this Base and Department!");
            }

            if (!ModelState.IsValid)
            {
                // IMPORTANT: repopulate dropdowns
                ViewData["Departments"] =
                    new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", model.DepartmentId);

                ViewData["AcMainGroups"] =
                    new SelectList(await _context.AcMainGroups.ToListAsync(), "Id", "Name", model.AcMainGroupId);

                return View(model);
            }

            var wing = await _context.Wings.FindAsync(id);
            if (wing == null) return NotFound();

            wing.Name = model.Name;
            wing.WingLong = model.WingLong;
            wing.DepartmentId = model.DepartmentId;
            wing.AcMainGroupId = model.AcMainGroupId;
            wing.Active = model.Active;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // ===== DETAILS =====
        public async Task<IActionResult> Details(int id)
        {
            var wing = await _context.Wings
                .Include(w => w.Department)
                .Include(w => w.AcMainGroup)
                .Include(w => w.Squadrons) // Include squadrons for display
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wing == null) return NotFound();

            var model = new WingViewModel
            {
                Id = wing.Id,
                Name = wing.Name,
                WingLong = wing.WingLong,
                DepartmentId = wing.DepartmentId,
                DepartmentName = wing.Department?.Name ?? "",
                AcMainGroupId = wing.AcMainGroupId,
                AcMainGroupName = wing.AcMainGroup?.Name ?? "",
                Active = wing.Active,
                Squadrons = wing.Squadrons?.Select(s => new SquadronViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CallSign = s.CallSign,
                    CallSignShort = s.CallSignShort,
                    FrenchName = s.FrenchName,
                    WingId = s.WingId,
                    WingName = wing.Name,
                    LogoPath = s.LogoPath,
                    Active = s.Active
                }).ToList()
            };

            return View(model);
        }

        // ===== DELETE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var wing = await _context.Wings.FindAsync(id);
            if (wing != null)
            {
                _context.Wings.Remove(wing);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
