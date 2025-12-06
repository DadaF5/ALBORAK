using FRAProject.Data;
using FRAProject.Helpers;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly FRAContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            FRAContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
            // supported values: user_asc, user_desc, email_asc, email_desc, created_asc, created_desc, lastlogin_asc, lastlogin_desc
            bool descending = false;
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

            // Materialize a page of users
            var pagedUsers = await PaginatedList<ApplicationUser>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);

            // Map to UserListViewModel (fetch roles per-page to avoid N+fullcount)
            var vmList = new List<UserListViewModel>(pagedUsers.Count);
            foreach (var u in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                string rolesCsv = roles.Any() ? string.Join(", ", roles) : string.Empty;

                string? baseName = null;
                if (u.BaseId.HasValue)
                {
                    var b = await _context.Set<Base>().FindAsync(u.BaseId.Value);
                    baseName = b?.BaseName;
                }

                vmList.Add(new UserListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    DisplayName = u.DisplayName,
                    Roles = rolesCsv,
                    BaseName = baseName,
                    IsActive = u.IsActive,
                    LastLoginUtc = u.LastLoginUtc,
                    CreatedAtUtc = u.CreatedAtUtc
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
            var vm = new RegisterUserViewModel
            {
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                BaseId = user.BaseId,
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

            ViewBag.UserId = id;
            return View(vm);
        }


        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RegisterUserViewModel model)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // repopulate lists for redisplay
            model.AvailableRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();

            model.BaseList = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.BaseId = model.BaseId;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors) ModelState.AddModelError(string.Empty, err.Description);
                ViewBag.UserId = id;
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Except(model.SelectedRoles).ToArray();
            var toAdd = model.SelectedRoles.Except(currentRoles).Distinct().ToArray();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            foreach (var role in toAdd)
            {
                if (await _roleManager.RoleExistsAsync(role))
                    await _userManager.AddToRoleAsync(user, role);
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
                DisplayName = user.DisplayName,
                Roles = roles.Any() ? string.Join(", ", roles) : "",
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
                DisplayName = user.DisplayName,
                Roles = roles.Any() ? string.Join(", ", roles) : "",
                BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                IsActive = user.IsActive,
                LastLoginUtc = user.LastLoginUtc,
                CreatedAtUtc = user.CreatedAtUtc
            };
            return View(vm);
        }
        // POST: Users/Delete/5
        // replace or merge this method into your UsersController
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
                    DisplayName = user.DisplayName,
                    Roles = rolesSelf.Any() ? string.Join(", ", rolesSelf) : "",
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
                        DisplayName = user.DisplayName,
                        Roles = roles.Any() ? string.Join(", ", roles) : "",
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
                    DisplayName = user.DisplayName,
                    Roles = roles.Any() ? string.Join(", ", roles) : "",
                    BaseName = user.BaseId.HasValue ? (await _context.Set<Base>().FindAsync(user.BaseId.Value))?.BaseName : null,
                    IsActive = user.IsActive,
                    LastLoginUtc = user.LastLoginUtc,
                    CreatedAtUtc = user.CreatedAtUtc
                };

                return View("Delete", vm);
            }
        }
    }
}