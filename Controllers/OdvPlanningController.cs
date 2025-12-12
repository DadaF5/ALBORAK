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
        // list / filter / eager-load for display
        [HttpGet("")]
        public async Task<IActionResult> Index(int? squadronId, DateTime? odvDate, int? acMainGroupId)
        {
            var vm = new OdvIndexVm
            {
                SelectedSquadronId = squadronId,
                SelectedDate = odvDate,
                SelectedAcMainGroupId = acMainGroupId
            };

            // populate select lists (small helper inline)
            vm.Squadrons = await _context.Squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToListAsync();

            vm.AcMainGroups = await _context.AcMainGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem(g.Name, g.Id.ToString()))
                .ToListAsync();

            vm.Missions = await _context.Missions
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
                .ToListAsync();

            vm.CallSigns = await _context.CallSigns
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem(c.Code, c.Id.ToString()))
                .ToListAsync();

            vm.Aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Registration)
                .Select(a => new SelectListItem(a.DisplayName, a.Id.ToString()))
                .ToListAsync();

            vm.CrewMembers = await _context.CrewMembers
                .OrderBy(cm => cm.NickName)
                .Select(cm => new SelectListItem(cm.Captain, cm.Id.ToString()))
                .ToListAsync();
            vm.ZoneList = Enum.GetValues(typeof(Enums.Zone))
                 .Cast<Enums.Zone>()
                 .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                 .ToList();
            vm.MissionTypeList = Enum.GetValues(typeof(Enums.MissionType))
                .Cast<Enums.MissionType>()
                .Select(mt => new SelectListItem { Value = ((int)mt).ToString(), Text = mt.ToString() })
                .ToList();
                
            // build query and eager load what index view needs
            var q = _context.Odvs
                .Include(o => o.Mission)
                .Include(o => o.AcMainGroup)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.Aircraft)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                            .ThenInclude(cm => cm.Person)
                .AsNoTracking()
                .AsQueryable();

            if (squadronId.HasValue) q = q.Where(o => o.SquadronId == squadronId.Value);
            if (odvDate.HasValue) q = q.Where(o => o.OdvDate.Date == odvDate.Value.Date);
            if (acMainGroupId.HasValue) q = q.Where(o => o.AcMainGroupId == acMainGroupId.Value);

            vm.Odvs = await q.OrderBy(o => o.Id).ToListAsync();
            return View("~/Views/Odvs/Index.cshtml", vm);
        }

        // POST: Create ODV header
        // Accepts OdvCreateVm (must exist in your ViewModels)
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] OdvCreateVm model)
        {
            _logger?.LogDebug("Create POST - Request URL: {Url}", Request.Path);
            _logger?.LogDebug("Create POST - Form keys: {@Form}", Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()));

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Errors
                    .Select(e => e.ErrorMessage + (e.Exception != null ? " | ex:" + e.Exception.Message : "")).ToArray());
                
                _logger?.LogWarning("Create POST - ModelState invalid: {@Errors}", errors);
                // repopulate selects and return Index to show validation
                return await Index(model.SquadronId, model.OdvDate, model.AcMainGroupId);
            }

            var odv = new Odv
            {
                SquadronId = model.SquadronId,
                MissionId = model.MissionId,
                OdvDate = model.OdvDate,
                Zone = model.Zone,
                MissionType = model.MissionType,
                Area = model.Area,
                Obs = model.Obs,
                CallSignId = model.CallSignId,
                AcMainGroupId = model.AcMainGroupId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Odvs.Add(odv);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index),
                new
                {
                    squadronId = odv.SquadronId,
                    odvDate = odv.OdvDate.Date,
                    acMainGroupId = odv.AcMainGroupId
                });
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
        private async Task PopulateOdvSelectLists(OdvIndexVm vm)
        {
            vm.Squadrons = await _context.Squadrons
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToListAsync();

            vm.AcMainGroups = await _context.AcMainGroups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem(g.Name, g.Id.ToString()))
                .ToListAsync();

            vm.Missions = await _context.Missions
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
                .ToListAsync();

            vm.CallSigns = await _context.CallSigns
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem(c.Code , c.Id.ToString()))
                .ToListAsync();

            vm.Aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Registration)
                .Select(a => new SelectListItem(a.DisplayName, a.Id.ToString()))
                .ToListAsync();

            vm.CrewMembers = await _context.CrewMembers
                .OrderBy(cm => cm.NickName)
                .Select(cm => new SelectListItem(cm.Captain, cm.Id.ToString()))
                .ToListAsync();
            // enum select lists can be populated in the view directly
            vm.ZoneList = Enum.GetValues(typeof(Enums.Zone))
                .Cast<Enums.Zone>()
                .Select(z => new SelectListItem { Value = ((int)z).ToString(), Text = z.ToString() })
                .ToList();

            vm.MissionTypeList = Enum.GetValues(typeof(Enums.MissionType))
                .Cast<Enums.MissionType>()
                .Select(mt => new SelectListItem { Value = ((int)mt).ToString(), Text = mt.ToString() })
                .ToList();
        }
    }
}