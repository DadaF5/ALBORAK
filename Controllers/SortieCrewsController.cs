using FRAProject.Data;
using FRAProject.Enums;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class SortieCrewsController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<SortieCrewsController> _logger;

        public SortieCrewsController(FRAContext context, ILogger<SortieCrewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SortieCrews/Index?sortieId=123
        [HttpGet]
        public async Task<IActionResult> Index(int sortieId)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null)
            {
                return NotFound($"Sortie with ID {sortieId} not found.");
            }

            // Get all crew assignments for this sortie
            var crewAssignments = await _context.SortieCrews
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Person)
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Squadron)
                .Where(sc => sc.SortieId == sortieId)
                .OrderBy(sc => sc.Seat)
                .ThenBy(sc => sc.IsPrimary ? 0 : 1)
                .ToListAsync();

            ViewBag.SortieId = sortieId;
            ViewBag.SortieCode = sortie.SortieCode;
            ViewBag.OdvId = sortie.OdvId;

            return View(crewAssignments);
        }

        // GET: SortieCrews/Create?sortieId=123
        [HttpGet]
        public async Task<IActionResult> Create(int sortieId)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null)
            {
                return NotFound($"Sortie with ID {sortieId} not found.");
            }

            // Get user's squadron/AcMainGroup for filtering
            var userSquadronId = await GetUserSquadronId();
            var userAcMainGroupId = await GetUserAcMainGroupId();

            // Get already assigned crew members to exclude them
            var assignedCrewIds = await _context.SortieCrews
                .Where(sc => sc.SortieId == sortieId)
                .Select(sc => sc.CrewMemberId)
                .ToListAsync();

            // Filter available crew members
            var availableCrewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active &&
                       !assignedCrewIds.Contains(cm.Id) &&
                       (userSquadronId == null || cm.SquadronId == userSquadronId))
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}"
                })
                .ToListAsync();

            var vm = new SortieCrewCreateVm
            {
                SortieId = sortieId,
                SortieCode = sortie.SortieCode
            };

            ViewBag.CrewMembers = availableCrewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions();

            return View(vm);
        }

        // POST: SortieCrews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SortieCrewCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model.SortieId);
                return View(model);
            }

            // Check if crew member is already assigned to this sortie
            var existingAssignment = await _context.SortieCrews
                .FirstOrDefaultAsync(sc => sc.SortieId == model.SortieId && sc.CrewMemberId == model.CrewMemberId);

            if (existingAssignment != null)
            {
                ModelState.AddModelError("CrewMemberId", "This crew member is already assigned to this sortie.");
                await PopulateDropdowns(model.SortieId);
                return View(model);
            }

            var sortieCrew = new SortieCrew
            {
                SortieId = model.SortieId,
                CrewMemberId = model.CrewMemberId,
                Seat = model.Seat,
                AircraftRole = model.AircraftRole,
                Role = model.Role,
                IsPrimary = model.IsPrimary,
                Remarks = model.Remarks
            };

            _context.SortieCrews.Add(sortieCrew);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew member added successfully.";
            return RedirectToAction("Index", new { sortieId = model.SortieId });
        }

        // GET: SortieCrews/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sortieCrew = await _context.SortieCrews
                .Include(sc => sc.CrewMember)
                .ThenInclude(cm => cm.Person)
                .Include(sc => sc.Sortie)
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            var vm = new SortieCrewCreateVm
            {
                Id = sortieCrew.Id,
                SortieId = sortieCrew.SortieId,
                CrewMemberId = sortieCrew.CrewMemberId,
                Seat = sortieCrew.Seat,
                AircraftRole = sortieCrew.AircraftRole,
                Role = sortieCrew.Role,
                IsPrimary = sortieCrew.IsPrimary,
                Remarks = sortieCrew.Remarks,
                CrewMemberName = sortieCrew.CrewMember?.Captain,
                SortieCode = sortieCrew.Sortie?.SortieCode
            };

            // Get all crew members (including current one)
            var userSquadronId = await GetUserSquadronId();
            var crewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active &&
                       (userSquadronId == null || cm.SquadronId == userSquadronId))
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}",
                    Selected = cm.Id == sortieCrew.CrewMemberId
                })
                .ToListAsync();

            ViewBag.CrewMembers = crewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions();

            return View(vm);
        }

        // POST: SortieCrews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SortieCrewCreateVm model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model.SortieId, model.CrewMemberId);
                return View(model);
            }

            var sortieCrew = await _context.SortieCrews
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            // Check for duplicate assignment (if crew member changed)
            if (sortieCrew.CrewMemberId != model.CrewMemberId)
            {
                var existingAssignment = await _context.SortieCrews
                    .FirstOrDefaultAsync(sc => sc.SortieId == model.SortieId &&
                                               sc.CrewMemberId == model.CrewMemberId &&
                                               sc.Id != id);

                if (existingAssignment != null)
                {
                    ModelState.AddModelError("CrewMemberId", "This crew member is already assigned to this sortie.");
                    await PopulateDropdowns(model.SortieId, model.CrewMemberId);
                    return View(model);
                }
            }

            sortieCrew.CrewMemberId = model.CrewMemberId;
            sortieCrew.Seat = model.Seat;
            sortieCrew.AircraftRole = model.AircraftRole;
            sortieCrew.Role = model.Role;
            sortieCrew.IsPrimary = model.IsPrimary;
            sortieCrew.Remarks = model.Remarks;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew assignment updated successfully.";
            return RedirectToAction("Index", new { sortieId = model.SortieId });
        }

        // POST: SortieCrews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sortieCrew = await _context.SortieCrews
                .Include(sc => sc.Sortie)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (sortieCrew == null)
            {
                return NotFound();
            }

            var sortieId = sortieCrew.SortieId;

            _context.SortieCrews.Remove(sortieCrew);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Crew assignment removed successfully.";
            return RedirectToAction("Index", new { sortieId = sortieId });
        }

        // Helper Methods
        private async Task<int?> GetUserSquadronId()
        {
            // Implement based on your user authentication
            // Example: Get from claims or user profile
            var squadronClaim = User.FindFirst("SquadronId");
            if (squadronClaim != null && int.TryParse(squadronClaim.Value, out int squadronId))
            {
                return squadronId;
            }
            return null;
        }

        private async Task<int?> GetUserAcMainGroupId()
        {
            var acMainGroupClaim = User.FindFirst("AcMainGroupId");
            if (acMainGroupClaim != null && int.TryParse(acMainGroupClaim.Value, out int acMainGroupId))
            {
                return acMainGroupId;
            }
            return null;
        }

        private List<SelectListItem> GetSeatOptions()
        {
            return Enum.GetValues(typeof(CrewSeat))
                .Cast<CrewSeat>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                })
                .ToList();
        }

        private List<SelectListItem> GetAircraftRoleOptions()
        {
            // Assuming AircraftRole is an enum
            return Enum.GetValues(typeof(AircraftRole))
                .Cast<AircraftRole>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();
        }

        private async Task PopulateDropdowns(int sortieId, int? currentCrewMemberId = null)
        {
            var userSquadronId = await GetUserSquadronId();

            var crewMembers = await _context.CrewMembers
                .Include(cm => cm.Person)
                .Include(cm => cm.Squadron)
                .Where(cm => cm.Active &&
                       (userSquadronId == null || cm.SquadronId == userSquadronId))
                .OrderBy(cm => cm.Captain)
                .Select(cm => new SelectListItem
                {
                    Value = cm.Id.ToString(),
                    Text = $"{cm.Captain} ({cm.NickName}) - {cm.Squadron.Name}",
                    Selected = cm.Id == currentCrewMemberId
                })
                .ToListAsync();

            ViewBag.CrewMembers = crewMembers;
            ViewBag.Seats = GetSeatOptions();
            ViewBag.AircraftRoles = GetAircraftRoleOptions();
        }
    }
}