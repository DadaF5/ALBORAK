// Areas/Settings/Controllers/ModuleRolesController.cs
// Deliberately Settings-area, [Authorize(Roles="Admin")] — matches
// Module.cs's own seed comment ("SETTINGS ← admin only") and overrides
// ModuleRole.cs's original "never edited by end users" design note,
// per this session's decision to build a real UI for it anyway.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.ViewModels;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class ModuleRolesController : Controller
    {
        private readonly IUnitOfWork _uow;
        public ModuleRolesController(IUnitOfWork uow) => _uow = uow;

        // GET: Settings/ModuleRoles
        public async Task<IActionResult> Index()
        {
            var roles = await _uow.ModuleRoles.GetAllAsync();
            return View(roles.OrderBy(r => r.ModuleCode).ThenBy(r => r.SortOrder).ToList());
        }

        // GET: Settings/ModuleRoles/Create
        public async Task<IActionResult> Create()
        {
            var dto = new ModuleRoleFormDto();
            await PopulateModules(dto);
            return View(dto);
        }

        // POST: Settings/ModuleRoles/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ModuleRoleFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateModules(dto);
                return View(dto);
            }

            var existing = await _uow.ModuleRoles.GetAllAsync();
            if (existing.Any(r => r.ModuleCode == dto.ModuleCode && r.RoleCode == dto.RoleCode))
            {
                ModelState.AddModelError(nameof(dto.RoleCode), "Ce code de rôle existe déjà pour ce module.");
                await PopulateModules(dto);
                return View(dto);
            }

            var entity = new ModuleRole
            {
                ModuleCode = dto.ModuleCode,
                RoleCode = dto.RoleCode,
                RoleName = dto.RoleName,
                Description = dto.Description,
                CanWrite = dto.CanWrite,
                SignOffLevel = string.IsNullOrWhiteSpace(dto.SignOffLevel) ? null : dto.SignOffLevel,
                ShowBaseScope = dto.ShowBaseScope,
                ShowGroupScope = dto.ShowGroupScope,
                ShowWingScope = dto.ShowWingScope,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder
            };

            await _uow.ModuleRoles.AddAsync(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Rôle de module créé.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/ModuleRoles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _uow.ModuleRoles.GetByIdAsync(id);
            if (role == null) return NotFound();

            var dto = new ModuleRoleFormDto
            {
                Id = role.Id,
                ModuleCode = role.ModuleCode,
                RoleCode = role.RoleCode,
                RoleName = role.RoleName,
                Description = role.Description,
                CanWrite = role.CanWrite,
                SignOffLevel = role.SignOffLevel,
                ShowBaseScope = role.ShowBaseScope,
                ShowGroupScope = role.ShowGroupScope,
                ShowWingScope = role.ShowWingScope,
                IsActive = role.IsActive,
                SortOrder = role.SortOrder
            };
            await PopulateModules(dto);
            return View(dto);
        }

        // POST: Settings/ModuleRoles/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ModuleRoleFormDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateModules(dto);
                return View(dto);
            }

            var role = await _uow.ModuleRoles.GetByIdAsync(id);
            if (role == null) return NotFound();

            role.ModuleCode = dto.ModuleCode;
            role.RoleCode = dto.RoleCode;
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            role.CanWrite = dto.CanWrite;
            role.SignOffLevel = string.IsNullOrWhiteSpace(dto.SignOffLevel) ? null : dto.SignOffLevel;
            role.ShowBaseScope = dto.ShowBaseScope;
            role.ShowGroupScope = dto.ShowGroupScope;
            role.ShowWingScope = dto.ShowWingScope;
            role.IsActive = dto.IsActive;
            role.SortOrder = dto.SortOrder;

            _uow.ModuleRoles.Update(role);
            await _uow.CompleteAsync();

            TempData["Success"] = "Rôle de module modifié.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/ModuleRoles/Delete/5 — confirmation page, soft/hard choice
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _uow.ModuleRoles.GetByIdAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: Settings/ModuleRoles/ToggleActive/5 — soft delete/reactivate
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var role = await _uow.ModuleRoles.GetByIdAsync(id);
            if (role == null) return NotFound();

            role.IsActive = !role.IsActive;
            _uow.ModuleRoles.Update(role);
            await _uow.CompleteAsync();

            TempData["Success"] = role.IsActive ? "Rôle réactivé." : "Rôle désactivé.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/ModuleRoles/DeleteConfirmed/5 — hard delete, FK-guarded
        [HttpPost, ActionName("DeleteConfirmed"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _uow.ModuleRoles.GetByIdAsync(id);
            if (role == null) return NotFound();

            try
            {
                _uow.ModuleRoles.Delete(role); // adjust to your generic repo's actual delete method name
                await _uow.CompleteAsync();
                TempData["Success"] = "Rôle de module supprimé.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Impossible de supprimer — ce rôle est utilisé par des affectations utilisateur existantes. Désactivez-le plutôt.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateModules(ModuleRoleFormDto dto)
        {
            var modules = await _uow.Modules.GetAllAsync();
            dto.ModuleOptions = modules
                .Where(m => m.IsActive)
                .OrderBy(m => m.SortOrder)
                .Select(m => new SelectListItem($"{m.Name} ({m.Code})", m.Code));
        }
    }
}