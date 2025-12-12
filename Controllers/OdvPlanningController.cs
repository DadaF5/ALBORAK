using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<OdvsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public OdvPlanningController(FRAContext context, ILogger<OdvsController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: /Odvs
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new OdvIndexVm
            {
                CreateModel = new OdvCreateVm { OdvDate = DateTime.UtcNow.Date }
            };

            // If non-admin, prefill the user's squadron/ac-group here (claims or DB)
            if (!User.IsInRole("Admin") && !User.IsInRole("Planner"))
            {
                var squadClaim = User.FindFirst("SquadronId")?.Value;
                var agClaim = User.FindFirst("AcMainGroupId")?.Value;
                if (int.TryParse(squadClaim, out var sq)) vm.CreateModel.SquadronId = sq;
                if (int.TryParse(agClaim, out var ag)) vm.CreateModel.AcMainGroupId = ag;
                // alternatively fetch from DB: current user record
            }

            await PopulateSelectLists(vm);

            vm.Odvs = await _context.Odvs
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)
                .Include(o => o.Sorties).ThenInclude(s => s.Aircraft)
                .AsNoTracking()
                .OrderBy(o => o.Id)
                .ToListAsync();

            return View("~/Views/Odvs/Index.cshtml", vm);
        }

        // POST: Create ODV header
        // Note: we expect inputs prefixed with CreateModel.* in the modal form, so bind with the prefix.
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm, Bind(Prefix = "CreateModel")] OdvCreateVm model)
        {
            _logger?.LogDebug("Create POST (modal) - incoming model: {@Model}", model);

            // Enforce server-side restrictions: non-admin users cannot set Squadron/AcMainGroup
            if (!User.IsInRole("Admin") && !User.IsInRole("Planner"))
            {
                var squadClaim = User.FindFirst("SquadronId")?.Value;
                if (int.TryParse(squadClaim, out var sq)) model.SquadronId = sq;

                var agClaim = User.FindFirst("AcMainGroupId")?.Value;
                if (int.TryParse(agClaim, out var ag)) model.AcMainGroupId = ag;
            }

            if (!ModelState.IsValid)
            {
                // Prepare ModelState errors keyed to the form field names (CreateModel.Property)
                var errors = new Dictionary<string, string[]>();
                foreach (var kv in ModelState.Where(k => k.Value.Errors.Count > 0))
                {
                    var key = kv.Key;
                    // Ensure keys are prefixed as used in the form (CreateModel.PropertyName)
                    if (!key.StartsWith("CreateModel.", StringComparison.OrdinalIgnoreCase))
                        key = "CreateModel." + key;
                    errors[key] = kv.Value.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? (e.Exception?.Message ?? "Invalid value") : e.ErrorMessage).ToArray();
                }

                _logger?.LogWarning("Create POST - validation failed: {@Errors}", errors);
                return BadRequest(errors); // AJAX caller will handle errors
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
                TOFF=model.TOFF,
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
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Create POST - DbUpdateException saving ODV");
                // handle other DB errors (FK violations, etc.)
                return StatusCode(500, new Dictionary<string, string[]>
                {
                    { "CreateModel", new[] { "Unexpected database error saving the ODV. See server logs." } }
                });
            }

           
          
        }


        // GET: /Odvs/Edit/{id}
        //[HttpGet("Edit/{id:int}")]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var odv = await _context.Odvs
        //        .AsNoTracking()
        //        .Include(o => o.Sorties) // optional - included if Edit view needs sorties
        //        .FirstOrDefaultAsync(o => o.Id == id);

        //    if (odv == null) return NotFound();

        //    var vm = new OdvEditVm
        //    {
        //        Id = odv.Id,
        //        SquadronId = odv.SquadronId,
        //        MissionId = odv.MissionId,
        //        OdvDate = odv.OdvDate,
        //        Zone = odv.Zone,
        //        MissionType = odv.MissionType,
        //        Area = odv.Area,
        //        Obs = odv.Obs,
        //        CallSignId = odv.CallSignId,
        //        AcMainGroupId = odv.AcMainGroupId
        //    };

        //    await PopulateOdvSelectLists(vm);
        //    return View("~/Views/Odvs/Edit.cshtml", vm);
        //}

        // POST: /Odvs/Edit/{id}
        //[HttpPost("Edit/{id:int}")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateOdv(int id, [FromForm] OdvEditVm model)
        //{
        //    if (id != model.Id) return BadRequest();

        //    // repopulate selects if returning view
        //    await PopulateOdvSelectLists(model);

        //    if (!ModelState.IsValid)
        //    {
        //        return View("~/Views/Odvs/Edit.cshtml", model);
        //    }

        //    var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == id);
        //    if (odv == null) return NotFound();

        //    // map editable properties
        //    odv.SquadronId = model.SquadronId;
        //    odv.MissionId = model.MissionId;
        //    odv.OdvDate = model.OdvDate;
        //    odv.Zone = model.Zone;
        //    odv.MissionType = model.MissionType;
        //    odv.Area = model.Area;
        //    odv.Obs = model.Obs;
        //    odv.CallSignId = model.CallSignId;
        //    odv.AcMainGroupId = model.AcMainGroupId;
        //    odv.UpdatedAtUtc = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index),
        //        new
        //        {
        //            squadronId = odv.SquadronId,
        //            odvDate = odv.OdvDate.Date,
        //            acMainGroupId = odv.AcMainGroupId
        //        });
        //}

        // small helper to populate select lists used by Edit/Create/Index
        // Helper to populate select lists (enum lists, call signs etc.)
        private async Task PopulateSelectLists(OdvIndexVm vm)
        {
            vm.Squadrons = await _context.Squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            vm.AcMainGroups = await _context.AcMainGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();

            vm.Missions = await _context.Missions
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToListAsync();

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

            // enum lists (numeric values)
            vm.ZoneList = Enum.GetValues(typeof(FRAProject.Enums.Zone))
                .Cast<FRAProject.Enums.Zone>()
                .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                .ToList();

            vm.MissionTypeList = Enum.GetValues(typeof(FRAProject.Enums.MissionType))
                .Cast<FRAProject.Enums.MissionType>()
                .Select(m => new SelectListItem { Value = ((int)m).ToString(), Text = m.ToString() })
                .ToList();
        }
    }
}