using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using FRAProject.Areas.SquadronOps.ViewModels;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    // Main controller consolidated: CRUD for ODV + inline sortie APIs + workflow endpoints
    // NOTE: This file expects the following types to exist in your project:
    // - FRAContext (DbContext) with DbSet<Odv>, DbSet<Sortie>, DbSet<SortieCrew>, DbSet<CrewMember>, DbSet<Aircraft>, DbSet<Squadron>, DbSet<Mission>
    // - Models: Odv, Sortie, SortieCrew, CrewMember, Aircraft, ApplicationUser (Identity user) and enums AircraftStatus, SortieStatus
    // - ViewModels: OdvCreateVm, SortieVm, SortieCrewVm, AircraftSelectVm, SortieFinalizeVm
    // - The controller uses optimistic concurrency (RowVersion) where model has [Timestamp] properties.
    [Authorize] // Protect controller globally; action-level attributes further refine roles
    public class OdvsController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<OdvsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public OdvsController(FRAContext context, ILogger<OdvsController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Odvs
        public async Task<IActionResult> Index()
        {
            await PopulateSelectListsAsync();

            var list = await _context.Odvs
                .Include(o => o.Squadron)
                .Include(o => o.Mission)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.Aircraft)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                            .ThenInclude(cm => cm.Person)
                .OrderByDescending(o => o.OdvDate)
                .AsNoTracking()
                .ToListAsync();

            // optional prefill values from ApplicationUser
            try
            {
                var appUser = await _userManager.GetUserAsync(User);
                if (appUser != null)
                {
                    ViewData["PrefillSquadronId"] = appUser.SquadronId?.ToString() ?? string.Empty;
                    ViewData["PrefillBaseId"] = appUser.BaseId?.ToString() ?? string.Empty;
                    ViewData["PrefillAcMainGroupId"] = appUser.AcMainGroupId?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "GetUserAsync failed while preparing Index prefill values.");
            }

            return View(list);
        }

        // GET: Odvs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var odv = await _context.Odvs
                .Include(o => o.Squadron)
                .Include(o => o.Mission)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                            .ThenInclude(cm => cm.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null) return NotFound();

            if (IsAjaxRequest() || QueryModalFlag()) return PartialView("_DetailsModal", odv);
            return View(odv);
        }

        //--------------------------------------
        //---................-------------------------
        // GET: /Odvs/AddSortieModal?odvId=123
        [HttpGet]
        [Authorize(Roles = "Squadron,CrewChief")]
        public async Task<IActionResult> AddSortieModal(int odvId)
        {
            var odv = await _context.Odvs.AsNoTracking().FirstOrDefaultAsync(o => o.Id == odvId);
            if (odv == null) return NotFound();

            var vm = new SortieVm
            {
                // prefill TOFF/planned date etc. if desired
            };

            ViewBag.Aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Registration)
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(a.DisplayName, a.Id.ToString()))
                .ToListAsync();

            return PartialView("_AddSortieModal", (odvId, vm));
        }

        // POST: /Odvs/AddSortie (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Squadron,CrewChief, Admin")]
        public async Task<IActionResult> AddSortie(int odvId, SortieVm vm)
        {
            if (odvId <= 0 || vm == null) return BadRequest(new { success = false, error = "Invalid data" });

            var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == odvId);
            if (odv == null) return NotFound(new { success = false, error = "ODV not found" });

            // Basic server validation
            if (!ModelState.IsValid)
            {
                // return validation failures
                var errors = ModelState.Where(kvp => kvp.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { success = false, errors });
            }

            try
            {
                var now = DateTime.UtcNow;
                var sortie = new Sortie
                {
                    OdvId = odvId,
                    AircraftId = vm.AircraftId,
                    Configuration = vm.Configuration,
                    FuelQuantity = vm.FuelQuantity,
                    StartTime = vm.StartTime,
                    LandingTime = vm.LandingTime,
                    TOFF = vm.TOFF,
                    CreatedAtUtc = now,
                    CreatedBy = GetCurrentUserName(),
                    Status = SortieStatus.Planned
                };

                _context.Sorties.Add(sortie);
                await _context.SaveChangesAsync();

                // render the new sortie row partial to HTML to send back
                var sortieHtml = await this.RenderViewAsync("_SortieRowPartial", sortie, true);

                return Json(new { success = true, sortieId = sortie.Id, sortieHtml });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddSortie failed for ODV {OdvId}", odvId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // Helper: Render an ODV row partial (useful to refresh the entire ODV row if needed)
        [HttpGet]
        [Authorize(Roles = "Squadron,CrewChief,Tower,Admin")]
        public async Task<IActionResult> OdvRowPartial(int odvId)
        {
            var odv = await _context.Odvs
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.Aircraft)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null) return NotFound();
            return PartialView("_OdvRowPartial", odv);
        }

        //---.................------------------------
        // Add these methods to your OdvsController (same partial)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Squadron,CrewChief,Admin")]
        public async Task<IActionResult> CreateHeader([FromForm] OdvCreateVm vm)
        {
            if (vm == null) return BadRequest(new { success = false, error = "Invalid payload" });

            // Basic model state validation first
            if (!ModelState.IsValid)
            {
                var msErrors = ModelState.Where(kvp => kvp.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { success = false, errors = msErrors });
            }

            // Validate referenced FK ids exist (Squadron, Mission, AcMainGroup)
            var errors = new Dictionary<string, string[]>();

            if (vm.SquadronId <= 0 || !await _context.Squadrons.AnyAsync(s => s.Id == vm.SquadronId))
            {
                errors["SquadronId"] = new[] { "Please select a valid Squadron." };
            }

            if (vm.MissionId <= 0 || !await _context.Missions.AnyAsync(m => m.Id == vm.MissionId))
            {
                errors["MissionId"] = new[] { "Please select a valid Mission." };
            }

            // If AcMainGroupId is required by the DB (non-nullable FK), enforce it here.
            // If it's optional in your schema, you can skip this check or allow vm.AcMainGroupId == 0 to map to null.
            if (vm.AcMainGroupId <= 0 || !await _context.AcMainGroups.AnyAsync(a => a.Id == vm.AcMainGroupId))
            {
                errors["AcMainGroupId"] = new[] { "Please select a valid Aircraft Main Group." };
            }

            if (errors.Any())
            {
                // merge into ModelState-like structure for client consumption
                return BadRequest(new { success = false, errors });
            }

            try
            {
                var now = DateTime.UtcNow;
                var odv = new Odv
                {
                    SquadronId = vm.SquadronId,
                    MissionId = vm.MissionId,
                    OdvDate = vm.OdvDate,
                    Zone = vm.Zone,
                    MissionType = vm.MissionType,
                    Area = vm.Area,
                    Obs = vm.Obs,
                    CallSignId = vm.CallSignId,
                    AcMainGroupId = vm.AcMainGroupId, // safe now because we validated it exists
                    CreatedAtUtc = now
                };
                SetCreatedAudit(odv);

                _context.Odvs.Add(odv);
                await _context.SaveChangesAsync();

                return Json(new { success = true, odvId = odv.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateHeader failed while saving ODV header (debug).");

                var inner = ex.InnerException?.Message ?? ex.GetBaseException()?.Message ?? ex.Message;
                return StatusCode(500, new
                {
                    success = false,
                    error = "Save failed. See 'details' for DB error (development only).",
                    details = inner
                });
            }
        }

        // GET: Odvs/Create
        [HttpGet]
        [Authorize(Roles = "Squadron,CrewChief, Admin")]
        public async Task<IActionResult> Create()
        {
            // prepare vm with a single empty sortie input so the dynamic UI has an initial row
            var vm = new OdvCreateVm();          

            await PopulateSelectListsAsync();

            // no planned ODV list initially (page will fetch when user selects squadron/date)
            ViewBag.PlannedOdvsHtml = string.Empty;

            return View(vm);
        }
        // GET: Odvs/PlannedOdvs?squadronId=1&odvDate=2025-12-10
        // Returns a partial view showing planned ODVs -> sorties -> crew for the squadron on a specific date.
        [HttpGet]
        [Authorize(Roles = "Squadron,CrewChief,Tower,Admin")]
        public async Task<IActionResult> PlannedOdvs(int squadronId, DateTime odvDate)
        {
            if (squadronId <= 0) return BadRequest();

            var odvs = await _context.Odvs
                .Where(o => o.SquadronId == squadronId && o.OdvDate.Date == odvDate.Date)
                .OrderBy(o => o.Id)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                        .ThenInclude(sc => sc.CrewMember)
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.Aircraft)
                .AsNoTracking()
                .ToListAsync();

            // If none found return an empty partial (lower area stays blank)
            if (odvs == null || odvs.Count == 0)
            {
                return PartialView("_PlannedOdvs", Enumerable.Empty<Odv>());
            }

            return PartialView("_PlannedOdvs", odvs);
        }
        // POST: Odvs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Squadron,CrewChief,Admin")]
        public async Task<IActionResult> Create(OdvCreateVm vm)
        {
            if (vm == null) return BadRequest();

            // Basic server-side validation
            //if (vm.Sorties == null || !vm.Sorties.Any())
            //{
            //    ModelState.AddModelError(string.Empty, "Please add at least one sortie.");
            //}

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;

                var odv = new Odv
                {
                    SquadronId = vm.SquadronId,
                    MissionId = vm.MissionId,
                    OdvDate = vm.OdvDate,
                    Zone = vm.Zone,
                    MissionType = vm.MissionType,
                    Area = vm.Area,
                    Obs = vm.Obs,
                    CreatedAtUtc = now
                };
                SetCreatedAudit(odv);

                _context.Odvs.Add(odv);
                await _context.SaveChangesAsync(); // ensure odv.Id is available for FK

                // Map sorties and nested crew
                if (vm.Sorties != null)
                {
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
                            CreatedAtUtc = now,
                            CreatedBy = GetCurrentUserName(),
                            Status = SortieStatus.Planned
                        };

                        _context.Sorties.Add(sortie);
                        await _context.SaveChangesAsync(); // Get sortie.Id if needed for crew FK

                        if (sVm.Crew != null)
                        {
                            foreach (var cVm in sVm.Crew.Where(c => c != null && c.CrewMemberId > 0))
                            {
                                // Attempt to resolve CrewMember by PersonId or direct Id
                                var cm = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == cVm.CrewMemberId || x.Id == cVm.CrewMemberId);
                                if (cm == null)
                                {
                                    // Option: skip or create a CrewMember entry if your flow supports it.
                                    continue;
                                }

                                var sc = new SortieCrew
                                {
                                    SortieId = sortie.Id,
                                    CrewMemberId = cm.Id,
                                    Role = cVm.Role,
                                    IsPrimary = cVm.IsPrimary
                                };
                                _context.SortieCrews.Add(sc);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to create ODV with sorties");
                ModelState.AddModelError(string.Empty, "Failed to save ODV: " + ex.Message);
                await PopulateSelectListsAsync();
                return View(vm);
            }
        }

        // POST: Odvs/AddCrewToSortie (assign a single crew member to an existing sortie)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CrewChief")]
        public async Task<IActionResult> AddCrewToSortie(int sortieId, int personId, string? role, bool isPrimary = false)
        {
            if (sortieId <= 0 || personId <= 0) return BadRequest();

            var sortie = await _context.Sorties.FirstOrDefaultAsync(s => s.Id == sortieId);
            if (sortie == null) return NotFound();

            // Optional rule: prevent assignment if ODV not preflight approved
            var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == sortie.OdvId);
            if (odv != null && !odv.IsPreflightApproved)
            {
                return StatusCode(409, new { success = false, error = "ODV is not preflight-approved by Squadron. Cannot add crew." });
            }

            // Resolve crew member
            var cm = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == personId || x.Id == personId);
            if (cm == null) return NotFound("Crew member not found");

            // Check for existing assignment
            var exists = await _context.SortieCrews.FirstOrDefaultAsync(sc => sc.SortieId == sortieId && sc.CrewMemberId == cm.Id);
            if (exists != null) return StatusCode(409, new { success = false, error = "Crew member already assigned to this sortie." });

            var scNew = new SortieCrew
            {
                SortieId = sortieId,
                CrewMemberId = cm.Id,
                Role = role,
                IsPrimary = isPrimary
            };
            _context.SortieCrews.Add(scNew);
            await _context.SaveChangesAsync();

            return Json(new { success = true, sortieId = sortieId, crewId = scNew.Id });
        }

        // GET: Odvs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var odv = await _context.Odvs
                .Include(o => o.Sorties)
                    .ThenInclude(s => s.SortieCrews)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null) return NotFound();

            var vm = MapOdvToVm(odv);

            await PopulateSelectListsAsync();

            if (IsAjaxRequest() || QueryModalFlag()) return PartialView("_CreateEditModal", vm);
            return View(vm);
        }

        // POST: Odvs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OdvCreateVm vm)
        {
            if (id <= 0) return BadRequest();

            vm.Sorties ??= new List<SortieVm>();

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                if (IsAjaxRequest()) return PartialView("_CreateEditModal", vm);
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

                odv.SquadronId = vm.SquadronId;
                odv.MissionId = vm.MissionId;
                odv.OdvDate = vm.OdvDate;
                odv.Zone = vm.Zone;
                odv.MissionType = vm.MissionType;
                odv.Area = vm.Area ?? string.Empty;
                odv.OdvStatus = vm.OdvStatus;
                odv.TOFF = vm.TOFF;
                odv.AcMainGroupId = vm.AcMainGroupId;
                odv.CallSignId = vm.CallSignId;
                odv.Obs = vm.Obs;
                odv.UpdatedAtUtc = DateTime.UtcNow;

                SetUpdatedAudit(odv);

                var existingSorties = odv.Sorties?.ToList() ?? new List<Sortie>();
                if (existingSorties.Any())
                {
                    var sortieIds = existingSorties.Select(s => s.Id).ToList();
                    var assigns = await _context.SortieCrews.Where(sc => sortieIds.Contains(sc.SortieId)).ToListAsync();
                    _context.SortieCrews.RemoveRange(assigns);
                    _context.Sorties.RemoveRange(existingSorties);
                    await _context.SaveChangesAsync();
                }

                var now = DateTime.UtcNow;
                if (vm.Sorties != null)
                {
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
                            CreatedAtUtc = now,
                            CreatedBy = GetCurrentUserName(),
                            IsCompleted = sVm.IsCompleted
                        };

                        _context.Sorties.Add(sortie);

                        if (sVm.Crew != null)
                        {
                            foreach (var c in sVm.Crew)
                            {
                                if (c.CrewMemberId == 0) continue;

                                int? crewMemberId = null;
                                var cm = await _context.CrewMembers.FindAsync(c.CrewMemberId);
                                if (cm != null) crewMemberId = cm.Id;
                                else
                                {
                                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == c.CrewMemberId);
                                    if (cmByPerson != null) crewMemberId = cmByPerson.Id;
                                }

                                if (!crewMemberId.HasValue) continue;

                                var sc = new SortieCrew
                                {
                                    Sortie = sortie,
                                    CrewMemberId = crewMemberId.Value,
                                    Role = c.Role,
                                    IsPrimary = c.IsPrimary,
                                    Remarks = (c as dynamic).Remarks
                                };

                                _context.SortieCrews.Add(sc);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _context.Odvs.Update(odv);

                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                if (IsAjaxRequest()) return Json(new { success = true, id = odv.Id });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit ODV {Id}", id);
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to save ODV: " + ex.Message);
                await PopulateSelectListsAsync();
                if (IsAjaxRequest()) return PartialView("_CreateEditModal", vm);
                return View(vm);
            }
        }

        // GET: Odvs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var odv = await _context.Odvs
                .Include(o => o.Squadron)
                .Include(o => o.Mission)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odv == null) return NotFound();

            if (IsAjaxRequest() || QueryModalFlag()) return PartialView("_DeleteModal", odv);
            return View(odv);
        }

        // POST: Odvs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var odv = await _context.Odvs
                    .Include(o => o.Sorties)
                        .ThenInclude(s => s.SortieCrews)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (odv == null) return NotFound();

                var sortieIds = odv.Sorties?.Select(s => s.Id).ToList() ?? new List<int>();
                if (sortieIds.Any())
                {
                    var assigns = await _context.SortieCrews.Where(sc => sortieIds.Contains(sc.SortieId)).ToListAsync();
                    _context.SortieCrews.RemoveRange(assigns);
                    _context.Sorties.RemoveRange(odv.Sorties);
                }

                _context.Odvs.Remove(odv);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                if (IsAjaxRequest()) return Json(new { success = true });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete ODV {Id}", id);
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to delete ODV: " + ex.Message);
                if (IsAjaxRequest()) return StatusCode(500, new { error = ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }

        //
        // --- Inline Sortie API: Add / Update / Delete with crew mapping and warnings (non-blocking) ---
        //

  

        // POST: Odvs/UpdateSortie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSortie(int sortieId, SortieVm vm)
        {
            if (sortieId <= 0) return BadRequest("sortieId required");
            if (vm == null) return BadRequest("Sortie data is required.");

            var sortie = await _context.Sorties
                .Include(s => s.SortieCrews)
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null) return NotFound($"Sortie {sortieId} not found.");

            // enforce that certain updates are allowed only in appropriate statuses (example)
            // This action remains "full edit" for authorized users; additional staged endpoints exist below.

            sortie.AircraftId = vm.AircraftId;
            sortie.Configuration = vm.Configuration;
            sortie.FuelQuantity = vm.FuelQuantity;
            sortie.StartTime = vm.StartTime;
            sortie.LandingTime = vm.LandingTime;
            sortie.TOFF = vm.TOFF;
            sortie.Notes = vm.Notes ?? string.Empty;
            sortie.IsCompleted = vm.IsCompleted;
            sortie.UpdatedAtUtc = DateTime.UtcNow;
            SetUpdatedAudit(sortie);

            var now = DateTime.UtcNow;
            var warnings = new List<string>();

            var desired = vm.Crew?.Where(c => c != null && c.CrewMemberId != 0).ToList() ?? new List<SortieCrewVm>();
            var existing = sortie.SortieCrews?.ToList() ?? new List<SortieCrew>();

            var desiredResolved = new List<(int crewMemberId, SortieCrewVm vm)>();
            foreach (var d in desired)
            {
                int? crewMemberId = null;
                var cm = await _context.CrewMembers.FindAsync(d.CrewMemberId);
                if (cm != null) crewMemberId = cm.Id;
                else
                {
                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == d.CrewMemberId);
                    if (cmByPerson != null) crewMemberId = cmByPerson.Id;
                }

                if (crewMemberId.HasValue)
                {
                    desiredResolved.Add((crewMemberId.Value, d));
                }
                else
                {
                    warnings.Add($"Crew person {d.CrewMemberId} not found as CrewMember.");
                }
            }

            var desiredIds = desiredResolved.Select(x => x.crewMemberId).ToHashSet();
            var toRemove = existing.Where(e => !desiredIds.Contains(e.CrewMemberId)).ToList();
            if (toRemove.Any())
            {
                _context.SortieCrews.RemoveRange(toRemove);
            }

            foreach (var item in desiredResolved)
            {
                var exist = existing.FirstOrDefault(e => e.CrewMemberId == item.crewMemberId);
                var memberWarnings = new List<string>();

                var cm = await _context.CrewMembers
                    .Include(x => x.Person)
                    .FirstOrDefaultAsync(x => x.Id == item.crewMemberId);

                if (cm == null)
                {
                    warnings.Add($"CrewMember id {item.crewMemberId} not found.");
                    continue;
                }

                try
                {
                    var odv = await _context.Odvs.FirstOrDefaultAsync(o => o.Id == sortie.OdvId);
                    if (odv != null && odv.SquadronId != 0 && cm.SquadronId != 0 && cm.SquadronId != odv.SquadronId)
                    {
                        memberWarnings.Add($"Not in ODV squadron (member sq {cm.SquadronId}).");
                    }
                }
                catch { }

                DateTime? medicalExpiry = GetDateProperty(cm, new[] { "MedicalExpiry", "MedicalExpiryDate", "MedicalCertificateExpiry", "MedicalCertificateExpiryDate" });
                DateTime? licenseExpiry = GetDateProperty(cm, new[] { "LicenseExpiry", "LicenseExpiryDate", "LicensingExpiry", "LicenseValidUntil" });

                if (medicalExpiry.HasValue)
                {
                    if (medicalExpiry.Value.Date < now.Date) memberWarnings.Add($"Medical expired {medicalExpiry.Value:yyyy-MM-dd}");
                    else if (medicalExpiry.Value.Date <= now.Date.AddDays(30)) memberWarnings.Add($"Medical due {medicalExpiry.Value:yyyy-MM-dd}");
                }
                if (licenseExpiry.HasValue)
                {
                    if (licenseExpiry.Value.Date < now.Date) memberWarnings.Add($"License expired {licenseExpiry.Value:yyyy-MM-dd}");
                    else if (licenseExpiry.Value.Date <= now.Date.AddDays(30)) memberWarnings.Add($"License due {licenseExpiry.Value:yyyy-MM-dd}");
                }

                if (exist != null)
                {
                    exist.Role = item.vm.Role;
                    exist.IsPrimary = item.vm.IsPrimary;
                    exist.Remarks = memberWarnings.Any() ? string.Join("; ", memberWarnings) : null;
                    _context.SortieCrews.Update(exist);
                    if (memberWarnings.Any()) warnings.Add($"Crew {GetCrewDisplay(cm)}: {string.Join("; ", memberWarnings)}");
                }
                else
                {
                    var sc = new SortieCrew
                    {
                        SortieId = sortie.Id,
                        CrewMemberId = item.crewMemberId,
                        Role = item.vm.Role,
                        IsPrimary = item.vm.IsPrimary,
                        Remarks = memberWarnings.Any() ? string.Join("; ", memberWarnings) : null
                    };
                    _context.SortieCrews.Add(sc);
                    if (memberWarnings.Any()) warnings.Add($"Crew {GetCrewDisplay(cm)}: {string.Join("; ", memberWarnings)}");
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                var crewSummary = await _context.SortieCrews
                    .Where(sc => sc.SortieId == sortie.Id)
                    .Include(sc => sc.CrewMember)
                        .ThenInclude(cm => cm.Person)
                    .Select(sc => new
                    {
                        sc.Id,
                        sc.CrewMemberId,
                        Display = string.IsNullOrEmpty(sc.CrewMember.NickName) ? (sc.CrewMember.Person != null ? sc.CrewMember.Person.FirstName + " " + sc.CrewMember.Person.LastName : ("CM#" + sc.CrewMemberId)) : sc.CrewMember.NickName,
                        sc.Role,
                        sc.IsPrimary,
                        sc.Remarks
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    sortieId = sortie.Id,
                    odvId = sortie.OdvId,
                    updated = new
                    {
                        StartTime = sortie.StartTime?.ToString(),
                        LandingTime = sortie.LandingTime?.ToString(),
                        Notes = sortie.Notes,
                        IsCompleted = sortie.IsCompleted
                    },
                    crew = crewSummary,
                    warnings = warnings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sortie {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // POST: Odvs/DeleteSortie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSortie(int sortieId)
        {
            if (sortieId <= 0) return BadRequest("sortieId required");

            var sortie = await _context.Sorties
                .Include(s => s.SortieCrews)
                .FirstOrDefaultAsync(s => s.Id == sortieId);

            if (sortie == null) return NotFound();

            try
            {
                if (sortie.SortieCrews?.Any() ?? false)
                {
                    _context.SortieCrews.RemoveRange(sortie.SortieCrews);
                }

                _context.Sorties.Remove(sortie);
                await _context.SaveChangesAsync();

                return Json(new { success = true, sortieId = sortieId, odvId = sortie.OdvId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete sortie {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        //
        // --- Aircraft selector modal & assign (CrewChief). UI opens /Odvs/SelectAircraft?sortieId=... (GET)
        //

        // GET: Odvs/SelectAircraft?sortieId=123
        [HttpGet]
        public async Task<IActionResult> SelectAircraft(int sortieId)
        {
            // Eager-load AcType so AcType.Name is available without lazy loading
            var aircrafts = await _context.Aircrafts
                .Include(a => a.AcType)
                .OrderBy(a => a.AircraftVersion)
                .ToListAsync();

            var vm = new AircraftSelectVm
            {
                SortieId = sortieId,
                Aircrafts = aircrafts.Select(a => new AircraftItemVm
                {
                    Id = a.Id,
                    Registration = a.Registration,
                    AcType = a.AcType.Name,
                    Status = (FRAProject.ViewModels.AircraftStatus)a.Status // Explicit cast to fix CS0266
                }).ToList()
            };

            return PartialView("_AircraftSelectModal", vm);
        }

        // POST: Odvs/AssignAircraft
        [HttpPost]
        [Authorize(Roles = "CrewChief")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAircraft(int sortieId, int aircraftId)
        {
            if (sortieId <= 0) return BadRequest("sortieId required");
            if (aircraftId <= 0) return BadRequest("aircraftId required");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var sortie = await _context.Sorties
                    .Include(s => s.Odv)
                    .FirstOrDefaultAsync(s => s.Id == sortieId);

                if (sortie == null) return NotFound($"Sortie {sortieId} not found.");

                if (!sortie.Odv.IsPreflightApproved)
                {
                    return StatusCode(409, new { success = false, error = "ODV is not preflight-approved by Squadron. Cannot assign aircraft." });
                }

                var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == aircraftId);
                if (aircraft == null) return NotFound($"Aircraft {aircraftId} not found.");

                if (aircraft.Status == Areas.Settings.Models.AircraftStatus.Unserviceable)
                {
                    return StatusCode(409, new { success = false, error = $"Aircraft {aircraft.Registration ?? aircraft.Id.ToString()} is unserviceable and cannot be assigned." });
                }

                if (aircraft.Status == Areas.Settings.Models.AircraftStatus.Airborne)
                {
                    return StatusCode(409, new { success = false, error = $"Aircraft {aircraft.Registration ?? aircraft.Id.ToString()} is airborne and cannot be assigned." });
                }

                if (aircraft.Status == Areas.Settings.Models.AircraftStatus.Assigned)
                {
                    return StatusCode(409, new { success = false, error = $"Aircraft {aircraft.Registration ?? aircraft.Id.ToString()} is already assigned to another sortie." });
                }

                sortie.AircraftId = aircraftId;
                sortie.Status = SortieStatus.AircraftAssigned;
                sortie.UpdatedAtUtc = DateTime.UtcNow;
                SetUpdatedAudit(sortie);

                aircraft.Status = Areas.Settings.Models.AircraftStatus.Assigned;

                _context.Sorties.Update(sortie);
                _context.Aircrafts.Update(aircraft);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true, sortieId = sortie.Id, aircraftId = aircraft.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { success = false, error = "Concurrency conflict - the aircraft or sortie was changed by another user. Please reload and try again." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "AssignAircraft failed for sortie {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        //
        // --- TWR endpoints (SetRealToff, SetRealLanding) ---
        //

        [HttpPost]
        [Authorize(Roles = "Tower")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRealToff(int sortieId, DateTime realToffUtc)
        {
            if (sortieId <= 0) return BadRequest();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var sortie = await _context.Sorties
                    .Include(s => s.Odv)
                    .FirstOrDefaultAsync(s => s.Id == sortieId);

                if (sortie == null) return NotFound();

                if (!sortie.Odv.IsPreflightApproved)
                {
                    return StatusCode(409, new { success = false, error = "ODV not preflight-approved." });
                }

                if (!sortie.AircraftId.HasValue)
                {
                    return StatusCode(409, new { success = false, error = "Aircraft not assigned for this sortie." });
                }

                var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == sortie.AircraftId);
                if (aircraft == null) return NotFound("Aircraft not found");

                if (aircraft.Status == Areas.Settings.Models.AircraftStatus.Airborne)
                {
                    return StatusCode(409, new { success = false, error = "Aircraft already airborne (not released). TWR must release before proceeding." });
                }

                sortie.RealTOFF = realToffUtc;
                sortie.Status = SortieStatus.Airborne;
                sortie.UpdatedAtUtc = DateTime.UtcNow;
                SetUpdatedAudit(sortie);

                aircraft.Status = Areas.Settings.Models.AircraftStatus.Airborne;
                _context.Sorties.Update(sortie);
                _context.Aircrafts.Update(aircraft);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true, sortieId = sortie.Id, realTOFF = sortie.RealTOFF });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { success = false, error = "Concurrency conflict - reload and retry." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "SetRealToff failed for {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Tower")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRealLanding(int sortieId, DateTime realLandingUtc, int? landings, bool? brakeChuteUsed)
        {
            if (sortieId <= 0) return BadRequest();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var sortie = await _context.Sorties
                    .Include(s => s.Odv)
                    .FirstOrDefaultAsync(s => s.Id == sortieId);

                if (sortie == null) return NotFound();

                if (!sortie.AircraftId.HasValue) return StatusCode(409, new { success = false, error = "No aircraft assigned" });

                var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == sortie.AircraftId);
                if (aircraft == null) return NotFound("Aircraft not found");

                sortie.RealLandingTime = realLandingUtc;
                sortie.Landings = landings;
                sortie.BrakeChuteUsed = brakeChuteUsed;
                sortie.Status = SortieStatus.Landed;
                sortie.UpdatedAtUtc = DateTime.UtcNow;
                SetUpdatedAudit(sortie);

                aircraft.Status = Areas.Settings.Models.AircraftStatus.Available;
                _context.Sorties.Update(sortie);
                _context.Aircrafts.Update(aircraft);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true, sortieId = sortie.Id, realLanding = sortie.RealLandingTime });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { success = false, error = "Concurrency conflict - reload and retry." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "SetRealLanding failed for {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        //
        // CrewChief post-flight report
        //
        [HttpPost]
        [Authorize(Roles = "CrewChief")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrewChiefReport(int sortieId, decimal? fuelUsedLiters, string? malfunctions)
        {
            if (sortieId <= 0) return BadRequest("sortieId required");

            try
            {
                var sortie = await _context.Sorties.FirstOrDefaultAsync(s => s.Id == sortieId);
                if (sortie == null) return NotFound();

                if (sortie.Status != SortieStatus.Landed)
                {
                    return StatusCode(409, new { success = false, error = "Sortie is not landed yet. CrewChief may report only after landing." });
                }

                sortie.FuelUsedLiters = fuelUsedLiters;
                sortie.Malfunctions = malfunctions ?? string.Empty;
                sortie.UpdatedAtUtc = DateTime.UtcNow;
                SetUpdatedAudit(sortie);

                _context.Sorties.Update(sortie);
                await _context.SaveChangesAsync();

                return Json(new { success = true, sortieId = sortie.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, new { success = false, error = "Concurrency conflict - reload and retry." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrewChiefReport failed for {SortieId}", sortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        //
        // Squadron finalization
        //
        [HttpPost]
        [Authorize(Roles = "Squadron")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeSortieData(SortieFinalizeVm vm)
        {
            if (vm == null || vm.SortieId <= 0) return BadRequest("sortieId required");

            try
            {
                var sortie = await _context.Sorties.FirstOrDefaultAsync(s => s.Id == vm.SortieId);
                if (sortie == null) return NotFound();

                if (sortie.Status != SortieStatus.Landed)
                {
                    return StatusCode(409, new { success = false, error = "Cannot finalize sortie until it is landed." });
                }

                sortie.DurationMinutes = vm.DurationMinutes;
                sortie.Interceptions = vm.Interceptions;
                sortie.RadarContacts = vm.RadarContacts;
                sortie.Approachs = vm.AppContacts; // optional: map AppContacts -> Approachs or keep separate
                sortie.SquadronReportNotes = vm.SquadronReportNotes;
                sortie.IsFinalized = true;
                sortie.FinalizedAtUtc = DateTime.UtcNow;
                sortie.FinalizedBy = GetCurrentUserName();
                sortie.Status = SortieStatus.Finalized;
                sortie.UpdatedAtUtc = DateTime.UtcNow;
                SetUpdatedAudit(sortie);

                _context.Sorties.Update(sortie);
                await _context.SaveChangesAsync();

                // Optionally mark ODV complete when all sorties finalized - omitted here (can be added)
                return Json(new { success = true, sortieId = sortie.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, new { success = false, error = "Concurrency conflict - reload and retry." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinalizeSortieData failed for {SortieId}", vm.SortieId);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        //
        // --- Helpers (Select lists, mapping, audit, small utilities) ---
        //

        private async Task PopulateSelectListsAsync()
        {
            var squadrons = await _context.Squadrons.OrderBy(s => s.Name).Select(s => new { s.Id, s.Name }).ToListAsync();
            var missions = await _context.Missions.OrderBy(m => m.Name).Select(m => new { m.Id, m.Name }).ToListAsync();
            var acs = await _context.Aircrafts.OrderBy(a => a.AcType).Select(a => new { a.Id, Display = a.AcType + " / " + a.Registration }).ToListAsync();
            var acMaingrps = await _context.AcMainGroups.OrderBy(amg => amg.Name).Select(amg => new { amg.Id, amg.Name }).ToListAsync();
            var cs = await _context.CallSigns.OrderBy(cs => cs.Code).Select(cs => new { cs.Id, cs.Code }).ToListAsync();
            
            var crew = await _context.CrewMembers
                .Include(cm => cm.Person)
                .OrderBy(cm => cm.NickName)
                .Select(cm => new { cm.Id, Display = (cm.NickName ?? "") + (cm.Person != null ? " (" + cm.Person.FirstName + " " + cm.Person.LastName + ")" : "") })
                .ToListAsync();

            ViewData["Squadrons"] = new SelectList(squadrons, "Id", "Name");
            ViewData["Missions"] = new SelectList(missions, "Id", "Name");
            ViewData["Aircrafts"] = new SelectList(acs, "Id", "Display");
            ViewData["CrewMembers"] = new SelectList(crew, "Id", "Display");
            ViewData["AcMainGroups"] = new SelectList(acMaingrps, "Id", "Name");
            ViewData["CallSigns"] = new SelectList(cs, "Id", "Code");
        }

        private OdvCreateVm MapOdvToVm(Odv odv)
        {
            return new OdvCreateVm
            {
                SquadronId = odv.SquadronId,
                MissionId = odv.MissionId,
                OdvDate = odv.OdvDate,
                Zone = odv.Zone,
                MissionType = odv.MissionType,
                Area = odv.Area,
                OdvStatus = odv.OdvStatus,
                TOFF = odv.TOFF,
                AcMainGroupId = odv.AcMainGroupId,
                CallSignId = odv.CallSignId,
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
                    IsCompleted = s.IsCompleted,
                    Crew = s.SortieCrews?.Select(sc => new SortieCrewVm
                    {
                        CrewMemberId = sc.CrewMemberId,
                        Role = sc.Role,
                        IsPrimary = sc.IsPrimary
                    }).ToList() ?? new List<SortieCrewVm>()
                }).ToList() ?? new List<SortieVm>()
            };
        }

        private bool IsAjaxRequest()
        {
            if (Request == null || Request.Headers == null) return false;
            if (Request.Headers.TryGetValue("X-Requested-With", out var headerValue))
            {
                return string.Equals(headerValue.ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool QueryModalFlag()
        {
            return (Request?.Query["modal"].ToString() ?? "").ToLowerInvariant() == "true";
        }

        private string? GetCurrentUserId()
        {
            if (User == null) return null;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id)) return id;
            return User.Identity?.Name;
        }

        private string? GetCurrentUserName()
        {
            if (User == null) return null;

            var name = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email)) return email;

            var given = User.FindFirstValue(ClaimTypes.GivenName);
            var family = User.FindFirstValue(ClaimTypes.Surname);
            if (!string.IsNullOrWhiteSpace(given) || !string.IsNullOrWhiteSpace(family))
            {
                return $"{given} {family}".Trim();
            }

            return GetCurrentUserId();
        }

        private void SetCreatedAudit(object entity, DateTime? createdAt = null)
        {
            if (entity == null) return;
            var now = createdAt ?? DateTime.UtcNow;
            var t = entity.GetType();

            var propCreatedAt = t.GetProperty("CreatedAtUtc", BindingFlags.Public | BindingFlags.Instance);
            if (propCreatedAt != null && propCreatedAt.CanWrite)
            {
                var propType = propCreatedAt.PropertyType;
                if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                {
                    propCreatedAt.SetValue(entity, now);
                }
            }

            var propCreatedBy = t.GetProperty("CreatedBy", BindingFlags.Public | BindingFlags.Instance);
            if (propCreatedBy != null && propCreatedBy.CanWrite && propCreatedBy.PropertyType.IsAssignableFrom(typeof(string)))
            {
                var name = GetCurrentUserName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    propCreatedBy.SetValue(entity, name);
                }
            }
        }

        private void SetUpdatedAudit(object entity, DateTime? updatedAt = null)
        {
            if (entity == null) return;
            var now = updatedAt ?? DateTime.UtcNow;
            var t = entity.GetType();

            var propUpdatedAt = t.GetProperty("UpdatedAtUtc", BindingFlags.Public | BindingFlags.Instance);
            if (propUpdatedAt != null && propUpdatedAt.CanWrite)
            {
                var propType = propUpdatedAt.PropertyType;
                if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                {
                    propUpdatedAt.SetValue(entity, now);
                }
            }

            var propUpdatedBy = t.GetProperty("UpdatedBy", BindingFlags.Public | BindingFlags.Instance);
            if (propUpdatedBy != null && propUpdatedBy.CanWrite && propUpdatedBy.PropertyType.IsAssignableFrom(typeof(string)))
            {
                var name = GetCurrentUserName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    propUpdatedBy.SetValue(entity, name);
                }
            }
        }

        private DateTime? GetDateProperty(object obj, string[] candidateNames)
        {
            if (obj == null) return null;
            var t = obj.GetType();

            foreach (var name in candidateNames)
            {
                var prop = t.GetProperty(name);
                if (prop == null) continue;

                var val = prop.GetValue(obj);
                if (val == null) continue;

                switch (val)
                {
                    case DateTime dt:
                        // boxed DateTime (covers DateTime and non-nullable DateTime? with value)
                        return dt;
                    case DateTimeOffset dto:
                        // convert to UTC DateTime
                        return dto.UtcDateTime;
                    case string s:
                        // try parse using invariant culture first
                        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
                        {
                            return parsed;
                        }
                        // fallback to current culture
                        if (DateTime.TryParse(s, out parsed))
                        {
                            return parsed;
                        }
                        break;
                    case long ticks:
                        // optional: if some models store ticks as long
                        try { return new DateTime(ticks, DateTimeKind.Utc); } catch { /* ignore */ }
                        break;
                    case int unixSec:
                        // optional: unix seconds -> DateTime
                        try { return DateTimeOffset.FromUnixTimeSeconds(unixSec).UtcDateTime; } catch { /* ignore */ }
                        break;
                }
            }

            return null;
        }

        private string GetCrewDisplay(CrewMember cm)
        {
            try
            {
                var nick = cm.NickName;
                if (!string.IsNullOrWhiteSpace(nick)) return nick;
                if (cm.Person != null)
                {
                    return $"{cm.Person.FirstName} {cm.Person.LastName}".Trim();
                }
            }
            catch { }
            return $"CrewMember#{cm.Id}";
        }
    }
}