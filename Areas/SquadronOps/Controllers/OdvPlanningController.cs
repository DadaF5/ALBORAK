using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Data;
using FRAProject.Models;
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
    [Route("Odvplanning")]
    
    [Authorize(Roles = "Admin,SquadronOps")]
    [Area("SquadronOps")]
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
        public async Task<IActionResult> Index(DateTime? odvDate)
        {
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
            // 1️⃣ Determine effective date
            //var selectedDate = (odvDate ?? DateTime.UtcNow).Date;
           
            var vm = new OdvIndexVm
            {
                SelectedDate = selectedDate,
                CreateModel = new OdvCreateVm
                {
                    OdvDate = selectedDate
                }
            };

            
           
            // 2️⃣ Apply user scope
            if (!IsAdmin)
            {
                var user = await GetCurrentUserAsync();

                if (!user.SquadronId.HasValue || !user.AcMainGroupId.HasValue)
                    throw new InvalidOperationException("User is not properly configured.");

                vm.CreateModel.SquadronId = user.SquadronId.Value;
                vm.CreateModel.AcMainGroupId = user.AcMainGroupId.Value;

                // 🔹 For display purposes only
                ViewBag.SquadronName = await _context.Squadrons
                    .Where(s => s.Id == user.SquadronId)
                    .Select(s => s.Name)
                    .FirstAsync();

                ViewBag.AcMainGroupName = await _context.AcMainGroups
                    .Where(g => g.Id == user.AcMainGroupId)
                    .Select(g => g.Name)
                    .FirstAsync();

            }
           
            // 3️⃣ Populate dropdowns
            await PopulateSelectListsAsync(vm);

            // 4️⃣ Load ODVs for that date + scope
            var odvQuery = _context.Odvs
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)  
                .Include(o=> o.CallSign)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.AcType)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)

                .Where(o => o.OdvDate == selectedDate);

           

            if (!IsAdmin)
            {
                odvQuery = odvQuery.Where(o =>
                    o.SquadronId == vm.CreateModel.SquadronId &&
                    o.AcMainGroupId == vm.CreateModel.AcMainGroupId);
            }

            // Execute query
            vm.Odvs = await odvQuery
                .AsNoTracking()                
                .OrderBy(o => o.TOFF)
                .ToListAsync();

            // 5️⃣ Load AcTypes for Sortie creation
            vm.AcTypes = await _context.AcTypes
               .OrderBy(a => a.Name)
               .Select(a => new SelectListItem
               {
                   Value = a.Id.ToString(),
                   Text = a.Name   // e.g. F-16C, F-16D
               })
               .ToListAsync();
           


            return View(vm);
        }   

        // =============================
        // POST: /Odvs/Create (P1)
        // =============================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm, Bind(Prefix = "CreateModel")] OdvCreateVm model)
        {
            _logger.LogDebug("ODV Create POST: {@Model}", model);

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
            // 1️⃣ Enforce scope for non-admin
            if (!IsAdmin)
            {
                var user = await GetCurrentUserAsync();

                if (!user.SquadronId.HasValue || !user.AcMainGroupId.HasValue)
                    throw new InvalidOperationException("User is not properly configured.");

                model.SquadronId = user.SquadronId.Value;
                model.AcMainGroupId = user.AcMainGroupId.Value;
            }

            // 2️⃣ Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ExtractModelStateErrors("CreateModel"));
            }

            // 3️⃣ Mission ownership check
            var missionAllowed = await IsMissionAllowedAsync(
                model.MissionId!,
                model.SquadronId);

            if (!missionAllowed)
            {
                return BadRequest(new Dictionary<string, string[]>
            {
            { "CreateModel.MissionId", new[] { "Mission not allowed." } }
                });
            }

            // 4️⃣ Create ODV
            var odv = new Odv
            {
                SquadronId = model.SquadronId,
                AcMainGroupId = model.AcMainGroupId,
                MissionId = model.MissionId,
                OdvDate = model.OdvDate!.Date,
                Zone = model.Zone,
                MissionType = model.MissionType,
                Area = model.Area,
                TOFF = model.TOFF,
                Obs = model.Obs,
                CallSignId = model.CallSignId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Odvs.Add(odv);
            await _context.SaveChangesAsync();

            // 5️⃣ Redirect back to SAME DATE
            return RedirectToAction(nameof(Index), new
            {
                odvDate = model.OdvDate!.ToString("yyyy-MM-dd")
            });
        }

        // =============================
        // GET: /Odvs/Edit/{id}

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, DateTime? odvDate)
        {
            var odv = await _context.Odvs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null)
                return NotFound();

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

        // Edit POST action would go here...
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OdvEditVm model, DateTime? odvDate)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == id);
            if (odv == null)
                return NotFound();

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
                        ? e.Exception?.Message ?? "Invalid value"
                        : e.ErrorMessage)
                    .ToArray();
            }

            return errors;
        }
    }
}
