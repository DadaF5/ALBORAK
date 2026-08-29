using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.ViewModels;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Previously had NO [Authorize] at all. Squadron belongs directly to
    // a Wing (Wing.BaseId + UserAssignment.WingId are the scope-relevant
    // fields here) — no need to traverse through Department.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class SquadronController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserScopeService _userScopeService;

        public SquadronController(FRAContext context, IWebHostEnvironment env, IUserScopeService userScopeService)
        {
            _context = context;
            _env = env;
            _userScopeService = userScopeService;
        }

        // Helper to delete a file given a web-relative path like "/uploads/squadrons/abc.png"
        private void DeletePhysicalFileIfExists(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath)) return;

            var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physical = Path.Combine(_env.WebRootPath ?? string.Empty, relative);

            try
            {
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
            }
            catch
            {
                // swallow exceptions for now; you might want to log
            }
        }

        // ===== INDEX =====
        public async Task<IActionResult> Index(int? baseId, int? wingId)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var query = _context.Squadrons
                .Include(s => s.Wing)
                .ThenInclude(w => w.Base)
                .AsQueryable();

            if (!scope.IsUnrestricted)
            {
                query = query.Where(s => s.Wing != null
                    && s.Wing.BaseId.HasValue && scope.AllowedBaseIds.Contains(s.Wing.BaseId.Value)
                    && (!scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(s.WingId)));
            }

            if (baseId.HasValue)
            {
                query = query.Where(s => s.Wing != null && s.Wing.BaseId == baseId.Value);
            }

            if (wingId.HasValue)
            {
                query = query.Where(s => s.WingId == wingId.Value);
            }

            var squadrons = await query.OrderBy(s => s.Name).ToListAsync();

            var model = squadrons.Select(s => new SquadronViewModel
            {
                Id = s.Id,
                Name = s.Name,
                CallSign = s.CallSign,
                CallSignShort = s.CallSignShort,
                FrenchName = s.FrenchName,
                WingId = s.WingId,
                WingName = s.Wing?.Name ?? "",
                BaseId = s.Wing?.BaseId,
                BaseName = s.Wing?.Base?.BaseName ?? "",
                LogoPath = s.LogoPath,
                Active = s.Active
            }).ToList();

            var basesQuery = _context.Bases.AsQueryable();
            var wingsFilterQuery = _context.Wings.AsQueryable();
            if (!scope.IsUnrestricted)
            {
                basesQuery = basesQuery.Where(b => scope.AllowedBaseIds.Contains(b.Id));
                wingsFilterQuery = wingsFilterQuery.Where(w =>
                    w.BaseId.HasValue && scope.AllowedBaseIds.Contains(w.BaseId.Value)
                    && (!scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)));
            }

            var bases = await basesQuery.OrderBy(b => b.BaseName).ToListAsync();
            ViewData["Bases"] = new SelectList(bases, "Id", "BaseName", baseId);

            if (baseId.HasValue)
            {
                wingsFilterQuery = wingsFilterQuery.Where(w => w.BaseId == baseId.Value);
            }
            var wingsList = await wingsFilterQuery.OrderBy(w => w.Name).ToListAsync();
            ViewData["Wings"] = new SelectList(wingsList, "Id", "Name", wingId);

            return View(model);
        }

        // ===== CREATE GET =====
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var model = new SquadronViewModel
            {
                Bases = await GetScopedBaseOptionsAsync(scope),
                Wings = await GetScopedWingOptionsAsync(scope, null),
                Active = true
            };

            return View(model);
        }

        // ===== CREATE POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(SquadronViewModel model)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Defense in depth — the dropdown only offers in-scope wings,
            // but WingId is still a posted value and can be tampered with.
            if (!await IsWingInScopeAsync(model.WingId, scope))
                return Forbid();

            // server-side uniqueness check
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var nameTrim = model.Name.Trim().ToLower();
                if (await _context.Squadrons.AnyAsync(s => s.WingId == model.WingId && s.Name.ToLower() == nameTrim))
                {
                    ModelState.AddModelError("Name", "A squadron with this name already exists in the selected wing.");
                }
            }

            if (ModelState.IsValid)
            {
                string? logoPath = null;

                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads", "squadrons");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fs);
                    }

                    logoPath = "/uploads/squadrons/" + fileName;
                }

                var squadron = new Squadron
                {
                    Name = model.Name.Trim(),
                    CallSign = model.CallSign,
                    CallSignShort = model.CallSignShort,
                    FrenchName = model.FrenchName,
                    WingId = model.WingId,
                    LogoPath = logoPath,
                    Active = model.Active
                };

                _context.Squadrons.Add(squadron);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // repopulate dropdowns if validation failed
            model.Bases = await GetScopedBaseOptionsAsync(scope);
            model.Wings = await GetScopedWingOptionsAsync(scope, null);

            return View(model);
        }

        // ===== EDIT GET =====
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsWingInScopeAsync(squadron.WingId, scope))
                return Forbid();

            var model = new SquadronViewModel
            {
                Id = squadron.Id,
                Name = squadron.Name,
                CallSign = squadron.CallSign,
                CallSignShort = squadron.CallSignShort,
                FrenchName = squadron.FrenchName,
                WingId = squadron.WingId,
                LogoPath = squadron.LogoPath,
                Active = squadron.Active,
                Wings = await GetScopedWingOptionsAsync(scope, squadron.WingId)
            };

            return View(model);
        }

        // ===== EDIT POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(SquadronViewModel model)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var squadron = await _context.Squadrons.FindAsync(model.Id);
            if (squadron == null) return NotFound();

            // Re-check the ORIGINAL record's scope, and the NEW WingId being
            // posted — a scoped user shouldn't be able to move a squadron
            // into or out of a wing they don't control.
            if (!await IsWingInScopeAsync(squadron.WingId, scope) || !await IsWingInScopeAsync(model.WingId, scope))
                return Forbid();

            // uniqueness check: exclude current record by Id
            if (await _context.Squadrons.AnyAsync(s => s.Id != model.Id && s.WingId == model.WingId && s.Name.ToLower() == model.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "A squadron with this name already exists in the selected wing.");
            }

            if (ModelState.IsValid)
            {
                // If the user asked to remove the existing logo, delete the file and clear the path
                if (model.RemoveLogo && !string.IsNullOrWhiteSpace(squadron.LogoPath))
                {
                    DeletePhysicalFileIfExists(squadron.LogoPath);
                    squadron.LogoPath = null;
                }

                // If a new file was uploaded, remove previous file (if any) then save new file
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
                    {
                        DeletePhysicalFileIfExists(squadron.LogoPath);
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads", "squadrons");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fs);
                    }

                    squadron.LogoPath = "/uploads/squadrons/" + fileName;
                }

                squadron.Name = model.Name.Trim();
                squadron.CallSign = model.CallSign;
                squadron.CallSignShort = model.CallSignShort;
                squadron.FrenchName = model.FrenchName;
                squadron.WingId = model.WingId;
                squadron.Active = model.Active;

                _context.Squadrons.Update(squadron);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Wings = await GetScopedWingOptionsAsync(scope, model.WingId);

            return View(model);
        }

        // ===== DETAILS =====
        public async Task<IActionResult> Details(int id)
        {
            var squadron = await _context.Squadrons
                .Include(s => s.Wing)
                .ThenInclude(w => w.Base)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (squadron == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsWingInScopeAsync(squadron.WingId, scope))
                return Forbid();

            var model = new SquadronViewModel
            {
                Id = squadron.Id,
                Name = squadron.Name,
                CallSign = squadron.CallSign,
                CallSignShort = squadron.CallSignShort,
                FrenchName = squadron.FrenchName,
                WingId = squadron.WingId,
                WingName = squadron.Wing?.Name ?? "",
                BaseName = squadron.Wing?.Base?.BaseName ?? "",
                LogoPath = squadron.LogoPath,
                Active = squadron.Active
            };

            return View(model);
        }

        // ===== Delete Logo action (POST) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> DeleteLogo(int id, string? returnUrl)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron == null) return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!await IsWingInScopeAsync(squadron.WingId, scope))
                return Forbid();

            if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
            {
                DeletePhysicalFileIfExists(squadron.LogoPath);
                squadron.LogoPath = null;
                _context.Squadrons.Update(squadron);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ===== DELETE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Delete(int id)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron != null)
            {
                var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
                if (!await IsWingInScopeAsync(squadron.WingId, scope))
                    return Forbid();

                if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
                {
                    DeletePhysicalFileIfExists(squadron.LogoPath);
                }

                _context.Squadrons.Remove(squadron);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Scope helpers ────────────────────────────────────────────────

        private async Task<bool> IsWingInScopeAsync(int wingId, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            var wing = await _context.Wings.FirstOrDefaultAsync(w => w.Id == wingId);
            if (wing == null || !wing.BaseId.HasValue || !scope.AllowedBaseIds.Contains(wing.BaseId.Value))
                return false;

            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(wingId))
                return false;

            return true;
        }

        private async Task<List<SelectListItem>> GetScopedBaseOptionsAsync(UserScope scope)
        {
            var query = _context.Bases.AsQueryable();
            if (!scope.IsUnrestricted)
                query = query.Where(b => scope.AllowedBaseIds.Contains(b.Id));

            return await query
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetScopedWingOptionsAsync(UserScope scope, int? selectedWingId)
        {
            var query = _context.Wings.AsQueryable();
            if (!scope.IsUnrestricted)
            {
                query = query.Where(w =>
                    w.BaseId.HasValue && scope.AllowedBaseIds.Contains(w.BaseId.Value)
                    && (!scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)));
            }

            var wings = await query.OrderBy(w => w.Name).ToListAsync();
            return wings.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name,
                Selected = selectedWingId.HasValue && selectedWingId.Value == w.Id
            }).ToList();
        }
    }
}
