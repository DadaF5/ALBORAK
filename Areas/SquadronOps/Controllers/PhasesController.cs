using System.Linq;
using System.Threading.Tasks;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ This controller previously had NO [Authorize] attribute at all —
    // Index/Details/Create/Edit/Delete were reachable by ANY authenticated
    // user regardless of module, only blocked from anonymous access by the
    // global FallbackPolicy added in the RBAC session. Phase is global
    // reference data (no Base/Squadron/AcMainGroup of its own, same shape
    // as Ata or AcCategory), so no data scoping is added here — just the
    // missing module-level policy gate.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class PhasesController : Controller
    {
        private readonly FRAContext _context;

        public PhasesController(FRAContext context)
        {
            _context = context;
        }

        // GET: Phases
        public async Task<IActionResult> Index()
        {
            var phases = await _context.Phases
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

            var vm = phases.Select(p => new PhaseViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).ToList();

            return View(vm);
        }

        // GET: Phases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phase = await _context.Phases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (phase == null) return NotFound();

            var vm = new PhaseViewModel
            {
                Id = phase.Id,
                Name = phase.Name,
                Description = phase.Description
            };

            return View(vm);
        }

        // GET: Phases/Create
        [Authorize(Policy = "SquadronOpsWrite")]
        public IActionResult Create()
        {
            return View(new PhaseViewModel());
        }

        // POST: Phases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(PhaseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // uniqueness check (case-insensitive)
            var nameTrim = model.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(nameTrim))
            {
                var exists = await _context.Phases
                    .AnyAsync(p => p.Name.ToLower() == nameTrim.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), "A phase with this name already exists.");
                    return View(model);
                }
            }

            var phase = new Phase
            {
                Name = nameTrim ?? "",
                Description = model.Description?.Trim()
            };

            _context.Phases.Add(phase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Phases/Edit/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phase = await _context.Phases.FindAsync(id.Value);
            if (phase == null) return NotFound();

            var model = new PhaseViewModel
            {
                Id = phase.Id,
                Name = phase.Name,
                Description = phase.Description
            };

            return View(model);
        }

        // POST: Phases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, PhaseViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var nameTrim = model.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(nameTrim))
            {
                var exists = await _context.Phases
                    .AnyAsync(p => p.Id != model.Id && p.Name.ToLower() == nameTrim.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), "A phase with this name already exists.");
                    return View(model);
                }
            }

            var phase = await _context.Phases.FindAsync(model.Id);
            if (phase == null) return NotFound();

            phase.Name = nameTrim ?? "";
            phase.Description = model.Description?.Trim();

            try
            {
                _context.Phases.Update(phase);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Phases.AnyAsync(p => p.Id == model.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Phases/Delete/5
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phase = await _context.Phases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (phase == null) return NotFound();

            var vm = new PhaseViewModel
            {
                Id = phase.Id,
                Name = phase.Name,
                Description = phase.Description
            };

            return View(vm);
        }

        // POST: Phases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phase = await _context.Phases.FindAsync(id);
            if (phase != null)
            {
                _context.Phases.Remove(phase);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
