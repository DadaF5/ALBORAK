using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Mapping;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    [Area("SquadronOps")]
    public class SortiesController : Controller
    {
        private readonly FRAContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public SortiesController(FRAContext context,
                                 UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Sorties/Create?odvId=123
        [HttpGet]
        public async Task<IActionResult> Create(int odvId)
        {
            var odv = await _context.Odvs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null)
                return NotFound();

            var vm = new SortieCreateVm
            {
                OdvId = odvId,
                Sequence = 1 // default, can be changed later
            };

            ViewBag.OdvInfo = $"{odv.MissionId} | {odv.OdvDate:yyyy-MM-dd}";
            await PopulateAcTypesForCurrentUser();
            return View(vm);
        }

        // POST: Sorties/Create?odvId=123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SortieCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAcTypesForCurrentUser();
                return View(model);
            }

            var sortie = new Sortie
            {
                OdvId = model.OdvId,
                SortieCode = model.SortieCode,
                Configuration = model.Configuration,
                Sequence = model.Sequence,
                AcTypeId = model.AcTypeId,
                Status = SortieStatus.Planned,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Sorties.Add(sortie);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "OdvPlanning",
                new { odvDate = DateTime.UtcNow.ToString("yyyy-MM-dd") }
            );
        }
        // GET: Sorties/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sortie == null)
                return NotFound();

            var vm = new SortieCreateVm
            {
                Id = sortie.Id,
                OdvId = sortie.OdvId,
                SortieCode = sortie.SortieCode,
                Configuration = sortie.Configuration,
                Sequence = sortie.Sequence,
                AcTypeId = sortie.AcTypeId, // This is the current type
                FuelQuantity = sortie.FuelQuantity
            };

            // Pass the current AcTypeId to ensure it appears in dropdown
            await PopulateAcTypesForCurrentUser(sortie.AcTypeId);

            return View(vm);
        }

        // POST: Sorties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SortieCreateVm model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                // Pass current AcTypeId to populate dropdown
                await PopulateAcTypesForCurrentUser(model.AcTypeId);
                return View(model);
            }

            var sortie = await _context.Sorties
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (sortie == null)
                return NotFound();

            // IMPORTANT: Check if user is allowed to change to this aircraft type
            var userAcMainGroupId = await GetUserAcMainGroupId();
            var canChangeAircraftType = true;

            if (userAcMainGroupId.HasValue && userAcMainGroupId.Value > 0)
            {
                // Check if the new aircraft type is in user's group
                var newAcType = await _context.AcTypes
                    .FirstOrDefaultAsync(t => t.Id == model.AcTypeId);

                if (newAcType != null && newAcType.AcMainGroupId != userAcMainGroupId.Value)
                {
                    // User is trying to select an aircraft type not in their group
                    // Only allow if they're keeping their current type
                    if (model.AcTypeId != sortie.AcTypeId)
                    {
                        ModelState.AddModelError("AcTypeId", "You cannot select an aircraft type outside your assigned group.");
                        canChangeAircraftType = false;
                    }
                }
            }

            if (!canChangeAircraftType)
            {
                await PopulateAcTypesForCurrentUser(model.AcTypeId);
                return View(model);
            }

            // Update sortie properties
            sortie.SortieCode = model.SortieCode;
            sortie.Configuration = model.Configuration;
            sortie.Sequence = model.Sequence;
            sortie.AcTypeId = model.AcTypeId;
            sortie.FuelQuantity = model.FuelQuantity;
            sortie.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "OdvPlanning");
        }

        // Get the current user's AcMainGroup and populate AcTypes accordingly
        private async Task<int?> GetUserAcMainGroupId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.AcMainGroupId; // this should match the ApplicationUser property type
        }


        // Populate aircraft types filter for current user 
        private bool UserCanSeeAllAircraftTypes()
        {
            // Users with these roles can see all aircraft types
            var allowedRoles = new[] { "Administrator", "SuperAdmin", "MaintenanceSupervisor", "FlightOpsManager" };

            return allowedRoles.Any(role => User.IsInRole(role));
        }

        private async Task PopulateAcTypesForCurrentUser(int? currentAcTypeId = null)
        {
            // Always start with an empty list
            var selectList = new List<SelectListItem>();

            // Get the current aircraft type if we have an ID
            AcType? currentAcType = null;
            if (currentAcTypeId.HasValue && currentAcTypeId.Value > 0)
            {
                currentAcType = await _context.AcTypes
                    .FirstOrDefaultAsync(t => t.Id == currentAcTypeId.Value);
            }

            // Get user's AcMainGroupId
            var userAcMainGroupId = await GetUserAcMainGroupId();

            if (userAcMainGroupId.HasValue && userAcMainGroupId.Value > 0)
            {
                // User has an AcMainGroup - get types from their group
                var userGroupTypes = await _context.AcTypes
                    .Where(t => t.AcMainGroupId == userAcMainGroupId.Value)
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                // Add types from user's group
                foreach (var type in userGroupTypes)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = type.Id.ToString(),
                        Text = type.Name,
                        Selected = type.Id == currentAcTypeId
                    });
                }

                // If current type exists but is NOT in user's group, add it with special marking
                if (currentAcType != null && !userGroupTypes.Any(t => t.Id == currentAcTypeId.Value))
                {
                    selectList.Insert(0, new SelectListItem
                    {
                        Value = currentAcType.Id.ToString(),
                        Text = $"{currentAcType.Name} (Currently Assigned)",
                        Selected = true
                    });
                }
            }
            else if (UserCanSeeAllAircraftTypes())
            {
                // Admin/Supervisor - show ALL aircraft types
                var allTypes = await _context.AcTypes
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                selectList = allTypes.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == currentAcTypeId
                }).ToList();
            }
            else
            {
                // Regular user without AcMainGroup
                // If they have a current type assigned, show ONLY that type
                if (currentAcType != null)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = currentAcType.Id.ToString(),
                        Text = $"{currentAcType.Name} (Currently Assigned)",
                        Selected = true
                    });

                    TempData["Warning"] = "You can only keep the currently assigned aircraft type. Contact an administrator to change it.";
                }
                else
                {
                    // No current type and no AcMainGroup
                    TempData["Warning"] = "No aircraft group assigned to your account. Please contact an administrator.";
                }
            }

            ViewBag.AcTypes = selectList;
        }
    }
}