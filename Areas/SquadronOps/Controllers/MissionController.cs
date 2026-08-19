using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Was [Authorize(Roles = "Admin,SquadronOps")] — "SquadronOps" is not
    // a real seeded AspNetRole (the 17 real roles live in the ModuleRole
    // table, not AspNetRoles). That check was almost certainly dead for
    // every non-Admin user. Replaced with the real SquadronOpsRead/Write
    // policies, same as the AircraftMaintenance conversion.
    [Area("SquadronOps")]
    [Authorize(Policy = "SquadronOpsRead")]
    public class MissionController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly ILogger<MissionController> _logger;
        private readonly FRAContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;

        public MissionController(
            ILogger<MissionController> logger,
            FRAContext context,
            UserManager<ApplicationUser> userManager,
            IUserScopeService userScopeService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _userScopeService = userScopeService;
        }

        // --- Index List missions ---
        public async Task<IActionResult> Index()
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            IQueryable<Mission> query = _context.Missions
                .Include(m => m.Phase)
                .Where(m => m.IsActive);

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                query = query.Where(m =>
                    m.SquadronId == null || allowedSquadronIds.Contains(m.SquadronId.Value));
            }

            var missions = await query
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();

            return View(missions);
        }

        // GET: Create Mission
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Phases = await _context.Phases
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(new Mission());
        }
        // POST: Create Mission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create(Mission model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Phases = await _context.Phases
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return View(model);
            }

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            if (scope.IsUnrestricted)
            {
                // Unrestricted (Admin, or a base-admin assignment with no
                // group filter) creates a GLOBAL mission by default, same
                // as before.
                model.SquadronId = null;
            }
            else
            {
                model.SquadronId = await GetCurrentSquadronIdAsync(scope);
            }

            model.IsActive = true;

            _context.Missions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        // GET: Edit Mission
        [HttpGet]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
                return NotFound();

            if (!await IsSquadronInScopeAsync(mission.SquadronId))
                return Forbid();

            ViewBag.Phases = await _context.Phases
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(mission);

        }
        // POST: Edit Mission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(Mission model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Phases = await _context.Phases
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return View(model);
            }
            var mission = await _context.Missions.FindAsync(model.Id);
            if (mission == null)
                return NotFound();

            if (!await IsSquadronInScopeAsync(mission.SquadronId))
                return Forbid();

            mission.Name = model.Name;
            mission.Code = model.Code;
            mission.PhaseId = model.PhaseId;
            mission.Description = model.Description;
            mission.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Deactivate (soft delete Mission)
        [HttpPost]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
                return NotFound();

            if (!await IsSquadronInScopeAsync(mission.SquadronId))
                return Forbid();

            mission.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ── Scope helpers ────────────────────────────────────────────────

        // Resolves the current user's "home squadron" for auto-assignment
        // on Create, the same convenience the legacy ApplicationUser.SquadronId
        // field provided — but cross-checked against the real
        // UserAssignment-derived scope first. These two systems are NOT
        // guaranteed to agree (flagged explicitly in the RBAC session
        // handoff), so trusting the legacy field blind would let a stale
        // or incorrect SquadronId bypass real authorization.
        private async Task<int?> GetCurrentSquadronIdAsync(UserScope scope)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                throw new InvalidOperationException("Authenticated user not found.");

            if (!user.SquadronId.HasValue)
                throw new InvalidOperationException("User is not assigned to a squadron.");

            if (!await IsSquadronInScopeAsync(user.SquadronId, scope))
                throw new InvalidOperationException(
                    $"User's assigned squadron (Id={user.SquadronId}) is outside their current " +
                    "UserAssignment scope. The legacy ApplicationUser.SquadronId and the real " +
                    "UserAssignment records have drifted — reconcile before proceeding.");

            return user.SquadronId.Value;
        }

        private async Task<bool> IsSquadronInScopeAsync(int? squadronId, UserScope? scope = null)
        {
            scope ??= await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (scope.IsUnrestricted) return true;
            if (!squadronId.HasValue) return true; // global mission — visible to all in-module users

            var info = await (from s in _context.Set<Squadron>()
                               join w in _context.Set<Wing>() on s.WingId equals w.Id
                               join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                               where s.Id == squadronId.Value
                               select new { WingId = w.Id, d.BaseId })
                              .FirstOrDefaultAsync();

            if (info == null) return false;
            if (!scope.AllowedBaseIds.Contains(info.BaseId)) return false;
            if (scope.AllowedWingIds.Any() && !scope.AllowedWingIds.Contains(info.WingId)) return false;

            return true;
        }

        private async Task<HashSet<int>> GetInScopeSquadronIdsAsync(UserScope scope)
        {
            var query = from s in _context.Set<Squadron>()
                        join w in _context.Set<Wing>() on s.WingId equals w.Id
                        join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                        where scope.AllowedBaseIds.Contains(d.BaseId)
                        where !scope.AllowedWingIds.Any() || scope.AllowedWingIds.Contains(w.Id)
                        select s.Id;

            return (await query.ToListAsync()).ToHashSet();
        }
    }
}
