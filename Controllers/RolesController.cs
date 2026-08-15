// Controllers/RolesController.cs — root, no Area, matching UsersController's placement
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FRAProject.Models;
using FRAProject.ViewModels;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private const string ProtectedRole = "Admin";

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            var vm = new List<RoleListVm>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                vm.Add(new RoleListVm
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count,
                    IsProtected = role.Name == ProtectedRole
                });
            }

            return View(vm);
        }

        // GET: Roles/Create
        public IActionResult Create() => View(new RoleFormDto());

        // POST: Roles/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleFormDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            if (await _roleManager.RoleExistsAsync(dto.Name))
            {
                ModelState.AddModelError(nameof(dto.Name), "Ce rôle existe déjà.");
                return View(dto);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(dto.Name));
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
                return View(dto);
            }

            TempData["Success"] = "Rôle créé.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == ProtectedRole)
            {
                TempData["Error"] = "Le rôle Admin ne peut pas être renommé.";
                return RedirectToAction(nameof(Index));
            }

            return View(new RoleFormDto { Id = role.Id, Name = role.Name! });
        }

        // POST: Roles/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleFormDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return View(dto);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            if (role.Name == ProtectedRole)
            {
                TempData["Error"] = "Le rôle Admin ne peut pas être renommé.";
                return RedirectToAction(nameof(Index));
            }

            role.Name = dto.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
                return View(dto);
            }

            TempData["Success"] = "Rôle modifié.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

            ViewBag.IsProtected = role.Name == ProtectedRole;
            ViewBag.UserCount = usersInRole.Count;
            return View(role);
        }

        // POST: Roles/DeleteConfirmed/{id}
        [HttpPost, ActionName("DeleteConfirmed"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == ProtectedRole)
            {
                TempData["Error"] = "Le rôle Admin ne peut pas être supprimé — protection permanente.";
                return RedirectToAction(nameof(Index));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                TempData["Error"] = $"Impossible de supprimer — ce rôle est assigné à {usersInRole.Count} utilisateur(s). Retirez-le d'abord de leurs comptes.";
                return RedirectToAction(nameof(Index));
            }

            await _roleManager.DeleteAsync(role);
            TempData["Success"] = "Rôle supprimé.";
            return RedirectToAction(nameof(Index));
        }
    }
}