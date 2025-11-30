using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;

namespace FRAProject.Controllers
{
    public class WingsController : Controller
    {
        private readonly FRAContext _context;

        public WingsController(FRAContext context)
        {
            _context = context;
        }

        // GET: /Wing
        public async Task<IActionResult> Index(int? departmentId, int? baseId, int? acMainGroupId, bool includeInactive = false)
        {
            var departments = await _context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
            var bases = await _context.Bases.AsNoTracking().OrderBy(b => b.BaseName).ToListAsync();
            var acMainGroups = await _context.AcMainGroups.AsNoTracking().OrderBy(a => a.Name).ToListAsync();

            // These ViewData keys are used by the Index view DropDownList helpers
            ViewData["Departments"] = new SelectList(departments, "Id", "Name", departmentId);
            ViewData["Bases"] = new SelectList(bases, "Id", "BaseName", baseId);
            ViewData["AcMainGroups"] = new SelectList(acMainGroups, "Id", "Name", acMainGroupId);

            var q = _context.Wings
                .AsNoTracking()
                .Include(w => w.Department)
                .Include(w => w.Base)
                .Include(w => w.AcMainGroup)
                .AsQueryable();

            if (!includeInactive)
                q = q.Where(w => w.Active);

            if (departmentId.HasValue)
                q = q.Where(w => w.DepartmentId == departmentId.Value);

            if (baseId.HasValue)
                q = q.Where(w => w.BaseId == baseId.Value);

            if (acMainGroupId.HasValue)
                q = q.Where(w => w.AcMainGroupId == acMainGroupId.Value);

            var wings = await q.OrderBy(w => w.Name).ToListAsync();

            var vmList = wings.Select(w => new WingViewModel
            {
                Id = w.Id,
                Name = w.Name,
                WingLong = w.WingLong,
                DepartmentId = w.DepartmentId,
                DepartmentName = w.Department?.Name ?? "",
                AcMainGroupId = w.AcMainGroupId,
                // populate the VM property that your view expects (AcMainGroupName or AcMainGroup)
                
                AcMainGroupName = w.AcMainGroup?.Name ?? "",
                BaseId = w.BaseId,
                BaseName = w.Base?.BaseName ?? "",
                Active = w.Active
            }).ToList();

            return View(vmList);
        }

        // GET: /Wing/Create
        public async Task<IActionResult> Create()
        {
            var vm = new WingViewModel();
            await PopulateSelects(vm);
            return View(vm);
        }

        // POST: /Wing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelects(vm);
                return View(vm);
            }

            var exists = await _context.Wings.AnyAsync(x => x.Name == vm.Name && x.DepartmentId == vm.DepartmentId);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name), "A wing with this short name already exists in the selected department.");
                await PopulateSelects(vm);
                return View(vm);
            }

            var wing = new Wing
            {
                Name = vm.Name,
                WingLong = vm.WingLong,
                DepartmentId = vm.DepartmentId,
                AcMainGroupId = vm.AcMainGroupId,
                BaseId = vm.BaseId,
                Active = vm.Active
            };

            _context.Wings.Add(wing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Wing/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var w = await _context.Wings
                .AsNoTracking()
                .Include(x => x.Base)
                .Include(x => x.Department)
                .Include(x => x.AcMainGroup)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (w == null) return NotFound();

            var vm = new WingViewModel
            {
                Id = w.Id,
                Name = w.Name,
                WingLong = w.WingLong,
                DepartmentId = w.DepartmentId,
                DepartmentName = w.Department?.Name ?? "",
                AcMainGroupId = w.AcMainGroupId,               
                AcMainGroupName = w.AcMainGroup?.Name ?? "",
                BaseId = w.BaseId,
                BaseName = w.Base?.BaseName ?? "",
                Active = w.Active
            };

            await PopulateSelects(vm);
            return View(vm);
        }

        // POST: /Wing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelects(vm);
                return View(vm);
            }

            var w = await _context.Wings.FindAsync(vm.Id);
            if (w == null) return NotFound();

            var dup = await _context.Wings.AnyAsync(x => x.Id != vm.Id && x.Name == vm.Name && x.DepartmentId == vm.DepartmentId);
            if (dup)
            {
                ModelState.AddModelError(nameof(vm.Name), "A wing with this short name already exists in the selected department.");
                await PopulateSelects(vm);
                return View(vm);
            }

            w.Name = vm.Name;
            w.WingLong = vm.WingLong;
            w.DepartmentId = vm.DepartmentId;
            w.AcMainGroupId = vm.AcMainGroupId;
            w.BaseId = vm.BaseId;
            w.Active = vm.Active;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Wing/GetDepartmentsByBase?baseId=3
        // Returns JSON array: [{ id = 1, name = "Dept A" }, ...]
        [HttpGet]
        public async Task<IActionResult> GetDepartmentsByBase(int? baseId)
        {
            if (!baseId.HasValue)
            {
                var allDeps = await _context.Departments
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .Select(d => new { id = d.Id, name = d.Name })
                    .ToListAsync();
                return Json(allDeps);
            }

            var deps = await _context.Departments
                .AsNoTracking()
                .Where(d => d.BaseId == baseId.Value)
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();

            return Json(deps);
        }

        // Populate select lists for Create/Edit
        private async Task PopulateSelects(WingViewModel vm)
        {
            var departments = await _context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
            var bases = await _context.Bases.AsNoTracking().OrderBy(b => b.BaseName).ToListAsync();
            var acMainGroups = await _context.AcMainGroups.AsNoTracking().OrderBy(a => a.Name).ToListAsync();

            // Use these ViewData keys in the view: "Departments", "Bases", "AcMainGroups"
            vm.Departments = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == vm.DepartmentId)).ToList();
            vm.Bases = bases.Select(b => new SelectListItem(b.BaseName, b.Id.ToString(), b.Id == vm.BaseId)).ToList();
            vm.AcMainGroups = acMainGroups.Select(a => new SelectListItem(a.Name, a.Id.ToString(), a.Id == vm.AcMainGroupId)).ToList();

            ViewData["Departments"] = new SelectList(departments, "Id", "Name", vm.DepartmentId);
            ViewData["Bases"] = new SelectList(bases, "Id", "BaseName", vm.BaseId);
            ViewData["AcMainGroups"] = new SelectList(acMainGroups, "Id", "Name", vm.AcMainGroupId);
        }
    }
}