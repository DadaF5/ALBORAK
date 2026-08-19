using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;
using FRAProject.Areas.HR.Models;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all. Also: the RBAC session
    // handoff flagged "Wing has no CRUD" as a known gap — that's incorrect,
    // this controller already exists with a full Index/Create/Edit. Worth
    // correcting that note; the real gap was just the missing auth here.
    // Wing carries BaseId directly (not just via Department), so scoping
    // uses that field straight, same as SquadronController.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class WingsController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly IUserScopeService _userScopeService;

        public WingsController(FRAContext context, IUserScopeService userScopeService)
        {
            _context = context;
            _userScopeService = userScopeService;
        }

        // GET: /Wing
        public async Task<IActionResult> Index(int? departmentId, int? baseId, int? acMainGroupId, bool includeInactive = false)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var departmentsQuery = _context.Departments.AsNoTracking().AsQueryable();
            var basesQuery = _context.Bases.AsNoTracking().AsQueryable();
            var acMainGroupsQuery = _context.AcMainGroups.AsNoTracking().AsQueryable();

            if (!scope.IsUnrestricted)
            {
                basesQuery = basesQuery.Where(b => scope.AllowedBaseIds.Contains(b.Id));
                if (scope.AllowedAcMainGroupIds.Any())
                    acMainGroupsQuery = acMainGroupsQuery.Where(a => scope.AllowedAcMainGroupIds.Contains(a.Id));
            }

            var departments = await departmentsQuery.OrderBy(d => d.Name).ToListAsync();
            var bases = await basesQuery.OrderBy(b => b.BaseName).ToListAsync();
            var acMainGroups = await acMainGroupsQuery.OrderBy(a => a.Name).ToListAsync();

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

            if (!scope.IsUnrestricted)
            {
                q = q.Where(w =>
                    w.BaseId.HasValue && scope.AllowedBaseIds.Contains(w.BaseId.Value)
                    && (!scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)));
            }

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
                AcMainGroupName = w.AcMainGroup?.Name ?? "",
                BaseId = w.BaseId,
                BaseName = w.Base?.BaseName ?? "",
                Active = w.Active
            }).ToList();

            return View(vmList);
        }

        // GET: /Wing/Create
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create()
        {
            var vm = new WingViewModel();
            await PopulateSelects(vm);
            return View(vm);
        }

        // POST: /Wing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(WingViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Defense in depth — the dropdown only offers in-scope bases,
            // but BaseId is still a posted value and can be tampered with.
            // (A brand-new Wing has no Id yet, so AllowedWingIds can't be
            // checked here — only the Base boundary applies at creation.)
            if (!scope.IsUnrestricted && (!vm.BaseId.HasValue || !scope.AllowedBaseIds.Contains(vm.BaseId.Value)))
                return Forbid();

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
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var w = await _context.Wings
                .AsNoTracking()
                .Include(x => x.Base)
                .Include(x => x.Department)
                .Include(x => x.AcMainGroup)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (w == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!IsWingInScope(w, scope))
                return Forbid();

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
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(WingViewModel vm)
        {
            var w = await _context.Wings.FindAsync(vm.Id);
            if (w == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Re-check the ORIGINAL record's scope, and the NEW BaseId being
            // posted — a scoped user shouldn't be able to move a wing into
            // or out of a base they don't control.
            if (!IsWingInScope(w, scope) ||
                (!scope.IsUnrestricted && (!vm.BaseId.HasValue || !scope.AllowedBaseIds.Contains(vm.BaseId.Value))))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateSelects(vm);
                return View(vm);
            }

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
        [HttpGet]
        public async Task<IActionResult> GetDepartmentsByBase(int? baseId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            if (!baseId.HasValue)
            {
                var allDepsQuery = _context.Departments.AsNoTracking().AsQueryable();
                if (!scope.IsUnrestricted)
                    allDepsQuery = allDepsQuery.Where(d => scope.AllowedBaseIds.Contains(d.BaseId));

                var allDeps = await allDepsQuery
                    .OrderBy(d => d.Name)
                    .Select(d => new { id = d.Id, name = d.Name })
                    .ToListAsync();
                return Json(allDeps);
            }

            if (!scope.IsUnrestricted && !scope.AllowedBaseIds.Contains(baseId.Value))
                return Json(Array.Empty<object>());

            var deps = await _context.Departments
                .AsNoTracking()
                .Where(d => d.BaseId == baseId.Value)
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();

            return Json(deps);
        }

        // ── Scope helpers ────────────────────────────────────────────────

        private static bool IsWingInScope(Wing w, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;
            if (!w.BaseId.HasValue || !scope.AllowedBaseIds.Contains(w.BaseId.Value)) return false;
            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(w.Id)) return false;
            return true;
        }

        // Populate select lists for Create/Edit
        private async Task PopulateSelects(WingViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var departmentsQuery = _context.Departments.AsNoTracking().AsQueryable();
            var basesQuery = _context.Bases.AsNoTracking().AsQueryable();
            var acMainGroupsQuery = _context.AcMainGroups.AsNoTracking().AsQueryable();

            if (!scope.IsUnrestricted)
            {
                basesQuery = basesQuery.Where(b => scope.AllowedBaseIds.Contains(b.Id));
                departmentsQuery = departmentsQuery.Where(d => scope.AllowedBaseIds.Contains(d.BaseId));
                if (scope.AllowedAcMainGroupIds.Any())
                    acMainGroupsQuery = acMainGroupsQuery.Where(a => scope.AllowedAcMainGroupIds.Contains(a.Id));
            }

            var departments = await departmentsQuery.OrderBy(d => d.Name).ToListAsync();
            var bases = await basesQuery.OrderBy(b => b.BaseName).ToListAsync();
            var acMainGroups = await acMainGroupsQuery.OrderBy(a => a.Name).ToListAsync();

            vm.Departments = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == vm.DepartmentId)).ToList();
            vm.Bases = bases.Select(b => new SelectListItem(b.BaseName, b.Id.ToString(), b.Id == vm.BaseId)).ToList();
            vm.AcMainGroups = acMainGroups.Select(a => new SelectListItem(a.Name, a.Id.ToString(), a.Id == vm.AcMainGroupId)).ToList();

            ViewData["Departments"] = new SelectList(departments, "Id", "Name", vm.DepartmentId);
            ViewData["Bases"] = new SelectList(bases, "Id", "BaseName", vm.BaseId);
            ViewData["AcMainGroups"] = new SelectList(acMainGroups, "Id", "Name", vm.AcMainGroupId);
        }
    }
}
