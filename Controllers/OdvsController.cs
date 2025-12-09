using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
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
    // Controller name pluralized as requested. Routes will be /Odvs/{action}
    public class OdvsController : Controller
    {
        private readonly FRAContext _context;
        private readonly ILogger<OdvsController> _logger;

        public OdvsController(FRAContext context, ILogger<OdvsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Odvs
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

        // GET: Odvs/Details/5
        // Returns full view or partial modal (when AJAX or ?modal=true)
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

            var isAjax = IsAjaxRequest();
            if (isAjax || QueryModalFlag()) return PartialView("_DetailsModal", odv);

            return View(odv);
        }

        // GET: Odvs/Create
        // Returns full create view or partial modal
        public async Task<IActionResult> Create()
        {
            var vm = new OdvCreateVm
            {
                OdvDate = DateTime.UtcNow.Date
            };

            await PopulateSelectListsAsync();
            if (IsAjaxRequest() || QueryModalFlag()) return PartialView("_CreateEditModal", vm);
            return View(vm);
        }

        // POST: Odvs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OdvCreateVm vm)
        {
            vm.Sorties ??= new List<SortieVm>();

            // Example validation: require at least one sortie
            if (!vm.Sorties.Any()) ModelState.AddModelError(string.Empty, "Please add at least one sortie.");

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                if (IsAjaxRequest()) return PartialView("_CreateEditModal", vm);
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Map VM -> entity
                var now = DateTime.UtcNow;
                var odv = new Odv
                {
                    SquadronId = vm.SquadronID,
                    MissionId = vm.MissionId,
                    OdvDate = vm.OdvDate,
                    Zone = vm.ZoneID,
                    MissionType = vm.MissionTypeID,
                    Area = vm.Area ?? string.Empty,
                    OdvStatus = vm.OdvStatus,
                    TOFF = vm.TOFF,
                    AcMainGroupId = vm.AcMainGroupID,
                    CallSign = vm.CallSignId,
                    Obs = vm.Obs,
                    CreatedAtUtc = now
                };
                SetCreatedAudit(odv);
                _context.Odvs.Add(odv);
                
                // Add sorties and crew. Set sortie.CreatedAtUtc = odv.CreatedAtUtc so created-time matches for those added together.
                if (vm.Sorties != null)
                {
                    foreach (var sVm in vm.Sorties)
                    {
                        var sortie = new Sortie
                        {
                            Odv = odv,
                            AircraftId = sVm.AircraftId,
                            Configuration = sVm.Configuration,
                            FuelQuantity = sVm.FuelQuantity,
                            StartTime = sVm.StartTime,
                            LandingTime = sVm.LandingTime,
                            TOFF = sVm.TOFF,
                            Notes = sVm.Notes,
                            CreatedAtUtc = now,
                            CreatedBy = User?.Identity?.Name
                        };

                        
                        SetCreatedAudit(sortie, odv.CreatedAtUtc);
                        _context.Sorties.Add(sortie);

                        if (sVm.Crew != null)
                        {
                            foreach (var c in sVm.Crew)
                            {
                                if (c.PersonId == 0) continue;

                                // Resolve crewMemberId: assume dropdown uses CrewMember.Id; if using Person.Id fallback to PersonId -> CrewMember lookup
                                int? crewMemberId = null;
                                var cm = await _context.CrewMembers.FindAsync(c.PersonId);
                                if (cm != null) crewMemberId = cm.Id;
                                else
                                {
                                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == c.PersonId);
                                    if (cmByPerson != null) crewMemberId = cmByPerson.Id;
                                }

                                if (!crewMemberId.HasValue) continue;

                                var sc = new SortieCrew
                                {
                                    Sortie = sortie,
                                    CrewMemberId = crewMemberId.Value,
                                    Role = c.Role,
                                    IsPrimary = c.IsPrimary,
                                    Remarks = (c as dynamic).Remarks // preserve Remarks if present on VM (weakly typed)
                                };

                                _context.SortieCrews.Add(sc);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                if (IsAjaxRequest()) return Json(new { success = true, id = odv.Id });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ODV");
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to create ODV: " + ex.Message);
                await PopulateSelectListsAsync();
                if (IsAjaxRequest()) return PartialView("_CreateEditModal", vm);
                return View(vm);
            }
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

                // Update header
                odv.SquadronId = vm.SquadronID;
                odv.MissionId = vm.MissionId;
                odv.OdvDate = vm.OdvDate;
                odv.Zone = vm.ZoneID;
                odv.MissionType = vm.MissionTypeID;
                odv.Area = vm.Area ?? string.Empty;
                odv.OdvStatus = vm.OdvStatus;
                odv.TOFF = vm.TOFF;
                odv.AcMainGroupId = vm.AcMainGroupID;
                odv.CallSign = vm.CallSignId;
                odv.Obs = vm.Obs;
                odv.UpdatedAtUtc = DateTime.UtcNow; 
                
                SetUpdatedAudit(odv);

                // Remove existing sorties and their crew assignments
                var existingSorties = odv.Sorties?.ToList() ?? new List<Sortie>();
                if (existingSorties.Any())
                {
                    var sortieIds = existingSorties.Select(s => s.Id).ToList();
                    var assigns = await _context.SortieCrews.Where(sc => sortieIds.Contains(sc.SortieId)).ToListAsync();
                    _context.SortieCrews.RemoveRange(assigns);
                    _context.Sorties.RemoveRange(existingSorties);                    
                    await _context.SaveChangesAsync();
                }

                // Add new sorties from VM
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
                            CreatedBy = User?.Identity?.Name,
                            IsCompleted = sVm.IsCompleted
                        };

                        _context.Sorties.Add(sortie);

                        if (sVm.Crew != null)
                        {
                            foreach (var c in sVm.Crew)
                            {
                                if (c.PersonId == 0) continue;

                                int? crewMemberId = null;
                                var cm = await _context.CrewMembers.FindAsync(c.PersonId);
                                if (cm != null) crewMemberId = cm.Id;
                                else
                                {
                                    var cmByPerson = await _context.CrewMembers.FirstOrDefaultAsync(x => x.PersonId == c.PersonId);
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

                // EF cascade will remove Sorties and SortieCrews if configured; otherwise remove manually
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
                // If AJAX return error status
                if (IsAjaxRequest()) return StatusCode(500, new { error = ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper: populate selects used in Create/Edit views
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

            // If you want enum selects (Zone/MissionType/OdvStatus) you can supply them here using an EnumExtensions.ToSelectList<T>()
        }

        // Helper: map Odv entity -> OdvCreateVm for editing
        private OdvCreateVm MapOdvToVm(Odv odv)
        {
            return new OdvCreateVm
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
                    IsCompleted = s.IsCompleted,
                    Crew = s.SortieCrews?.Select(sc => new SortieCrewVm
                    {
                        PersonId = sc.CrewMemberId,
                        Role = sc.Role,
                        IsPrimary = sc.IsPrimary,
                        // If your SortieCrewVm has Remarks property, map it; otherwise ignore
                    }).ToList() ?? new List<SortieCrewVm>()
                }).ToList() ?? new List<SortieVm>()
            };
        }

        private bool IsAjaxRequest()
        {
            // Request is normally non-null in Controller, but guard defensively anyway.
            if (Request == null || Request.Headers == null) return false;

            if (Request.Headers.TryGetValue("X-Requested-With", out var headerValue))
            {
                // StringValues.ToString() returns a comma-separated list if there are multiple values.
                return string.Equals(headerValue.ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool QueryModalFlag()
        {
            return (Request?.Query["modal"].ToString() ?? "").ToLowerInvariant() == "true";
        }

        // Try to get the current user's identifier (NameIdentifier claim) or fallback to Name.
        private string? GetCurrentUserId()
        {
            if (User == null) return null;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id)) return id;
            return User.Identity?.Name;
        }

        // Prefer a friendly name for CreatedBy/UpdatedBy (Name claim or email, fallback to id)
        private string? GetCurrentUserName()
        {
            if (User == null) return null;

            // Try Name
            var name = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;

            // Try common claims
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email)) return email;

            var given = User.FindFirstValue(ClaimTypes.GivenName);
            var family = User.FindFirstValue(ClaimTypes.Surname);
            if (!string.IsNullOrWhiteSpace(given) || !string.IsNullOrWhiteSpace(family))
            {
                return $"{given} {family}".Trim();
            }

            // Fallback to id
            return GetCurrentUserId();
        }

        // Generic reflection-based helper to set CreatedAtUtc and CreatedBy if those properties exist on the entity.
        // This allows reuse for Odv, Sortie, etc., even if some entities don't have CreatedBy property yet.
        private void SetCreatedAudit(object entity, DateTime? createdAt = null)
        {
            if (entity == null) return;
            var now = createdAt ?? DateTime.UtcNow;
            var t = entity.GetType();

            var propCreatedAt = t.GetProperty("CreatedAtUtc", BindingFlags.Public | BindingFlags.Instance);
            if (propCreatedAt != null && propCreatedAt.CanWrite && propCreatedAt.PropertyType.IsAssignableFrom(typeof(DateTime)))
            {
                propCreatedAt.SetValue(entity, now);
            }

            var propCreatedBy = t.GetProperty("CreatedBy", BindingFlags.Public | BindingFlags.Instance);
            if (propCreatedBy != null && propCreatedBy.CanWrite && propCreatedBy.PropertyType.IsAssignableFrom(typeof(string)))
            {
                propCreatedBy.SetValue(entity, GetCurrentUserName());
            }
        }

        // Generic reflection-based helper to set UpdatedAtUtc and UpdatedBy if those properties exist on the entity.
        private void SetUpdatedAudit(object entity, DateTime? updatedAt = null)
        {
            if (entity == null) return;
            var now = updatedAt ?? DateTime.UtcNow;
            var t = entity.GetType();

            var propUpdatedAt = t.GetProperty("UpdatedAtUtc", BindingFlags.Public | BindingFlags.Instance);
            if (propUpdatedAt != null && propUpdatedAt.CanWrite && propUpdatedAt.PropertyType.IsAssignableFrom(typeof(DateTime?)) || (propUpdatedAt != null && propUpdatedAt.CanWrite && propUpdatedAt.PropertyType.IsAssignableFrom(typeof(DateTime))))
            {
                propUpdatedAt.SetValue(entity, now);
            }

            var propUpdatedBy = t.GetProperty("UpdatedBy", BindingFlags.Public | BindingFlags.Instance);
            if (propUpdatedBy != null && propUpdatedBy.CanWrite && propUpdatedBy.PropertyType.IsAssignableFrom(typeof(string)))
            {
                propUpdatedBy.SetValue(entity, GetCurrentUserName());
            }
        }

        // Optional convenience overloads with strong typing (calls the generic reflection helpers)
        private void SetCreatedAuditForOdv(Models.Odv odv, DateTime? createdAt = null) => SetCreatedAudit(odv, createdAt);
        private void SetUpdatedAuditForOdv(Models.Odv odv, DateTime? updatedAt = null) => SetUpdatedAudit(odv, updatedAt);

        private void SetCreatedAuditForSortie(Models.Sortie sortie, DateTime? createdAt = null) => SetCreatedAudit(sortie, createdAt);
        private void SetUpdatedAuditForSortie(Models.Sortie sortie, DateTime? updatedAt = null) => SetUpdatedAudit(sortie, updatedAt);

    }
}