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
    [Authorize(Roles = "Admin")]
    public class AcTypesController : Controller
    {
        private readonly FRAContext _context;

        public AcTypesController(FRAContext context)
        {
            _context = context;
        }

        // GET: Settings/AcTypes
        public async Task<IActionResult> Index()
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
                    Code = t.Code ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    IsActive = t.IsActive,
                    SortOrder = t.SortOrder,
                    MaxGrossweight = t.MaxGrossweight,
                    MaxPassengers = t.MaxPassengers,
                    SeatCount = t.SeatCount,
                    MaxEngines = t.MaxEngines,
                    AcMainGroupId = t.AcMainGroupId,
                    AcMainGroupName = t.AcMainGroup != null ? t.AcMainGroup.Name : string.Empty,
                    AircraftManufacturerId = t.AircraftManufacturerId,
                    ManufacturerName = t.AircraftManufacturer != null ? t.AircraftManufacturer.Name : null
                })
                .ToListAsync();

            return View(types);
        }

        // GET: Settings/AcTypes/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();

            var dto = new AcTypeCreateDto
            {
                IsActive = true,
                SortOrder = 99
            };

            return View(dto);
        }

        // POST: Settings/AcTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcTypeCreateDto dto)
        {
            var normalizedCode = dto.Code.Trim().ToUpper();

            var normalizedName = dto.Name.Trim() ;
            var normalizedDescription = dto.Description.Trim() ;

            if (await _context.AcTypes.AnyAsync(t => t.Code != null && t.Code.ToUpper() == normalizedCode))
            {
                ModelState.AddModelError("Code", $"Le code '{normalizedCode}' existe déjà.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedName) &&
                await _context.AcTypes.AnyAsync(t => t.Name == normalizedName && t.AcMainGroupId == dto.AcMainGroupId))
            {
                ModelState.AddModelError("Name", $"Le type '{normalizedName}' existe déjà pour ce groupe principal.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto.AcMainGroupId, dto.AircraftManufacturerId);
                return View(dto);
            }

            var acType = new AcType
            {
                Code = normalizedCode,
                Name = normalizedName,
                Description = normalizedDescription,
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
            return RedirectToAction(nameof(Index));
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
                Code = acType.Code ?? string.Empty,
                Name = acType.Name ?? string.Empty,
                Description = acType.Description ?? string.Empty,
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

            var normalizedCode = dto.Code.Trim().ToUpper();

            var normalizedName = dto.Name.Trim();
            var normalizedDescription = dto.Description.Trim() ;

            // Vérification de l'unicité du code (en ignorant l'entité actuelle)
            if (await _context.AcTypes.AnyAsync(t => t.Code != null && t.Code.ToUpper() == normalizedCode))
            {
                ModelState.AddModelError("Code", $"Le code '{normalizedCode}' existe déjà.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedName) &&
                await _context.AcTypes.AnyAsync(t => t.Id != id && t.Name == normalizedName && t.AcMainGroupId == dto.AcMainGroupId))
            {
                ModelState.AddModelError("Name", $"Le type '{normalizedName}' existe déjà pour ce groupe principal.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(dto.AcMainGroupId, dto.AircraftManufacturerId);
                return View(dto);
            }

            try
            {
                var acType = await _context.AcTypes.FindAsync(id);
                if (acType == null)
                {
                    return NotFound();
                }

                acType.Code = normalizedCode;
                acType.Name = normalizedName;
                acType.Description = normalizedDescription;
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
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcTypeExists(id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // GET: Settings/AcTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var acType = await _context.AcTypes
                .Include(a => a.AcMainGroup)
                .Include(a => a.AircraftManufacturer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (acType == null)
            {
                return NotFound();
            }

            return View(acType);
        }

        // POST: Settings/AcTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var acType = await _context.AcTypes.FindAsync(id);
            if (acType != null)
            {
                _context.AcTypes.Remove(acType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Type d'aéronef supprimé avec succès";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AcTypeExists(int id)
        {
            return _context.AcTypes.Any(e => e.Id == id);
        }

        private async Task PopulateDropdowns(int? selectedAcMainGroupId = null, int? selectedManufacturerId = null)
        {
            ViewBag.AcMainGroups = new SelectList(
                await _context.AcMainGroups
                    .Where(g => g.Active)
                    .OrderBy(g => g.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                selectedAcMainGroupId
            );

            var manufacturers = await _context.AircraftManufacturers
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();

            var manufacturerList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Sélectionner un constructeur (optionnel) --"
                }
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