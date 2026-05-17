using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class AircraftController : Controller
    {
        private readonly FRAContext _context;

        public AircraftController(FRAContext context)
        {
            _context = context;
        }

        // ----------------------------
        // INDEX
        // ----------------------------
        public async Task<IActionResult> Index(AircraftIndexViewModel vm)
        {
            var query = _context.Aircrafts
                .Include(a => a.AcType)
                .Include(a => a.AcStatusType)
                .Include(a => a.AcType.AcMainGroup) // for Base filter
                .ThenInclude(mg => mg.Base)
                .AsQueryable();

            // --------------------------
            // Apply Filters
            // --------------------------
            if (vm.FilterBaseId.HasValue)
                query = query.Where(a => a.AcType.AcMainGroup.BaseId == vm.FilterBaseId);

            if (vm.FilterAcTypeId.HasValue)
                query = query.Where(a => a.AcTypeId == vm.FilterAcTypeId);

            if (vm.FilterStatusTypeId.HasValue)
                query = query.Where(a => a.AcStatusTypeId == vm.FilterStatusTypeId);

            vm.Aircrafts = await query
                .OrderBy(a => a.Registration)
                .ToListAsync();

            // --------------------------
            // Populate dropdowns
            // --------------------------
            vm.Bases = await _context.Bases
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                .ToListAsync();

            vm.AcTypes = await _context.AcTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToListAsync();

            vm.StatusTypes = await _context.AcStatusTypes
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            return View(vm);
        }

        // Returns JSON list of AcTypes for a given MainGroup
        public async Task<JsonResult> GetAcTypesByMainGroup(int id)
        {
            var list = await _context.AcTypes
                .Where(t => t.AcMainGroupId == id)
                .OrderBy(t => t.Name)
                .Select(t => new { id = t.Id, name = t.Name })
                .ToListAsync();

            return Json(list);
        }

        // ----------------------------
        // CREATE GET
        // ----------------------------
        public async Task<IActionResult> Create()
        {
            var vm = new AircraftViewModel
            {
                AcMainGroups = await _context.AcMainGroups
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToListAsync(),

                AcStatusTypes = await _context.AcStatusTypes
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync(),

                // Set default Serviceable status
                DefaultServiceableStatusId = await _context.AcStatusTypes
                .Where(s => s.Name == "Serviceable")
                .Select(s => s.Id)
                .FirstOrDefaultAsync()
            };

            return View(vm);
        }

        // ----------------------------
        // CREATE POST
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel vm)
        {
            // ----------------------------
            // Check duplicates within same AcType
            // ----------------------------
            if (await _context.Aircrafts.AnyAsync(a =>
                    a.AcTypeId == vm.AcTypeId &&
                    (a.TailNo == vm.TailNo ||
                     a.Registration == vm.Registration ||
                     (!string.IsNullOrEmpty(vm.IntCode) && a.IntCode == vm.IntCode))
                ))
            {
                ModelState.AddModelError(string.Empty,
                    "An Aircraft with the same Tail Number, Registration, or IntCode already exists for this Type.");
            }

            if (!ModelState.IsValid)
            {
                vm.AcTypes = await GetAcTypesSelectList();
                vm.AcStatusTypes = await GetStatusTypesSelectList();
                return View(vm);
            }

            var aircraft = new Aircraft
            {
                TailNo = vm.TailNo,
                Registration = vm.Registration,
                SerialNumber = vm.SerialNumber,
                Manufacturer = vm.Manufacturer,
                Model = vm.Model,
                ManufactureDate = vm.ManufactureDate,
                IntCode = vm.IntCode,
                Obs = vm.Obs,
                Active = vm.IsActive,
                Serviceable = vm.IsServiceable,
                AcTypeId = vm.AcTypeId,
                AcStatusTypeId = vm.AcStatusTypeId
            };

            _context.Aircrafts.Add(aircraft);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------
        // EDIT GET
        // ----------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aircraft = await _context.Aircrafts
                .Include(a => a.AcType)
                .FirstOrDefaultAsync(a => a.Id == id);


            if (aircraft == null) return NotFound();

            // Determine AcMainGroup for this Aircraft's AcType
            var acMainGroupId = aircraft.AcType.AcMainGroupId;

            // Fill AcTypes dropdown based on AcMainGroup
            var filteredAcTypes = await _context.AcTypes
                .Where(t => t.AcMainGroupId == acMainGroupId)
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();


            var vm = new AircraftViewModel
            {
                Id = aircraft.Id,
                TailNo = aircraft.TailNo,
                Registration = aircraft.Registration,
                SerialNumber = aircraft.SerialNumber,
                Manufacturer = aircraft.Manufacturer,
                Model = aircraft.Model,
                ManufactureDate = aircraft.ManufactureDate,
                IntCode = aircraft.IntCode,
                Obs = aircraft.Obs,
                IsActive = aircraft.Active,
                IsServiceable = aircraft.Serviceable,
                AcTypeId = aircraft.AcTypeId,
                AcStatusTypeId = aircraft.AcStatusTypeId,

                AcTypes = filteredAcTypes,

                AcStatusTypes = await GetStatusTypesSelectList()
            };

            return View(vm);
        }

        // ----------------------------
        // EDIT POST
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            // ----------------------------
            // Check duplicates within same AcType excluding current record
            // ----------------------------
            if (await _context.Aircrafts.AnyAsync(a =>
                    a.Id != vm.Id &&
                    a.AcTypeId == vm.AcTypeId &&
                    (a.TailNo == vm.TailNo ||
                     a.Registration == vm.Registration ||
                     (!string.IsNullOrEmpty(vm.IntCode) && a.IntCode == vm.IntCode))
                ))
            {
                ModelState.AddModelError(string.Empty,
                    "Another Aircraft with the same Tail Number, Registration, or IntCode exists for this Type.");
            }

            if (!ModelState.IsValid)
            {
                vm.AcTypes = await GetAcTypesSelectList();
                vm.AcStatusTypes = await GetStatusTypesSelectList();
                return View(vm);
            }

            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null) return NotFound();

            aircraft.TailNo = vm.TailNo;
            aircraft.Registration = vm.Registration;
            aircraft.SerialNumber = vm.SerialNumber;
            aircraft.Manufacturer = vm.Manufacturer;
            aircraft.Model = vm.Model;
            aircraft.ManufactureDate = vm.ManufactureDate;
            aircraft.IntCode = vm.IntCode;
            aircraft.Obs = vm.Obs;
            aircraft.Active = vm.IsActive;
            aircraft.Serviceable = vm.IsServiceable;
            aircraft.AcTypeId = vm.AcTypeId;
            aircraft.AcStatusTypeId = vm.AcStatusTypeId;

            try
            {
                _context.Update(aircraft);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Aircrafts.Any(e => e.Id == vm.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ----------------------------
        // DELETE GET
        // ----------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var aircraft = await _context.Aircrafts
                .Include(a => a.AcType)
                .Include(a => a.AcStatusType)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aircraft == null) return NotFound();

            return View(aircraft);
        }

        // ----------------------------
        // DELETE POST
        // ----------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft != null)
            {
                _context.Aircrafts.Remove(aircraft);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ----------------------------
        // HELPERS - Dropdowns
        // ----------------------------
        private async Task<IEnumerable<SelectListItem>> GetAcTypesSelectList()
        {
            return await _context.AcTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetStatusTypesSelectList()
        {
            return await _context.AcStatusTypes
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }
    }
}