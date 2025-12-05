using System.Linq;
using System.Threading.Tasks;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    public class OdvsController : Controller
    {
        private readonly FRAContext _context;

        public OdvsController(FRAContext context)
        {
            _context = context;
        }

        // GET: Odvs/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var vm = new OdvCreateVm
            {
                OdvDate = System.DateTime.UtcNow.Date,
                Sorties = new List<SortieVm> { new SortieVm() } // one empty sortie by default
            };
            return View(vm);
        }

        // POST: Odvs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OdvCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var odv = new Odv
                {
                    SquadronID = vm.SquadronID,
                    MissionId = vm.MissionId,
                    OdvDate = vm.OdvDate,
                    ZoneID = vm.ZoneID,
                    MissionTypeID = vm.MissionTypeID,
                    Area = vm.Area,
                    OdvStatus = vm.OdvStatus,
                    TOFF = vm.TOFF,
                    AcMainGroupID = vm.AcMainGroupID,
                    CallSignId = vm.CallSignId,
                    Obs = vm.Obs
                };

                _context.Odvs.Add(odv);
                await _context.SaveChangesAsync(); // get odv.OdvID

                // Add sorties
                if (vm.Sorties != null)
                {
                    foreach (var sVm in vm.Sorties)
                    {
                        var sortie = new Sortie
                        {
                            OdvID = odv.OdvID,
                            AircraftId = sVm.AircraftId,
                            Configuration = sVm.Configuration,
                            FuelQuantity = sVm.FuelQuantity,
                            StartTime = sVm.StartTime,
                            LandingTime = sVm.LandingTime,
                            TOFF = sVm.TOFF,
                            Notes = sVm.Notes
                        };

                        _context.Sorties.Add(sortie);
                        await _context.SaveChangesAsync(); // obtain SortieId for crew

                        // Add crew assignments
                        if (sVm.Crew != null)
                        {
                            foreach (var cVm in sVm.Crew)
                            {
                                // optional validation: ensure Person exists
                                var sc = new SortieCrew
                                {
                                    SortieId = sortie.SortieId,
                                    PersonId = cVm.PersonId,
                                    Role = cVm.Role,
                                    IsPrimary = cVm.IsPrimary
                                };
                                _context.SortieCrews.Add(sc);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await tx.CommitAsync();
                return RedirectToAction("Index", "Odvs");
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Error saving ODV. Please try again.");
                await PopulateDropdowns();
                return View(vm);
            }
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Squadrons = new SelectList(await _context.Squadrons.AsNoTracking().OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
            ViewBag.Missions = new SelectList(await _context.Missions.AsNoTracking().OrderBy(m => m.Name).ToListAsync(), "Id", "Name");
            ViewBag.Aircrafts = new SelectList(await _context.Aircrafts.AsNoTracking().OrderBy(a => a.TailNo).ToListAsync(), "Id", "TailNumber");
            ViewBag.People = new SelectList(await _context.Persons.AsNoTracking().OrderBy(p => p.LastName).ToListAsync(), "Id", "FullName"); // adjust FullName
            ViewBag.AcMainGroups = new SelectList(await _context.AcMainGroups.AsNoTracking().OrderBy(g => g.Name).ToListAsync(), "Id", "Name");

            // For enum selects you can create select-lists in view from enums directly.
        }
    }
}