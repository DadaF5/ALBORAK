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
    // Full ODV controller with create/edit that supports modal workflows (AJAX partials)
    // Usage pattern:
    // - Index page shows list of ODV records.
    // - Links/buttons for Create/Edit point to /Odv/Create?modal=true or /Odv/Edit/{id}?modal=true
    // - Client JS fetches the partial and displays a bootstrap modal.
    // - POST requests from the modal are sent as normal form posts. If posted via AJAX (X-Requested-With)
    //   the controller will return either the modal partial (with validation errors) or JSON { success: true }.
    // - If the request is non-AJAX the controller will render the full page views as a fallback.
    public class OdvController : Controller
    {
        private readonly FRAContext _context;

        public OdvController(FRAContext context)
        {
            _context = context;
        }

        // GET: Odv
        public async Task<IActionResult> Index()
        {
            var list = await _context.Odvs
                .Include(o => o.Squadron)
                .Include(o => o.Mission)
                .OrderByDescending(o => o.OdvDate)
                .AsNoTracking()
                .ToListAsync();

            return View(list);
        }

        // GET: Odv/Create
        // If requested as AJAX (X-Requested-With) or with ?modal=true, returns a partial view with modal markup.
        public async Task<IActionResult> Create()
        {
            var vm = new OdvCreateVm
            {
                OdvDate = DateTime.UtcNow.Date
            };

            await PopulateSelectListsAsync();

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var modalQuery = (Request.Query["modal"].ToString() ?? "").ToLowerInvariant() == "true";

            if (isAjax || modalQuery)
            {
                return PartialView("_CreateEditModal", vm);
            }

            return View(vm);
        }

        // POST: Odv/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OdvCreateVm vm)
        {
            vm.Sorties ??= new List<SortieVm>();

            // Basic server-side validation
            if (!vm.Sorties.Any()) ModelState.AddModelError(string.Empty, "Please add at least one sortie.");

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                // If AJAX, return partial so client can re-render modal with validation summary
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_CreateEditModal", vm);
                }
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var odv = new Odv
                {
                    SquadronId = vm.SquadronID,
                    MissionId = vm.MissionId,
                    OdvDate = vm.OdvDate,
                    Zone = vm.ZoneID,
                    MissionType = vm.MissionTypeID,
                    Area = vm.Area,
                    OdvStatus = vm.OdvStatus,
                    TOFF = vm.TOFF,
                    AcMainGroupId = vm.AcMainGroupID,
                    CallSign = vm.CallSignId,
                    Obs = vm.Obs,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Odvs.Add(odv);
                await _context.SaveChangesAsync();

                // Create sorties and crew assignments
                foreach (var sVm in vm.Sorties)
                {
                    var sortie = new Sortie
                    {
                        OdvId = odv.Id,
                        AircraftId = sVm.AircraftId,
                        Configuration = sVm.Configuration,
                        FuelQuantity = sVm.FuelQuantity,
                        StartTime = sVm.StartTime,
                        LandingTime = sVm.LandingTime,
                        TOFF = sVm.TOFF,
                        Notes = sVm.Notes,
                        CompletedAtUtc = DateTime.UtcNow
                    };

                    _context.Sorties.Add(sortie);
                    await _context.SaveChangesAsync();

                    if (sVm.Crew != null)
                    {
                        foreach (var c in sVm.Crew)
                        {
                            // if crew member selection uses PersonId, map person -> crew member id if necessary
                            int? crewMemberId = null;

                            if (c.PersonId != 0)
                            {
                                // Try to treat PersonId as CrewMember.Id first (most common).
                                var cm = await _context.CrewMembers.FindAsync(c.PersonId);
                                if (cm != null) crewMemberId = cm.Id;
                                else
                                {
                                    // Fallback: if PersonId was actually a Person.Id, attempt to find CrewMember for that Person
                                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == c.PersonId);
                                    if (cmByPerson != null) crewMemberId = cmByPerson.Id;
                                }
                            }

                            if (!crewMemberId.HasValue) continue;

                            var assignment = new SortieCrew
                            {
                                SortieId = sortie.Id,
                                CrewMemberId = crewMemberId.Value,
                                Role = c.Role,
                                IsPrimary = c.IsPrimary
                            };
                            _context.SortieCrews.Add(assignment);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                await tx.CommitAsync();

                // If AJAX return JSON success; client can close modal and update UI
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, id = odv.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to create ODV: " + ex.Message);
                await PopulateSelectListsAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_CreateEditModal", vm);
                }

                return View(vm);
            }
        }

        // GET: Odv/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var odv = await _context.Odvs
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null) return NotFound();

            // map domain -> OdvCreateVm
            var vm = new OdvCreateVm
            {
                SquadronID = odv.SquadronId,
                MissionId = odv.MissionId,
                OdvDate = odv.OdvDate,
                ZoneID = odv.Zone,
                MissionTypeID = odv.MissionType,
                Area = odv.Area,
                OdvStatus = odv.OdvStatus,
                TOFF = odv.TOFF,
                AcMainGroupID = odv.AcMainGroupId,
                CallSignId = odv.CallSign,
                Obs = odv.Obs,
                Sorties = odv.Sorties?.Select(s => new SortieVm
                {
                    SortieId = s.Id,
                    AircraftId = s.AircraftId,
                    Configuration = s.Configuration,
                    FuelQuantity = s.FuelQuantity,
                    StartTime = s.StartTime,
                    LandingTime = s.LandingTime,
                    TOFF = s.TOFF,
                    Notes = s.Notes,
                    Crew = s.SortieCrews?.Select(a => new SortieCrewVm
                    {
                        PersonId = a.CrewMemberId, // will try to map to CrewMemberId on save
                        Role = a.Role,
                        IsPrimary = a.IsPrimary
                    }).ToList() ?? new System.Collections.Generic.List<SortieCrewVm>()
                }).ToList() ?? new System.Collections.Generic.List<SortieVm>()
            };

            await PopulateSelectListsAsync();

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var modalQuery = (Request.Query["modal"].ToString() ?? "").ToLowerInvariant() == "true";

            if (isAjax || modalQuery)
            {
                // Partial with modal markup for editing
                return PartialView("_CreateEditModal", vm);
            }

            return View(vm);
        }

        // POST: Odv/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OdvCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_CreateEditModal", vm);
                }
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var odv = await _context.Odvs
                    .Include(o => o.Sorties)
                        .ThenInclude(s => s.SortieCrews)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (odv == null) return NotFound();

                // update header
                odv.SquadronId = vm.SquadronID;
                odv.MissionId = vm.MissionId;
                odv.OdvDate = vm.OdvDate;
                odv.Zone = vm.ZoneID;
                odv.MissionType = vm.MissionTypeID;
                odv.Area = vm.Area;
                odv.OdvStatus = vm.OdvStatus;
                odv.TOFF = vm.TOFF;
                odv.AcMainGroupId = vm.AcMainGroupID;
                odv.CallSign = vm.CallSignId;
                odv.Obs = vm.Obs;
                odv.UpdatedAtUtc = DateTime.UtcNow;

                // Simple approach: remove existing sorties & assignments for this ODV and recreate from VM
                // (adjust to do diffs if you prefer)
                var existingSorties = odv.Sorties?.ToList() ?? new System.Collections.Generic.List<Sortie>();
                foreach (var es in existingSorties)
                {
                    // remove assignments
                    var assigns = await _context.SortieCrews.Where(a => a.SortieId == es.Id).ToListAsync();
                    _context.SortieCrews.RemoveRange(assigns);
                    // remove sortie
                    _context.Sorties.Remove(es);
                }
                await _context.SaveChangesAsync();

                // create new sorties from vm
                foreach (var sVm in vm.Sorties)
                {
                    var sortie = new Sortie
                    {
                        OdvId = odv.Id,
                        AircraftId = sVm.AircraftId,
                        Configuration = sVm.Configuration,
                        FuelQuantity = sVm.FuelQuantity,
                        StartTime = sVm.StartTime,
                        LandingTime = sVm.LandingTime,
                        TOFF = sVm.TOFF,
                        Notes = sVm.Notes,
                        CompletedAtUtc = DateTime.UtcNow
                    };
                    _context.Sorties.Add(sortie);
                    await _context.SaveChangesAsync();

                    if (sVm.Crew != null)
                    {
                        foreach (var c in sVm.Crew)
                        {
                            int? crewMemberId = null;
                            if (c.PersonId != 0)
                            {
                                var cm = await _context.CrewMembers.FindAsync(c.PersonId);
                                if (cm != null) crewMemberId = cm.Id;
                                else
                                {
                                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == c.PersonId);
                                    if (cmByPerson != null) crewMemberId = cmByPerson.Id;
                                }
                            }

                            if (!crewMemberId.HasValue) continue;

                            var assignment = new SortieCrew
                            {
                                SortieId = sortie.Id,
                                CrewMemberId = crewMemberId.Value,
                                Role = c.Role,
                                IsPrimary = c.IsPrimary
                            };
                            _context.SortieCrews.Add(assignment);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                _context.Odvs.Update(odv);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, id = odv.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to save ODV: " + ex.Message);
                await PopulateSelectListsAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_CreateEditModal", vm);
                }

                return View(vm);
            }
        }

        // Optional: a simple Details endpoint that returns full view or modal partial as in earlier patterns
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var odv = await _context.Odvs
                .Include(o => o.Squadron)
                .Include(o => o.Mission)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(a => a.CrewMember)
                            .ThenInclude(cm => cm.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null) return NotFound();

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var modalQuery = (Request.Query["modal"].ToString() ?? "").ToLowerInvariant() == "true";

            if (isAjax || modalQuery)
            {
                return PartialView("_DetailsModal", odv);
            }

            return View(odv);
        }

        // Helper that populates select lists for Create/Edit views
        private async Task PopulateSelectListsAsync()
        {
            var squadrons = await _context.Squadrons.OrderBy(s => s.Name).Select(s => new { s.Id, s.Name }).ToListAsync();
            var missions = await _context.Missions.OrderBy(m => m.Name).Select(m => new { m.Id, m.Name }).ToListAsync();
            var acs = await _context.Aircrafts.OrderBy(a => a.AcType).Select(a => new { a.Id, Display = a.AcType + " / " + a.Registration }).ToListAsync();

            var crew = await _context.CrewMembers
                .Include(cm => cm.Person)
                .OrderBy(cm => cm.NickName)
                .Select(cm => new { cm.Id, Display = (cm.NickName ?? "") + (cm.Person != null ? " (" + cm.Person.FirstName + " " + cm.Person.LastName + ")" : "") })
                .ToListAsync();

            ViewData["Squadrons"] = new SelectList(squadrons, "Id", "Name");
            ViewData["Missions"] = new SelectList(missions, "Id", "Name");
            ViewData["Aircrafts"] = new SelectList(acs, "Id", "Display");
            ViewData["CrewMembers"] = new SelectList(crew, "Id", "Display");

            // Enums to select lists (Zone, MissionType, OdvStatus) can be added similarly if needed by your views
        }
    }
}