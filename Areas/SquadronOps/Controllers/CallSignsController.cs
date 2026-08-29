using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.ViewModels;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class CallSignsController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly ILogger<CallSignsController> _logger;
        private readonly IUserScopeService _userScopeService;

        public CallSignsController(FRAContext context, ILogger<CallSignsController> logger, IUserScopeService userScopeService)
        {
            _context = context;
            _logger = logger;
            _userScopeService = userScopeService;
        }

        // GET: CallSigns
        // Replaces the old raw "BaseId" claim check with real UserAssignment
        // scope. A CallSign with BaseId==null is a global entry (visible to
        // everyone in-module); a squadron-linked one is additionally
        // filtered by Wing for roles that carry ShowWingScope=true.
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var query = _context.CallSigns
                .Include(c => c.Base)
                .Include(c => c.Squadron)
                .AsNoTracking()
                .AsQueryable();

            if (!scope.IsUnrestricted)
            {
                query = query.Where(c => c.BaseId == null || scope.AllowedBaseIds.Contains(c.BaseId.Value));
            }

            var list = await query.OrderBy(c => c.Code).ToListAsync();

            if (!scope.IsUnrestricted && scope.AllowedWingIds.Any())
            {
                list = list
                    .Where(c => !c.SquadronId.HasValue || scope.AllowedWingIds.Contains(c.Squadron!.WingId))
                    .ToList();
            }

            return View(list);
        }

        // GET: CallSigns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns
                .Include(c => c.Squadron)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (cs == null) return NotFound();

            if (!await IsCallSignScopeOkAsync(cs.BaseId, cs.Squadron?.WingId))
                return Forbid();

            return View(cs);
        }

        // GET: CallSigns/Create
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create()
        {
            var vm = new CallSignViewModel();
            await PopulateSelectsAsync(vm);
            return View(vm);
        }

        // POST: CallSigns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(CallSignViewModel model)
        {
            model.Code = (model.Code ?? string.Empty).Trim();

            if (model.SquadronId.HasValue && model.BaseId == null)
            {
                ModelState.AddModelError(nameof(model.BaseId), "Please select a Base when selecting a Squadron.");
            }

            // Defense in depth — the dropdown only offers in-scope Base/
            // Squadron options, but these are still posted values and can
            // be tampered with. A scoped user also can't create a global
            // (BaseId == null) call sign — that's an unrestricted-only action.
            var wingId = await GetWingIdForSquadronAsync(model.SquadronId);
            if (!await IsCallSignScopeOkAsync(model.BaseId, wingId, requireBase: true))
                return Forbid();

            if (await DuplicateCallSignExistsSimpleAsync(model.Code, model.BaseId, model.SquadronId, null))
            {
                ModelState.AddModelError(nameof(model.Code), "A CallSign with this code already exists in the selected scope.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectsAsync(model);
                return View(model);
            }

            var entity = new CallSign
            {
                Code = model.Code,
                Description = model.Description,
                BaseId = model.BaseId,
                SquadronId = model.SquadronId,
                IsActive = model.IsActive,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            _context.CallSigns.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: CallSigns/Edit/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns
                .Include(c => c.Squadron)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (cs == null) return NotFound();

            if (!await IsCallSignScopeOkAsync(cs.BaseId, cs.Squadron?.WingId))
                return Forbid();

            var vm = new CallSignViewModel
            {
                Id = cs.Id,
                Code = cs.Code,
                Description = cs.Description,
                BaseId = cs.BaseId,
                SquadronId = cs.SquadronId,
                IsActive = cs.IsActive
            };

            await PopulateSelectsAsync(vm);
            return View(vm);
        }

        // POST: CallSigns/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, CallSignViewModel model)
        {
            if (id != model.Id) return BadRequest();

            model.Code = (model.Code ?? string.Empty).Trim();

            if (model.SquadronId.HasValue && model.BaseId == null)
            {
                ModelState.AddModelError(nameof(model.BaseId), "Please select a Base when selecting a Squadron.");
            }

            var wingId = await GetWingIdForSquadronAsync(model.SquadronId);
            if (!await IsCallSignScopeOkAsync(model.BaseId, wingId, requireBase: true))
                return Forbid();

            if (await DuplicateCallSignExistsSimpleAsync(model.Code, model.BaseId, model.SquadronId, model.Id))
            {
                ModelState.AddModelError(nameof(model.Code), "A CallSign with this code already exists in the selected scope.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectsAsync(model);
                return View(model);
            }

            var cs = await _context.CallSigns.FindAsync(id);
            if (cs == null) return NotFound();

            // Re-check the ORIGINAL record's scope too — a scoped user
            // shouldn't be able to reach into another base's call sign via
            // a tampered id even if the new BaseId they posted is in-scope.
            var originalWingId = await GetWingIdForSquadronAsync(cs.SquadronId);
            if (!await IsCallSignScopeOkAsync(cs.BaseId, originalWingId))
                return Forbid();

            cs.Code = model.Code;
            cs.Description = model.Description;
            cs.BaseId = model.BaseId;
            cs.SquadronId = model.SquadronId;
            cs.IsActive = model.IsActive;
            cs.UpdatedAtUtc = DateTime.UtcNow;
            cs.UpdatedBy = User.Identity?.Name;

            _context.CallSigns.Update(cs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: CallSigns/Delete/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns
                .Include(c => c.Squadron)
                .FirstOrDefaultAsync(c => c.Id == id.Value);
            if (cs == null) return NotFound();

            if (!await IsCallSignScopeOkAsync(cs.BaseId, cs.Squadron?.WingId))
                return Forbid();

            return View(cs);
        }

        // POST: CallSigns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cs = await _context.CallSigns
                .Include(c => c.Squadron)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cs == null) return NotFound();

            if (!await IsCallSignScopeOkAsync(cs.BaseId, cs.Squadron?.WingId))
                return Forbid();

            _context.CallSigns.Remove(cs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool CallSignExists(int id) => _context.CallSigns.Any(e => e.Id == id);

        // JSON endpoint: get squadrons for a given base by traversing:
        // Base -> Department -> Wing -> Squadron
        [HttpGet]
        public async Task<IActionResult> GetSquadrons(int baseId)
        {
            if (baseId <= 0)
                return Json(Array.Empty<object>());

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!scope.IsUnrestricted && !scope.AllowedBaseIds.Contains(baseId))
                return Json(Array.Empty<object>());

            var squadrons = await (from s in _context.Set<Squadron>()
                                   join w in _context.Set<Wing>() on s.WingId equals w.Id
                                   join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                                   where d.BaseId == baseId
                                   where scope.IsUnrestricted || !scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)
                                   orderby s.Name
                                   select new { id = s.Id, text = s.Name })
                                  .ToListAsync();

            return Json(squadrons);
        }

        // Helper: populate select lists for Create/Edit views
        private async Task PopulateSelectsAsync(CallSignViewModel vm)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var basesQuery = _context.Set<Base>().AsQueryable();
            if (!scope.IsUnrestricted)
            {
                basesQuery = basesQuery.Where(b => scope.AllowedBaseIds.Contains(b.Id));
            }

            var bases = await basesQuery
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            // "Global" (no base) call signs are visible to everyone but only
            // creatable by unrestricted (Admin) users — a scoped user
            // shouldn't be able to publish a fleet-wide call sign.
            vm.BaseList = scope.IsUnrestricted
                ? (new[] { new SelectListItem { Value = "", Text = "-- Global --" } }).Concat(bases)
                : bases;

            // Populate Squadron list according to selected Base (server-side initial population for Edit)
            await PopulateSquadronsForBaseAsync(vm);
        }

        // Populate SquadronList by traversing Base -> Department -> Wing -> Squadron
        private async Task PopulateSquadronsForBaseAsync(CallSignViewModel vm)
        {
            if (vm.BaseId.HasValue && vm.BaseId.Value > 0)
            {
                var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

                var squadrons = await (from s in _context.Set<Squadron>()
                                       join w in _context.Set<Wing>() on s.WingId equals w.Id
                                       join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                                       where d.BaseId == vm.BaseId.Value
                                       where scope.IsUnrestricted || !scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)
                                       orderby s.Name
                                       select new SelectListItem
                                       {
                                           Value = s.Id.ToString(),
                                           Text = s.Name
                                       }).ToListAsync();

                vm.SquadronList = (new[] { new SelectListItem { Value = "", Text = "-- Select Squadron (optional) --" } })
                                    .Concat(squadrons);
            }
            else
            {
                vm.SquadronList = new[] { new SelectListItem { Value = "", Text = "-- Select Base first --" } };
            }
        }

        // ── Scope helpers ────────────────────────────────────────────────

        private async Task<int?> GetWingIdForSquadronAsync(int? squadronId)
        {
            if (!squadronId.HasValue) return null;
            var squadron = await _context.Set<Squadron>().FirstOrDefaultAsync(s => s.Id == squadronId.Value);
            return squadron?.WingId;
        }

        // requireBase: true for Create/Edit POST, where a scoped (non-Admin)
        // user must supply a real Base — they can't post a global call sign.
        private async Task<bool> IsCallSignScopeOkAsync(int? baseId, int? wingId, bool requireBase = false)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;

            if (!baseId.HasValue)
                return !requireBase; // global entries: viewable by all, creatable by Admin only

            if (!scope.AllowedBaseIds.Contains(baseId.Value))
                return false;

            if (wingId.HasValue && scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(wingId.Value))
                return false;

            return true;
        }

        // Simple duplicate check: same Code (case-insensitive, trimmed) within the selected scope.
        // If squadronId is provided we check within that squadron.
        // Else if baseId is provided we check within that base (but not squadron scoped ones).
        // Else we check for global (BaseId == null && SquadronId == null).
        // ignoreId: optional id to exclude (useful for Edit).
        private async Task<bool> DuplicateCallSignExistsSimpleAsync(string code, int? baseId, int? squadronId, int? ignoreId)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var normalized = code.Trim().ToUpperInvariant();

            var query = _context.CallSigns.AsQueryable();

            if (ignoreId.HasValue)
                query = query.Where(c => c.Id != ignoreId.Value);

            if (squadronId.HasValue && squadronId.Value > 0)
            {
                return await query.AnyAsync(c =>
                    c.SquadronId.HasValue &&
                    c.SquadronId.Value == squadronId.Value &&
                    c.Code.ToUpper() == normalized);
            }

            if (baseId.HasValue && baseId.Value > 0)
            {
                return await query.AnyAsync(c =>
                    !c.SquadronId.HasValue &&
                    c.BaseId.HasValue &&
                    c.BaseId.Value == baseId.Value &&
                    c.Code.ToUpper() == normalized);
            }

            // Global call sign duplicate (no base, no squadron)
            return await query.AnyAsync(c =>
                !c.BaseId.HasValue &&
                !c.SquadronId.HasValue &&
                c.Code.ToUpper() == normalized);
        }
    }
}
