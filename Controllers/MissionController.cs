using FRAProject.Data;
using FRAProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin,SquadronOps")]
    public class MissionController : Controller
    {
        private readonly ILogger<MissionController> _logger;
        private readonly FRAContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public MissionController(
            ILogger<MissionController> logger, 
            FRAContext context ,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;

        }

        // --- Index List missions ---
        public async Task<IActionResult> Index()
        {
            int? squadronId = await GetCurrentSquadronIdAsync();

            IQueryable<Mission> query = _context.Missions
                .Include(m => m.Phase)
                .Where(m => m.IsActive);

            if (squadronId.HasValue)
            {
                query = query.Where(m =>
                    m.SquadronId == null || m.SquadronId == squadronId);
            }
            // else Admin → no filter

            var missions = await query
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();

            return View(missions);
        }
        // GET: Create Mission
        [HttpGet]
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
        public async Task<IActionResult> Create(Mission model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Phases = await _context.Phases
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return View(model);
            }
            if (User.IsInRole("Admin"))
            {
                // Admin creates GLOBAL mission by default
                model.SquadronId = null;
            }
            else
            {
                model.SquadronId = await GetCurrentSquadronIdAsync();
            }

            model.IsActive = true;

            _context.Missions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        // GET: Edit Mission
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
            
                return NotFound();

            ViewBag.Phases = await _context.Phases
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(mission);
            
        }
        // POST: Edit Mission
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Deactivate(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
                return NotFound();
            mission.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private async Task<int?> GetCurrentSquadronIdAsync()
        {
            if (User.IsInRole("Admin"))
                return null; // Admins can access all squadrons

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                throw new InvalidOperationException("Authenticated user not found.");

            if (!user.SquadronId.HasValue)
                throw new InvalidOperationException("User is not assigned to a squadron.");

            return user.SquadronId.Value;
        }

       
    }
}
