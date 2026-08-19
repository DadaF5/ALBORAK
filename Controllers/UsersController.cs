using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Helpers;
using FRAProject.Models;
using FRAProject.ViewModels;
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
    // ⚠ Simplified after live testing (test.f5tech@example.com) showed this
    // controller's Create/Edit forms had Department/Wing fields and a
    // 7-checkbox Roles list that LOOKED like real access control but
    // weren't — nothing reads ApplicationUser.DepartmentId/WingId anywhere
    // in the app, and ModuleAccessHandler only checks IsInRole("Admin"),
    // not any of the other six role names. Real module access is granted
    // separately via UserAssignment (see UserAssignmentsController).
    // Department/Wing removed entirely; the role checklist collapsed to a
    // single IsAdmin toggle, the only one that ever did anything.
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private const string AdminRole = "Admin";

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
            var baseIds = pagedUsers.Where(u => u.BaseId.HasValue).Select(u => u.BaseId!.Value).Distinct().ToList();
            var squadIds = pagedUsers.Where(u => u.SquadronId.HasValue).Select(u => u.SquadronId!.Value).Distinct().ToList();
            var acGroupIds = pagedUsers.Where(u => u.AcMainGroupId.HasValue).Select(u => u.AcMainGroupId!.Value).Distinct().ToList();

            var baseMap = await _context.Set<Base>().Where(b => baseIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.BaseName);
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
                    SquadronId = u.SquadronId,
                    SquadronName = u.SquadronId.HasValue && squadMap.TryGetValue(u.SquadronId.Value, out var sname) ? sname : null,
                    AcMainGroupId = u.AcMainGroupId,
                    AcMainGroupName = u.AcMainGroupId.HasValue && acMap.TryGetValue(u.AcMainGroupId.Value, out var aname) ? aname : null,
                    IsActive = u.IsActive,
                    Roles = rolesArray
                });
            }

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

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var vm = new RegisterUserViewModel();
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                BaseId = model.BaseId,
                SquadronId = model.SquadronId,
                AcMainGroupId = model.AcMainGroupId,
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (model.IsAdmin)
            {
                if (await _roleManager.RoleExistsAsync(AdminRole))
                    await _userManager.AddToRoleAsync(user, AdminRole);
            }

            _logger?.LogInformation("Admin {Admin} created user {User}.", User.Identity?.Name, user.UserName);

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, AdminRole);

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                BaseId = user.BaseId,
                SquadronId = user.SquadronId,
                AcMainGroupId = user.AcMainGroupId,
                IsAdmin = isAdmin,
                IsActive = user.IsActive
            };

            await PopulateDropdownsAsync(vm);

            ViewBag.UserId = id;
            ViewBag.CurrentBaseId = user.BaseId;
            ViewBag.CurrentAcMainGroupId = user.AcMainGroupId;

            return View(vm);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
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
                await PopulateDropdownsAsync(model);
                ViewBag.UserId = id;
                return View(model);
            }

            var wasAdmin = await _userManager.IsInRoleAsync(user, AdminRole);

            // ⚠ Previously nothing guarded against removing the last Admin
            // via role edits (only self-delete was blocked, on Delete —
            // deactivating or demoting the last Admin here was completely
            // unguarded, despite that being one of the "4 guard rules"
            // documented back in Phase 1). Added here since collapsing the
            // role checklist to a single toggle makes this a one-click
            // mistake instead of an obscure one.
            if (wasAdmin && (!model.IsAdmin || !model.IsActive) && await IsLastAdminAsync(user))
            {
                ModelState.AddModelError(string.Empty,
                    "Impossible de retirer le rôle Admin ou de désactiver ce compte — c'est le dernier administrateur du système.");
                await PopulateDropdownsAsync(model);
                ViewBag.UserId = id;
                return View(model);
            }

            // map editable properties
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.BaseId = model.BaseId;
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
                await PopulateDropdownsAsync(model);
                ViewBag.UserId = id;
                return View(model);
            }

            // single Admin toggle replaces the old multi-role checklist —
            // "Admin" is the only role ModuleAccessHandler ever checks
            if (model.IsAdmin && !wasAdmin)
            {
                if (await _roleManager.RoleExistsAsync(AdminRole))
                    await _userManager.AddToRoleAsync(user, AdminRole);
            }
            else if (!model.IsAdmin && wasAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, AdminRole);
            }

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
                return await ReturnDeleteViewWithErrors(user);
            }

            // ⚠ Previously unguarded — see the same note in Edit POST.
            if (await IsLastAdminAsync(user))
            {
                ModelState.AddModelError(string.Empty,
                    "Impossible de supprimer ce compte — c'est le dernier administrateur du système.");
                return await ReturnDeleteViewWithErrors(user);
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);

                    return await ReturnDeleteViewWithErrors(user);
                }

                _logger?.LogInformation("Admin {Admin} deleted user {User}.", User?.Identity?.Name, user.UserName);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting user {UserId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the user. See server logs for details.");

                return await ReturnDeleteViewWithErrors(user);
            }
        }

        // GET: Users/GetAcMainGroupsByBase?baseId=1
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

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task<bool> IsLastAdminAsync(ApplicationUser user)
        {
            if (!await _userManager.IsInRoleAsync(user, AdminRole)) return false;
            var admins = await _userManager.GetUsersInRoleAsync(AdminRole);
            return admins.Count <= 1;
        }

        private async Task<IActionResult> ReturnDeleteViewWithErrors(ApplicationUser user)
        {
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

        private async Task PopulateDropdownsAsync(RegisterUserViewModel vm)
        {
            vm.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            vm.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();
        }

        private async Task PopulateDropdownsAsync(EditUserViewModel vm)
        {
            vm.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            // Flat list, no longer cascaded through Wing/Department — this
            // field is a plain default value now, not a scoped selector.
            vm.SquadronList = await _context.Set<Squadron>()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroupList = await _context.Set<AcMainGroup>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();
        }
    }
}
