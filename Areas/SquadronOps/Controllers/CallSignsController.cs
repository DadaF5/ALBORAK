using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // Allow any authenticated user to access read-only actions (Index, Details).
    [Authorize]
    [Area("SquadronOps")]
    public class CallSignsController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<CallSignsController> _logger;

        public CallSignsController(FRAContext context, ILogger<CallSignsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CallSigns
        public async Task<IActionResult> Index()
        {
            var user = User;
            var query = _context.CallSigns.AsQueryable();

            // Non-admins see a filtered list according to their BaseId claim
            if (!user.IsInRole("Admin"))
            {
                var baseClaim = user.FindFirst("BaseId")?.Value;
                if (!string.IsNullOrEmpty(baseClaim) && int.TryParse(baseClaim, out var baseId))
                {
                    query = query.Where(c => c.BaseId == null || c.BaseId == baseId);
                }
                else
                {
                    query = query.Where(c => c.BaseId == null);
                }
            }

            var list = await query
                .Include(c => c.Base)       // <-- eager load Base
                .Include(c => c.Squadron)   // <-- eager load Squadron
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .ToListAsync();
            return View(list);
        }

        // GET: CallSigns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns.FindAsync(id.Value);
            if (cs == null) return NotFound();

            // Non-admins should only see items allowed by the same filter used in Index
            if (!User.IsInRole("Admin"))
            {
                var baseClaim = User.FindFirst("BaseId")?.Value;
                if (!string.IsNullOrEmpty(baseClaim) && int.TryParse(baseClaim, out var baseId))
                {
                    if (cs.BaseId.HasValue && cs.BaseId.Value != baseId)
                        return Forbid();
                }
                else
                {
                    if (cs.BaseId.HasValue)
                        return Forbid();
                }
            }

            return View(cs);
        }

        // GET: CallSigns/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new CallSignViewModel();
            await PopulateSelectsAsync(vm);
            return View(vm);
        }

        // POST: CallSigns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CallSignViewModel model)
        {
            model.Code = (model.Code ?? string.Empty).Trim();

            if (model.SquadronId.HasValue && model.BaseId == null)
            {
                ModelState.AddModelError(nameof(model.BaseId), "Please select a Base when selecting a Squadron.");
            }

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns.FindAsync(id.Value);
            if (cs == null) return NotFound();

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, CallSignViewModel model)
        {
            if (id != model.Id) return BadRequest();

            model.Code = (model.Code ?? string.Empty).Trim();

            if (model.SquadronId.HasValue && model.BaseId == null)
            {
                ModelState.AddModelError(nameof(model.BaseId), "Please select a Base when selecting a Squadron.");
            }

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cs = await _context.CallSigns.FindAsync(id.Value);
            if (cs == null) return NotFound();

            return View(cs);
        }

        // POST: CallSigns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cs = await _context.CallSigns.FindAsync(id);
            if (cs != null)
            {
                _context.CallSigns.Remove(cs);
                await _context.SaveChangesAsync();
            }
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

            var squadrons = await (from s in _context.Set<Squadron>()
                                   join w in _context.Set<Wing>() on s.WingId equals w.Id
                                   join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                                   where d.BaseId == baseId
                                   orderby s.Name
                                   select new { id = s.Id, text = s.Name })
                                  .ToListAsync();

            return Json(squadrons);
        }

        // Helper: populate select lists for Create/Edit views
        private async Task PopulateSelectsAsync(CallSignViewModel vm)
        {
            var bases = await _context.Set<Base>()
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            var baseList = (new[] { new SelectListItem { Value = "", Text = "-- Global --" } }).Concat(bases);
            vm.BaseList = baseList;

            // Populate Squadron list according to selected Base (server-side initial population for Edit)
            await PopulateSquadronsForBaseAsync(vm);
        }

        // Populate SquadronList by traversing Base -> Department -> Wing -> Squadron
        private async Task PopulateSquadronsForBaseAsync(CallSignViewModel vm)
        {
            if (vm.BaseId.HasValue && vm.BaseId.Value > 0)
            {
                var squadrons = await (from s in _context.Set<Squadron>()
                                       join w in _context.Set<Wing>() on s.WingId equals w.Id
                                       join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                                       where d.BaseId == vm.BaseId.Value
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