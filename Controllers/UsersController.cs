using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Helpers;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly FRAContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            FRAContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // GET: Users
        // Supports: search, roleFilter, baseFilter, isActiveFilter, sorting, paging
        public async Task<IActionResult> Index(
            string? search,
            string? sortOrder,
            string? roleFilter,
            int? baseFilter,
            bool? isActiveFilter,
            int pageNumber = 1,
            int pageSize = 20)
        {
            // populate filter lists for the view
            var availableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            var bases = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            // Build base user query
            var query = _userManager.Users.AsQueryable();

            // Search across username/email/display name/first/last
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u =>
                    (u.UserName ?? "").Contains(s) ||
                    (u.Email ?? "").Contains(s) ||
                    (u.FirstName ?? "").Contains(s) ||
                    (u.LastName ?? "").Contains(s));
            }

            // Base filter
            if (baseFilter.HasValue && baseFilter.Value > 0)
            {
                query = query.Where(u => u.BaseId.HasValue && u.BaseId.Value == baseFilter.Value);
            }

            // IsActive filter
            if (isActiveFilter.HasValue)
            {
                query = query.Where(u => u.IsActive == isActiveFilter.Value);
            }

            // Role filter: get user ids in the selected role and filter by them.
            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var ids = usersInRole.Select(u => u.Id).ToList();
                if (!ids.Any())
                {
                    // no users match -> return empty paged result
                    var emptyVm = new UsersIndexViewModel
                    {
                        Search = search,
                        SortOrder = sortOrder,
                        RoleFilter = roleFilter,
                        BaseFilter = baseFilter,
                        IsActiveFilter = isActiveFilter,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        AvailableRoles = availableRoles,
                        BaseList = bases,
                        Users = await PaginatedList<UserListViewModel>.CreateAsync(Enumerable.Empty<UserListViewModel>().AsQueryable(), pageNumber, pageSize)
                    };
                    return View(emptyVm);
                }
                query = query.Where(u => ids.Contains(u.Id));
            }

            // Sorting
            switch (sortOrder)
            {
                case "user_desc":
                    query = query.OrderByDescending(u => u.UserName);
                    break;
                case "email_asc":
                    query = query.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    query = query.OrderByDescending(u => u.Email);
                    break;
                case "created_asc":
                    query = query.OrderBy(u => u.CreatedAtUtc);
                    break;
                case "created_desc":
                    query = query.OrderByDescending(u => u.CreatedAtUtc);
                    break;
                case "lastlogin_asc":
                    query = query.OrderBy(u => u.LastLoginUtc);
                    break;
                case "lastlogin_desc":
                    query = query.OrderByDescending(u => u.LastLoginUtc);
                    break;
                default:
                    // default sort by username ascending
                    query = query.OrderBy(u => u.UserName);
                    break;
            }

            // Materialize a page of users (ApplicationUser)
            var pagedUsers = await PaginatedList<ApplicationUser>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);

            // Map to UserListViewModel (fetch roles per-page and friendly names to avoid ViewBag lookups)
            var vmList = new List<UserListViewModel>();
            // gather the BaseIds/WingIds/etc that we need to resolve names for in batch
            // batch load ids (adapted from your code)
            var baseIds = pagedUsers.Where(u => u.BaseId.HasValue).Select(u => u.BaseId!.Value).Distinct().ToList();
            var wingIds = pagedUsers.Where(u => u.WingId.HasValue).Select(u => u.WingId!.Value).Distinct().ToList();
            var deptIds = pagedUsers.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId!.Value).Distinct().ToList();
            var squadIds = pagedUsers.Where(u => u.SquadronId.HasValue).Select(u => u.SquadronId!.Value).Distinct().ToList();
            var acGroupIds = pagedUsers.Where(u => u.AcMainGroupId.HasValue).Select(u => u.AcMainGroupId!.Value).Distinct().ToList();

            // batch load maps
            var baseMap = await _context.Set<Base>().Where(b => baseIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.BaseName);
            var wingMap = await _context.Set<Wing>().Where(w => wingIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w.Name);
            var deptMap = await _context.Set<Department>().Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);
            var squadMap = await _context.Set<Squadron>().Where(s => squadIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);
            var acMap = await _context.Set<AcMainGroup>().Where(a => acGroupIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, a => a.Name);

            foreach (var u in pagedUsers) // works if PaginatedList<T> implements IEnumerable<T>
            {
                var roles = await _userManager.GetRolesAsync(u);
                var rolesArray = roles?.ToArray() ?? Array.Empty<string>();

                vmList.Add(new UserListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAtUtc = u.CreatedAtUtc,
                    LastLoginUtc = u.LastLoginUtc,
                    BaseId = u.BaseId,
                    BaseName = u.BaseId.HasValue && baseMap.TryGetValue(u.BaseId.Value, out var bname) ? bname : null,
                    WingId = u.WingId,
                    WingName = u.WingId.HasValue && wingMap.TryGetValue(u.WingId.Value, out var wname) ? wname : null,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.DepartmentId.HasValue && deptMap.TryGetValue(u.DepartmentId.Value, out var dname) ? dname : null,
                    SquadronId = u.SquadronId,
                    SquadronName = u.SquadronId.HasValue && squadMap.TryGetValue(u.SquadronId.Value, out var sname) ? sname : null,
                    AcMainGroupId = u.AcMainGroupId,
                    AcMainGroupName = u.AcMainGroupId.HasValue && acMap.TryGetValue(u.AcMainGroupId.Value, out var aname) ? aname : null,
                    IsActive = u.IsActive,
                    Roles = rolesArray
                });
            }

            // Create paginated view model for UserListViewModel (preserve paging metadata)
            var usersPage = new PaginatedList<UserListViewModel>(vmList, pagedUsers.TotalCount, pagedUsers.PageIndex, pagedUsers.PageSize);

            var vm = new UsersIndexViewModel
            {
                Users = usersPage,
                Search = search,
                SortOrder = sortOrder,
                RoleFilter = roleFilter,
                BaseFilter = baseFilter,
                IsActiveFilter = isActiveFilter,
                PageNumber = pageNumber,
                PageSize = pageSize,
                AvailableRoles = availableRoles,
                BaseList = bases
            };

            return View(vm);
        }

        // GET: Users/Edit/5       
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                BaseId = user.BaseId,
                WingId = user.WingId,
                DepartmentId = user.DepartmentId,
                SquadronId = user.SquadronId,
                AcMainGroupId = user.AcMainGroupId,
                SelectedRoles = roles.ToList(),
                IsActive = user.IsActive
            };

            vm.AvailableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            vm.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            vm.WingList = await _context.Set<Wing>()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            vm.DepartmentList = await _context.Set<Department>()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();

            vm.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();

            ViewBag.UserId = id;

            // Store current values in ViewBag for JavaScript
            ViewBag.CurrentBaseId = user.BaseId;
            ViewBag.CurrentDepartmentId = user.DepartmentId;
            ViewBag.CurrentWingId = user.WingId;
            ViewBag.CurrentSquadronId = user.SquadronId;
            ViewBag.CurrentAcMainGroupId = user.AcMainGroupId;

            return View(vm);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            // repopulate lists for redisplay
            model.AvailableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            model.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            model.WingList = await _context.Set<Wing>()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            model.DepartmentList = await _context.Set<Department>()
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();

            model.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            model.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("ModelState invalid for Users/Edit: {@Errors}", ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray()));
                ViewBag.UserId = id;
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // check email uniqueness
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null && existingByEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already used by another account.");
                ViewBag.UserId = id;
                return View(model);
            }

            // map editable properties (only those coming from the edit form)
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.BaseId = model.BaseId;
            user.WingId = model.WingId;
            user.DepartmentId = model.DepartmentId;
            user.SquadronId = model.SquadronId;
            user.AcMainGroupId = model.AcMainGroupId;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                    _logger?.LogWarning("User update failed: {Code} - {Desc}", err.Code, err.Description);
                }
                ViewBag.UserId = id;
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Except(model.SelectedRoles ?? Enumerable.Empty<string>()).ToArray();
            var toAdd = (model.SelectedRoles ?? Enumerable.Empty<string>()).Except(currentRoles).Distinct().ToArray();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            foreach (var role in toAdd)
            {
                if (await _roleManager.RoleExistsAsync(role))
                    await _userManager.AddToRoleAsync(user, role);
            }

            // update stored claims so the persisted claims match the user's profile
            await AddOrReplaceClaimAsync(user, "BaseId", model.BaseId?.ToString());
            await AddOrReplaceClaimAsync(user, "WingId", model.WingId?.ToString());
            await AddOrReplaceClaimAsync(user, "DepartmentId", model.DepartmentId?.ToString());
            await AddOrReplaceClaimAsync(user, "SquadronId", model.SquadronId?.ToString());
            await AddOrReplaceClaimAsync(user, "AcMainGroupId", model.AcMainGroupId?.ToString());

            // if the edited user is the current user, refresh their cookie so claims update immediately
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // Get: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.Any() ? roles.ToArray() : Array.Empty<string>(),
                BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                IsActive = user.IsActive,
                LastLoginUtc = user.LastLoginUtc,
                CreatedAtUtc = user.CreatedAtUtc
            };

            return View(vm);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            var vm = new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.Any() ? roles.ToArray() : Array.Empty<string>(),
                BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                IsActive = user.IsActive,
                LastLoginUtc = user.LastLoginUtc,
                CreatedAtUtc = user.CreatedAtUtc
            };
            return View(vm);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent admins from deleting themselves accidentally
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                ModelState.AddModelError(string.Empty, "You cannot delete your own account while signed in.");
                var rolesSelf = await _userManager.GetRolesAsync(user);
                var vmSelf = new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = rolesSelf.Any() ? rolesSelf.ToArray() : Array.Empty<string>(),
                    BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                    IsActive = user.IsActive,
                    LastLoginUtc = user.LastLoginUtc,
                    CreatedAtUtc = user.CreatedAtUtc
                };
                return View("Delete", vmSelf);
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // add Identity errors to ModelState so the Delete view can show them
                    foreach (var err in result.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);

                    var roles = await _userManager.GetRolesAsync(user);
                    var vm = new UserListViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = roles.Any() ? roles.ToArray() : Array.Empty<string>(),
                        BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                        IsActive = user.IsActive,
                        LastLoginUtc = user.LastLoginUtc,
                        CreatedAtUtc = user.CreatedAtUtc
                    };

                    return View("Delete", vm);
                }

                // success - optionally log
                _logger?.LogInformation("Admin {Admin} deleted user {User}.", User?.Identity?.Name, user.UserName);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // unexpected error - log and show friendly message
                _logger?.LogError(ex, "Error deleting user {UserId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the user. See server logs for details.");

                var roles = await _userManager.GetRolesAsync(user);
                var vm = new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.Any() ? roles.ToArray() : Array.Empty<string>(),
                    BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                    IsActive = user.IsActive,
                    LastLoginUtc = user.LastLoginUtc,
                    CreatedAtUtc = user.CreatedAtUtc
                };

                return View("Delete", vm);
            }
        }

        // helper to add or replace a claim on a user (removes any existing claim of the same type)
        private async Task AddOrReplaceClaimAsync(ApplicationUser user, string claimType, string? value)
        {
            var existing = (await _userManager.GetClaimsAsync(user)).Where(c => c.Type == claimType).ToList();
            if (existing.Any())
            {
                foreach (var e in existing) await _userManager.RemoveClaimAsync(user, e);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, value));
            }
        }

        // JSON: Get Department by Base
        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByBase(int baseId)
        {
            var departments = await _context.Set<Department>()
                .Where(d => d.BaseId == baseId)
                .OrderBy(d => d.Name)
                .Select(d => new { value = d.Id.ToString(), text = d.Name })
                .ToListAsync();
            return Json(departments);
        }

        // JSON: Get Wings by Department
        [HttpGet]
        public async Task<JsonResult> GetWingsByDepartment(int departmentId)
        {
            var wings = await _context.Set<Wing>()
                .Where(w => w.DepartmentId == departmentId)
                .OrderBy(w => w.Name)
                .Select(w => new { value = w.Id.ToString(), text = w.Name })
                .ToListAsync();
            return Json(wings);
        }

        // JSON: Get Squadrons by Wing
        [HttpGet]
        public async Task<JsonResult> GetSquadronsByWing(int wingId)
        {
            var squadrons = await _context.Set<Squadron>()
                .Where(s => s.WingId == wingId)
                .OrderBy(s => s.Name)
                .Select(s => new { value = s.Id.ToString(), text = s.Name })
                .ToListAsync();
            return Json(squadrons);
        }

        // JSON: Get AcMainGroup by Base
        [HttpGet]
        public async Task<JsonResult> GetAcMainGroupsByBase(int baseId)
        {
            var acGroups = await _context.Set<AcMainGroup>()
                .Where(a => a.BaseId == baseId)
                .OrderBy(a => a.Name)
                .Select(a => new { value = a.Id.ToString(), text = a.Name })
                .ToListAsync();
            return Json(acGroups);
        }
        
    }
}