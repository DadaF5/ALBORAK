using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    [Route("Odvs")]
    public class OdvPlanningController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<OdvPlanningController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public OdvPlanningController(
            FRAContext context,
            ILogger<OdvPlanningController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // =============================
        // Helpers (identity & scoping)
        // =============================

        private bool IsAdmin => User.IsInRole("Admin");

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                throw new InvalidOperationException("Authenticated user not found.");
            return user;
        }

        private async Task<int?> GetCurrentSquadronIdAsync()
        {
            if (IsAdmin)
                return null;

            var user = await GetCurrentUserAsync();

            if (!user.SquadronId.HasValue)
                throw new InvalidOperationException("SquadronId is not assigned to the current user.");

            return user.SquadronId.Value;
        }

        // =============================
        // GET: /Odvs
        // =============================
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new OdvIndexVm
            {
                CreateModel = new OdvCreateVm
                {
                    OdvDate = DateTime.UtcNow.Date
                }
            };

            // Prefill squadron / AC group for non-admin users
            if (!IsAdmin)
            {
                var user = await GetCurrentUserAsync();

                if (!user.SquadronId.HasValue)
                    throw new InvalidOperationException("Current user does not have a Squadron assigned.");

                if (!user.AcMainGroupId.HasValue)
                    throw new InvalidOperationException("Current user does not have an Aircraft Main Group assigned.");

                vm.CreateModel.SquadronId = user.SquadronId.Value;
                vm.CreateModel.AcMainGroupId = user.AcMainGroupId.Value;
            }

            await PopulateSelectListsAsync(vm);

            // Load existing ODVs (planning overview)
            vm.Odvs = await _context.Odvs
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.Aircraft)
                .AsNoTracking()
                .OrderByDescending(o => o.OdvDate)
                .ThenBy(o => o.Id)
                .ToListAsync();

            return View("~/Views/Odvs/Index.cshtml", vm);
        }

        // =============================
        // POST: /Odvs/Create (P1)
        // =============================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm, Bind(Prefix = "CreateModel")] OdvCreateVm model)
        {
            _logger.LogDebug("ODV Create POST: {@Model}", model);

            // Enforce squadron & AC group from server side for non-admins
            if (!IsAdmin)
            {
                var user = await GetCurrentUserAsync();

                if (!user.SquadronId.HasValue)
                    throw new InvalidOperationException("Current user does not have a Squadron assigned.");

                if (!user.AcMainGroupId.HasValue)
                    throw new InvalidOperationException("Current user does not have an Aircraft Main Group assigned.");

                model.SquadronId = user.SquadronId.Value;
                model.AcMainGroupId = user.AcMainGroupId.Value;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ExtractModelStateErrors("CreateModel"));
            }

            // Validate mission visibility/ownership
            var allowedMission = await IsMissionAllowedAsync(model.MissionId!, model.SquadronId);
            if (!allowedMission)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.MissionId", new[] { "Selected mission is not allowed." } }
                });
            }

            var odv = new Odv
            {
                SquadronId = model.SquadronId!,
                MissionId = model.MissionId!,
                OdvDate = model.OdvDate!,
                Zone = model.Zone,
                MissionType = model.MissionType,
                Area = model.Area,
                Obs = model.Obs,
                TOFF = model.TOFF,
                CallSignId = model.CallSignId,
                AcMainGroupId = model.AcMainGroupId!,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Odvs.Add(odv);

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = odv.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving ODV.");
                return StatusCode(500, new Dictionary<string, string[]>
                {
                    { "CreateModel", new[] { "Unexpected database error while saving the ODV." } }
                });
            }
        }

        // =============================
        // Select-list population
        // =============================
        private async Task PopulateSelectListsAsync(OdvIndexVm vm)
        {
            vm.Squadrons = await _context.Squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroups = await _context.AcMainGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();

            vm.Missions = await GetMissionSelectListAsync();

            vm.CallSigns = await _context.CallSigns
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                .ToListAsync();

            vm.Aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Registration)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.DisplayName })
                .ToListAsync();

            vm.CrewMembers = await _context.CrewMembers
                .OrderBy(cm => cm.NickName)
                .Select(cm => new SelectListItem { Value = cm.Id.ToString(), Text = cm.Captain })
                .ToListAsync();

            vm.ZoneList = Enum.GetValues(typeof(FRAProject.Enums.Zone))
                .Cast<FRAProject.Enums.Zone>()
                .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                .ToList();

            vm.MissionTypeList = Enum.GetValues(typeof(FRAProject.Enums.MissionType))
                .Cast<FRAProject.Enums.MissionType>()
                .Select(m => new SelectListItem { Value = ((int)m).ToString(), Text = m.ToString() })
                .ToList();
        }

        // =============================
        // Mission scoping helpers
        // =============================
        private async Task<List<SelectListItem>> GetMissionSelectListAsync()
        {
            int? squadronId = await GetCurrentSquadronIdAsync();

            IQueryable<Mission> query = _context.Missions
                .Where(m => m.IsActive);

            if (squadronId.HasValue)
            {
                query = query.Where(m => m.SquadronId == null || m.SquadronId == squadronId);
            }

            return await query
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                })
                .ToListAsync();
        }

        private async Task<bool> IsMissionAllowedAsync(int missionId, int? squadronId)
        {
            return await _context.Missions.AnyAsync(m =>
                m.Id == missionId &&
                m.IsActive &&
                (IsAdmin || m.SquadronId == null || m.SquadronId == squadronId));
        }

        // =============================
        // ModelState helper (AJAX)
        // =============================
        private Dictionary<string, string[]> ExtractModelStateErrors(string prefix)
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var kv in ModelState.Where(k => k.Value.Errors.Count > 0))
            {
                var key = kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? kv.Key
                    : $"{prefix}.{kv.Key}";

                errors[key] = kv.Value.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                        ? (e.Exception?.Message ?? "Invalid value")
                        : e.ErrorMessage)
                    .ToArray();
            }

            return errors;
        }
    }
}
