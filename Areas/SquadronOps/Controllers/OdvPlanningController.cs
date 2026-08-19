using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services;
using FRAProject.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace FRAProject.Areas.SquadronOps.Controllers
{
    // ⚠ Was [Authorize(Roles = "Admin,SquadronOps")] — "SquadronOps" is not
    // a real seeded AspNetRole. Replaced with real SquadronOpsRead/Write
    // policies. Also: scoping previously forced every non-Admin user to
    // exactly ONE squadron/AcMainGroup pulled from the legacy
    // ApplicationUser.SquadronId/AcMainGroupId fields — a user with a real
    // UserAssignment spanning a whole Wing (multiple squadrons) or multiple
    // AcMainGroups couldn't see or create ODVs outside that single legacy
    // value. This conversion switches to real scope-based filtering
    // (Base+AcMainGroup+Wing via UserAssignment), while still using the
    // legacy field as a Create-time default for convenience — cross-checked
    // against the real scope so a stale/incorrect legacy value can't grant
    // access it shouldn't.
    [Route("Odvplanning")]
    [Authorize(Policy = "SquadronOpsRead")]
    [Area("SquadronOps")]
    public class OdvPlanningController : Controller
    {
        private const string ModuleCode = "SQUADRONOPS";

        private readonly FRAContext _context;
        private readonly ILogger<OdvPlanningController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserScopeService _userScopeService;

        public OdvPlanningController(
            FRAContext context,
            ILogger<OdvPlanningController> logger,
            UserManager<ApplicationUser> userManager,
            IUserScopeService userScopeService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _userScopeService = userScopeService;
        }

        // =============================
        // GET: /Odvs
        // =============================
        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? odvDate)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            // Persist board date
            DateTime selectedDate;

            if (odvDate.HasValue)
            {
                selectedDate = odvDate.Value.Date;
                TempData["OdvDate"] = selectedDate.ToString("yyyy-MM-dd");
            }
            else if (TempData["OdvDate"] != null)
            {
                selectedDate = DateTime.Parse(TempData["OdvDate"]!.ToString()!);
                TempData.Keep("OdvDate");
            }
            else
            {
                selectedDate = DateTime.Today;
                TempData["OdvDate"] = selectedDate.ToString("yyyy-MM-dd");
            }

            var vm = new OdvIndexVm
            {
                SelectedDate = selectedDate,
                CreateModel = new OdvCreateVm
                {
                    OdvDate = selectedDate
                }
            };

            // Default the Create form's Squadron/AcMainGroup from the
            // user's legacy "home" fields, for convenience only — Create
            // itself re-validates against real scope regardless of what's
            // defaulted here.
            if (!scope.IsUnrestricted)
            {
                var user = await GetCurrentUserAsync();

                if (user.SquadronId.HasValue && user.AcMainGroupId.HasValue &&
                    await IsSquadronInScopeAsync(user.SquadronId.Value, scope) &&
                    scope.AllowedAcMainGroupIds.Contains(user.AcMainGroupId.Value))
                {
                    vm.CreateModel.SquadronId = user.SquadronId.Value;
                    vm.CreateModel.AcMainGroupId = user.AcMainGroupId.Value;

                    ViewBag.SquadronName = await _context.Squadrons
                        .Where(s => s.Id == user.SquadronId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync();

                    ViewBag.AcMainGroupName = await _context.AcMainGroups
                        .Where(g => g.Id == user.AcMainGroupId)
                        .Select(g => g.Name)
                        .FirstOrDefaultAsync();
                }
                // else: legacy field missing/stale/out of scope — leave
                // CreateModel unset, PopulateSelectListsAsync still offers
                // the user's real in-scope squadrons/groups to choose from.
            }

            // Populate dropdowns (scoped)
            await PopulateSelectListsAsync(vm, scope);

            // Load ODVs for that date + scope
            var odvQuery = _context.Odvs
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)
                .Include(o => o.CallSign)
                .Include(o => o.Sorties!)
                    .ThenInclude(s => s.AcType)
                .Include(o => o.Sorties!)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                .Where(o => o.OdvDate == selectedDate);

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                odvQuery = odvQuery.Where(o =>
                    allowedSquadronIds.Contains(o.SquadronId) &&
                    (!scope.AllowedAcMainGroupIds.Any() || scope.AllowedAcMainGroupIds.Contains(o.AcMainGroupId)));
            }

            vm.Odvs = await odvQuery
                .AsNoTracking()
                .OrderBy(o => o.TOFF)
                .ToListAsync();

            // Load AcTypes for Sortie creation
            vm.AcTypes = await _context.AcTypes
               .OrderBy(a => a.Name)
               .Select(a => new SelectListItem
               {
                   Value = a.Id.ToString(),
                   Text = a.Name
               })
               .ToListAsync();

            return View(vm);
        }

        // =============================
        // POST: /Odvs/Create (P1)
        // =============================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Create([FromForm, Bind(Prefix = "CreateModel")] OdvCreateVm model)
        {
            _logger.LogDebug("ODV Create POST: {@Model}", model);

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            bool acGroupExists = await _context.AcMainGroups
                .AnyAsync(g => g.Id == model.AcMainGroupId);

            if (!acGroupExists)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.AcMainGroupId", new[] { "Aircraft Main Group does not exist." } }
                });
            }

            if (model.AcMainGroupId <= 0)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.AcMainGroupId", new[] { "Aircraft Main Group is missing or invalid." } }
                });
            }

            // Real scope enforcement — replaces the old "force to the user's
            // single legacy squadron/group" behaviour. A scoped user can now
            // create an ODV for ANY squadron/AcMainGroup within their real
            // UserAssignment scope, not just their one legacy "home" value.
            // The dropdowns only offer in-scope options, but this is still a
            // posted value and must be re-validated server-side regardless.
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(model.SquadronId, scope))
                {
                    return BadRequest(new Dictionary<string, string[]>
                    {
                        { "CreateModel.SquadronId", new[] { "This squadron is outside your assigned scope." } }
                    });
                }

                if (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(model.AcMainGroupId))
                {
                    return BadRequest(new Dictionary<string, string[]>
                    {
                        { "CreateModel.AcMainGroupId", new[] { "This aircraft group is outside your assigned scope." } }
                    });
                }
            }

            // Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ExtractModelStateErrors("CreateModel"));
            }

            // Mission ownership check
            var missionAllowed = await IsMissionAllowedAsync(
                model.MissionId!,
                model.SquadronId,
                scope);

            if (!missionAllowed)
            {
                return BadRequest(new Dictionary<string, string[]>
                {
                    { "CreateModel.MissionId", new[] { "Mission not allowed." } }
                });
            }

            // Create ODV
            var odv = new Odv
            {
                SquadronId = model.SquadronId,
                AcMainGroupId = model.AcMainGroupId,
                MissionId = model.MissionId,
                OdvDate = model.OdvDate!.Date,
                Zone = model.Zone,
                MissionType = model.MissionType,
                Area = model.Area,
                TOFF = model.TOFF ?? TimeSpan.Zero,
                Obs = model.Obs,
                CallSignId = model.CallSignId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Odvs.Add(odv);
            await _context.SaveChangesAsync();

            // Redirect back to SAME DATE
            return RedirectToAction(nameof(Index), new
            {
                odvDate = model.OdvDate!.ToString("yyyy-MM-dd")
            });
        }

        // =============================
        // GET: /Odvs/Edit/{id}
        // =============================
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, DateTime? odvDate)
        {
            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);

            var odv = await _context.Odvs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null)
                return NotFound();

            // ⚠ This scope check did not exist before — Edit GET/POST had
            // no per-record authorization at all beyond the (effectively
            // dead) "SquadronOps" role check, so any authenticated
            // non-Admin user with real access to this controller could
            // reach any ODV by id.
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(odv.SquadronId, scope) ||
                    (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(odv.AcMainGroupId)))
                    return Forbid();
            }

            var vm = new OdvEditVm
            {
                Id = odv.Id,
                MissionId = odv.MissionId,
                CallSignId = odv.CallSignId,
                TOFF = odv.TOFF ?? TimeSpan.Zero,
                Area = odv.Area,
                Obs = odv.Obs
            };

            // reuse dropdown population
            ViewBag.Missions = await _context.Missions
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToListAsync();

            ViewBag.CallSigns = await _context.CallSigns
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                .ToListAsync();

            ViewBag.ReturnDate = odvDate;

            return View(vm);
        }

        // Edit POST
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SquadronOpsWrite")]
        public async Task<IActionResult> Edit(int id, OdvEditVm model, DateTime? odvDate)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == id);
            if (odv == null)
                return NotFound();

            var scope = await _userScopeService.GetScopeAsync(User, ModuleCode);
            if (!scope.IsUnrestricted)
            {
                if (!await IsSquadronInScopeAsync(odv.SquadronId, scope) ||
                    (scope.AllowedAcMainGroupIds.Any() && !scope.AllowedAcMainGroupIds.Contains(odv.AcMainGroupId)))
                    return Forbid();
            }

            odv.MissionId = model.MissionId;
            odv.CallSignId = model.CallSignId;
            odv.TOFF = model.TOFF;
            odv.Area = model.Area;
            odv.Obs = model.Obs;
            odv.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { odvDate });
        }

        // =============================
        // Select-list population
        // =============================
        private async Task PopulateSelectListsAsync(OdvIndexVm vm, UserScope scope)
        {
            var squadronsQuery = _context.Squadrons.AsQueryable();
            var acMainGroupsQuery = _context.AcMainGroups.AsQueryable();
            var callSignsQuery = _context.CallSigns.AsQueryable();
            var aircraftsQuery = _context.Aircrafts.AsQueryable();
            var crewMembersQuery = _context.CrewMembers.AsQueryable();

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                squadronsQuery = squadronsQuery.Where(s => allowedSquadronIds.Contains(s.Id));
                crewMembersQuery = crewMembersQuery.Where(cm => allowedSquadronIds.Contains(cm.SquadronId));

                if (scope.AllowedAcMainGroupIds.Any())
                {
                    acMainGroupsQuery = acMainGroupsQuery.Where(g => scope.AllowedAcMainGroupIds.Contains(g.Id));
                    aircraftsQuery = aircraftsQuery.Where(a => a.AcType != null && scope.AllowedAcMainGroupIds.Contains(a.AcType.AcMainGroupId));
                }

                callSignsQuery = callSignsQuery.Where(c =>
                    c.BaseId == null || scope.AllowedBaseIds.Contains(c.BaseId.Value));
            }

            vm.Squadrons = await squadronsQuery
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroups = await acMainGroupsQuery
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();

            vm.Missions = await GetMissionSelectListAsync(scope);

            vm.CallSigns = await callSignsQuery
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
                .ToListAsync();

            vm.Aircrafts = await aircraftsQuery
                .OrderBy(a => a.Registration)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Registration })
                .ToListAsync();

            vm.CrewMembers = await crewMembersQuery
                .OrderBy(cm => cm.NickName)
                .Select(cm => new SelectListItem { Value = cm.Id.ToString(), Text = cm.Captain })
                .ToListAsync();

            vm.ZoneList = Enum.GetValues(typeof(Enums.Zone))
                .Cast<Enums.Zone>()
                .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                .ToList();

            vm.MissionTypeList = Enum.GetValues(typeof(Enums.MissionType))
                .Cast<Enums.MissionType>()
                .Select(m => new SelectListItem { Value = ((int)m).ToString(), Text = m.ToString() })
                .ToList();
        }

        // =============================
        // Mission scoping helpers
        // =============================
        private async Task<List<SelectListItem>> GetMissionSelectListAsync(UserScope scope)
        {
            IQueryable<Mission> query = _context.Missions.Where(m => m.IsActive);

            if (!scope.IsUnrestricted)
            {
                var allowedSquadronIds = await GetInScopeSquadronIdsAsync(scope);
                query = query.Where(m => m.SquadronId == null || allowedSquadronIds.Contains(m.SquadronId.Value));
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

        private async Task<bool> IsMissionAllowedAsync(int missionId, int squadronId, UserScope scope)
        {
            var mission = await _context.Missions.FirstOrDefaultAsync(m => m.Id == missionId && m.IsActive);
            if (mission == null) return false;
            if (scope.IsUnrestricted) return true;
            if (mission.SquadronId == null) return true; // global mission

            // A global-scope match on the mission's OWN squadron isn't
            // enough on its own — the ODV's target squadron must also
            // match the mission's squadron, same intent as the original
            // single-squadron check, just expressed against the posted
            // squadronId instead of a forced legacy value.
            return mission.SquadronId == squadronId && await IsSquadronInScopeAsync(squadronId, scope);
        }

        // =============================
        // Scope helpers
        // =============================
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                throw new InvalidOperationException("Authenticated user not found.");
            return user;
        }

        private async Task<bool> IsSquadronInScopeAsync(int squadronId, UserScope scope)
        {
            if (scope.IsUnrestricted) return true;

            var info = await (from s in _context.Set<Squadron>()
                               join w in _context.Set<Wing>() on s.WingId equals w.Id
                               join d in _context.Set<Department>() on w.DepartmentId equals d.Id
                               where s.Id == squadronId
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

        // =============================
        // ModelState helper (AJAX)
        // =============================
        private Dictionary<string, string[]> ExtractModelStateErrors(string prefix)
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var kv in ModelState)
            {
                if (kv.Value == null || kv.Value.Errors.Count == 0) continue;

                var key = kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? kv.Key
                    : $"{prefix}.{kv.Key}";

                errors[key] = kv.Value.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                        ? e.Exception?.Message ?? "Invalid value"
                        : e.ErrorMessage)
                    .ToArray();
            }

            return errors;
        }
    }
}
