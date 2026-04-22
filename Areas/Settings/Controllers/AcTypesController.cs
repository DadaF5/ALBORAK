using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Administrators")]
    public class AcTypesController : Controller
    {
        private readonly FRAContext _context;

        public AcTypesController(FRAContext context)
        {
            _context = context;
        }

        // Return partial view with list (AJAX)
        public async Task<IActionResult> List()
        {
            var types = await _context.AcTypes
                .Include(t => t.AcMainGroup)
                .Include(t => t.AircraftManufacturer)
                .OrderBy(t => t.AcMainGroup.Name)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .Select(t => new AcTypeDto
                {
                    Id = t.Id,
                    Code = t.Code ?? "",
                    Name = t.Name,
                    Description = t.Description,
                    IsActive = t.IsActive,
                    SortOrder = t.SortOrder,
                    MaxGrossweight = t.MaxGrossweight,
                    MaxPassengers = t.MaxPassengers,
                    SeatCount = t.SeatCount,
                    MaxEngines = t.MaxEngines,
                    AcMainGroupId = t.AcMainGroupId,
                    AcMainGroupName = t.AcMainGroup.Name,
                    AircraftManufacturerId = t.AircraftManufacturerId,
                    ManufacturerName = t.AircraftManufacturer != null ? t.AircraftManufacturer.Name : null
                })
                .ToListAsync();

            return PartialView("_AcTypesList", types);
        }

        // GET: Settings/AcTypes/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var dto = new AcTypeCreateDto { IsActive = true, SortOrder = 99 };
            return View(dto);
        }

        // POST: Settings/AcTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcTypeCreateDto dto)
        {
            // Check for duplicate Code
            if (!string.IsNullOrWhiteSpace(dto.Code) && 
                await _context.AcTypes.AnyAsync(t => t.Code == dto.Code))
            {
                ModelState.AddModelError("Code", $"Le code '{dto.Code}' existe déjà.");
            }

            // Check for duplicate Name within same AcMainGroup
            if (await _context.AcTypes.AnyAsync(t => t.Name == dto.Name && t.AcMainGroupId == dto.AcMainGroupId))
            {
                ModelState.AddModelError("Name", $"Le type '{dto.Name}' existe déjà pour ce groupe principal.");
            }

            if (ModelState.IsValid)
            {
                var acType = new AcType
                {
                    Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpper(),
                    Name = dto.Name.Trim(),
                    Description = dto.Description.Trim(),
                    AcMainGroupId = dto.AcMainGroupId,
                    AircraftManufacturerId = dto.AircraftManufacturerId,
                    MaxGrossweight = dto.MaxGrossweight,
                    MaxPassengers = dto.MaxPassengers,
                    SeatCount = dto.SeatCount,
                    MaxEngines = dto.MaxEngines,
                    IsActive = dto.IsActive,
                    SortOrder = dto.SortOrder
                };

                _context.AcTypes.Add(acType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Type d'aéronef créé avec succès";
                return RedirectToAction("Index", "Home", new { area = "Settings" });
            }

            await PopulateDropdowns(dto.AcMainGroupId, dto.AircraftManufacturerId);
            return View(dto);
        }

        // GET: Settings/AcTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var acType = await _context.AcTypes.FindAsync(id);
            if (acType == null)
            {
                return NotFound();
            }

            var dto = new AcTypeCreateDto
            {
                Id = acType.Id,
                Code = acType.Code ?? "",
                Name = acType.Name,
                Description = acType.Description,
                AcMainGroupId = acType.AcMainGroupId,
                AircraftManufacturerId = acType.AircraftManufacturerId,
                MaxGrossweight = acType.MaxGrossweight,
                MaxPassengers = acType.MaxPassengers,
                SeatCount = acType.SeatCount,
                MaxEngines = acType.MaxEngines,
                IsActive = acType.IsActive,
                SortOrder = acType.SortOrder
            };

            await PopulateDropdowns(dto.AcMainGroupId, dto.AircraftManufacturerId);
            return View(dto);
        }

        // POST: Settings/AcTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcTypeCreateDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            // Check for duplicate Code (excluding current record)
            if (!string.IsNullOrWhiteSpace(dto.Code) &&
                await _context.AcTypes.AnyAsync(t => t.Code == dto.Code && t.Id != id))
            {
                ModelState.AddModelError("Code", $"Le code '{dto.Code}' est déjà utilisé.");
            }

            // Check for duplicate Name within same AcMainGroup (excluding current record)
            if (await _context.AcTypes.AnyAsync(t => t.Name == dto.Name && t.AcMainGroupId == dto.AcMainGroupId && t.Id != id))
            {
                ModelState.AddModelError("Name", $"Le type '{dto.Name}' existe déjà pour ce groupe principal.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var acType = await _context.AcTypes.FindAsync(id);
                    if (acType == null)
                    {
                        return NotFound();
                    }

                    acType.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim().ToUpper();
                    acType.Name = dto.Name.Trim();
                    acType.Description = dto.Description.Trim();
                    acType.AcMainGroupId = dto.AcMainGroupId;
                    acType.AircraftManufacturerId = dto.AircraftManufacturerId;
                    acType.MaxGrossweight = dto.MaxGrossweight;
                    acType.MaxPassengers = dto.MaxPassengers;
                    acType.SeatCount = dto.SeatCount;
                    acType.MaxEngines = dto.MaxEngines;
                    acType.IsActive = dto.IsActive;
                    acType.SortOrder = dto.SortOrder;

                    _context.Update(acType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Type d'aéronef modifié avec succès";
                    return RedirectToAction("Index", "Home", new { area = "Settings" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcTypeExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateDropdowns(dto.AcMainGroupId, dto.AircraftManufacturerId);
            return View(dto);
        }

        // POST: Settings/AcTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var acType = await _context.AcTypes.FindAsync(id);
            if (acType != null)
            {
                _context.AcTypes.Remove(acType);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private bool AcTypeExists(int id)
        {
            return _context.AcTypes.Any(e => e.Id == id);
        }

        // Helper method to populate dropdowns
        private async Task PopulateDropdowns(int? selectedAcMainGroupId = null, int? selectedManufacturerId = null)
        {
            // AcMainGroup dropdown
            ViewBag.AcMainGroups = new SelectList(
                await _context.AcMainGroups
                    .Where(g => g.Active)
                    .OrderBy(g => g.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                selectedAcMainGroupId
            );

            // Manufacturer dropdown (optional)
            var manufacturers = await _context.AircraftManufacturers
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();

            var manufacturerList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Sélectionner un constructeur (optionnel) --" }
            };
            manufacturerList.AddRange(manufacturers.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
                Selected = m.Id == selectedManufacturerId
            }));

            ViewBag.Manufacturers = manufacturerList;
        }
    }
}
