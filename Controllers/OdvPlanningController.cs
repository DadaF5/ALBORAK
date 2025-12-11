using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;

namespace FRAProject.Controllers
{
    [Route("Odvs")]
    public class OdvPlanningController : Controller
    {
        private readonly FRAContext _context;

        public OdvPlanningController(FRAContext context)
        {
            _context = context;
        }

        // GET: /Odvs
        [HttpGet("")]
        public async Task<IActionResult> Index(int? squadronId, DateTime? odvDate, int? acMainGroupId)
        {
            var vm = new OdvIndexVm
            {
                SelectedSquadronId = squadronId,
                SelectedDate = odvDate,
                SelectedAcMainGroupId = acMainGroupId,
                Squadrons = await _context.Squadrons.OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToListAsync(),
                AcMainGroups = await _context.AcMainGroups.OrderBy(g => g.Name).Select(g => new SelectListItem(g.Name, g.Id.ToString())).ToListAsync(),
                Missions = await _context.Missions.OrderBy(m => m.Name).Select(m => new SelectListItem(m.Name, m.Id.ToString())).ToListAsync(),
                CallSigns = await _context.CallSigns.OrderBy(c => c.Code).Select(c => new SelectListItem(c.Code, c.Code)).ToListAsync(),
                Aircrafts = await _context.Aircrafts.OrderBy(a => a.Registration).Select(a => new SelectListItem(a.DisplayName, a.Id.ToString())).ToListAsync(),
                CrewMembers = await _context.CrewMembers
                    .OrderBy(cm => cm.NickName)
                    .Select(cm => new SelectListItem(cm.Captain, cm.Id.ToString()))
                    .ToListAsync()
            };

            // Query ODVs eager loaded with sorties and crew (apply filters)
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
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOdv([FromForm] OdvCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate select-lists and re-render index with validation messages
                return await Index(model.SquadronId, model.OdvDate, model.AcMainGroupId);
            }

            var odv = new Odv
            {
                SquadronId = model.SquadronId,
                MissionId = model.MissionId,
                OdvDate = model.OdvDate,
                Zone = model.ZoneID,
                MissionType = model.MissionTypeId,
                Area = model.Area,
                Obs = model.Obs,
                CallSignId = model.CallSignId,
                AcMainGroupId = model.AcMainGroupId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Odvs.Add(odv);
            await _context.SaveChangesAsync();

            // Redirect to Index with the same filters and highlight the new ODV (simple redirect)
            return RedirectToAction(nameof(Index), 
                new { squadronId = odv.SquadronId, 
                    odvDate = odv.OdvDate.Date, 
                    acMainGroupId = odv.AcMainGroupId });
        }

        // POST: Create sortie attached to an ODV
        [HttpPost("AddSortie")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSortie([FromForm] SortieVm model)
        {
            if (!ModelState.IsValid)
            {
                // when validation fails, re-render Index (you can display a message)
                return RedirectToAction(nameof(Index), new { squadronId = Request.Form["SquadronId"].FirstOrDefault() });
            }

            var sortie = new Sortie
            {
                OdvId = model.OdvId,
                AircraftId = model.AircraftId,
                Configuration = model.Configuration,
                FuelQuantity = model.FuelQuantity,
                StartTime = model.StartTime,
                LandingTime = model.LandingTime,
                TOFF = model.TOFF,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "system",
                Status = SortieStatus.Planned
            };

            _context.Sorties.Add(sortie);
            await _context.SaveChangesAsync();

            // optionally create SortieCrews here if model contains one crew entry
            if (model.Crew != null)
            {
                foreach (var c in model.Crew.Where(c => c != null && c.CrewMemberId > 0))
                {
                    var cm = await _context.CrewMembers.FindAsync(c.CrewMemberId);
                    if (cm != null)
                    {
                        _context.SortieCrews.Add(new SortieCrew { SortieId = sortie.Id, CrewMemberId = cm.Id, Role = c.Role, IsPrimary = c.IsPrimary });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { squadronId = (await _context.Odvs.FindAsync(model.OdvId)).SquadronId, odvDate = DateTime.Now.Date });
        }

        // POST: Delete ODV (and cascade delete sorties/crews server-side)
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOdv(int id)
        {
            var odv = await _context.Odvs.Include(o => o.Sorties).ThenInclude(s => s.SortieCrews).FirstOrDefaultAsync(o => o.Id == id);
            if (odv == null) return NotFound();

            // Remove nested entities then odv
            var sortieIds = odv.Sorties.Select(s => s.Id).ToList();
            var assigns = await _context.SortieCrews.Where(sc => sortieIds.Contains(sc.SortieId)).ToListAsync();
            _context.SortieCrews.RemoveRange(assigns);
            _context.Sorties.RemoveRange(odv.Sorties);
            _context.Odvs.Remove(odv);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Additional endpoints for edit, details, add/remove crew, set real TOFF etc. can be added similarly.
    }
}
