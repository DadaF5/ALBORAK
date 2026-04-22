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
    public class AircraftVersionsController : Controller
    {
        private readonly FRAContext _context;

        public AircraftVersionsController(FRAContext context)
        {
            _context = context;
        }

        // Return partial view with list (AJAX)
        public async Task<IActionResult> List()
        {
            var versions = await _context.AircraftVersions
                .Include(v => v.AcType)
                .OrderBy(v => v.AcType.Name)
                .ThenBy(v => v.SortOrder)
                .ThenBy(v => v.Name)
                .Select(v => new AircraftVersionDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Description = v.Description,
                    IsActive = v.IsActive,
                    SortOrder = v.SortOrder,
                    AcTypeId = v.AcTypeId,
                    AcTypeName = v.AcType.Name
                })
                .ToListAsync();

            return PartialView("_AircraftVersionsList", versions);
        }

        // GET: Settings/AircraftVersions/Create
        public async Task<IActionResult> Create()
        {
            await PopulateAcTypeDropdown();
            var dto = new AircraftVersionCreateDto { IsActive = true, SortOrder = 99 };
            return View(dto);
        }

        // POST: Settings/AircraftVersions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftVersionCreateDto dto)
        {
            // Check for duplicate Code within same AcType
            if (await _context.AircraftVersions.AnyAsync(v => v.Code == dto.Code && v.AcTypeId == dto.AcTypeId))
            {
                ModelState.AddModelError("Code", $"Le code '{dto.Code}' existe déjà pour ce type d'aéronef.");
            }

            // Check for duplicate Name within same AcType
            if (await _context.AircraftVersions.AnyAsync(v => v.Name == dto.Name && v.AcTypeId == dto.AcTypeId))
            {
                ModelState.AddModelError("Name", $"Le nom '{dto.Name}' existe déjà pour ce type d'aéronef.");
            }

            if (ModelState.IsValid)
            {
                var version = new AircraftVersion
                {
                    Code = dto.Code.Trim().ToUpper(),
                    Name = dto.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    AcTypeId = dto.AcTypeId,
                    IsActive = dto.IsActive,
                    SortOrder = dto.SortOrder
                };

                _context.AircraftVersions.Add(version);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Version d'aéronef créée avec succès";
                return RedirectToAction("Index", "Home", new { area = "Settings" });
            }

            await PopulateAcTypeDropdown(dto.AcTypeId);
            return View(dto);
        }

        // GET: Settings/AircraftVersions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var version = await _context.AircraftVersions.FindAsync(id);
            if (version == null)
            {
                return NotFound();
            }

            var dto = new AircraftVersionCreateDto
            {
                Id = version.Id,
                Code = version.Code,
                Name = version.Name,
                Description = version.Description,
                AcTypeId = version.AcTypeId,
                IsActive = version.IsActive,
                SortOrder = version.SortOrder
            };

            await PopulateAcTypeDropdown(dto.AcTypeId);
            return View(dto);
        }

        // POST: Settings/AircraftVersions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftVersionCreateDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            // Check for duplicate Code within same AcType (excluding current record)
            if (await _context.AircraftVersions.AnyAsync(v => v.Code == dto.Code && v.AcTypeId == dto.AcTypeId && v.Id != id))
            {
                ModelState.AddModelError("Code", $"Le code '{dto.Code}' est déjà utilisé pour ce type d'aéronef.");
            }

            // Check for duplicate Name within same AcType (excluding current record)
            if (await _context.AircraftVersions.AnyAsync(v => v.Name == dto.Name && v.AcTypeId == dto.AcTypeId && v.Id != id))
            {
                ModelState.AddModelError("Name", $"Le nom '{dto.Name}' est déjà utilisé pour ce type d'aéronef.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var version = await _context.AircraftVersions.FindAsync(id);
                    if (version == null)
                    {
                        return NotFound();
                    }

                    version.Code = dto.Code.Trim().ToUpper();
                    version.Name = dto.Name.Trim();
                    version.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
                    version.AcTypeId = dto.AcTypeId;
                    version.IsActive = dto.IsActive;
                    version.SortOrder = dto.SortOrder;

                    _context.Update(version);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Version d'aéronef modifiée avec succès";
                    return RedirectToAction("Index", "Home", new { area = "Settings" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VersionExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateAcTypeDropdown(dto.AcTypeId);
            return View(dto);
        }

        // POST: Settings/AircraftVersions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var version = await _context.AircraftVersions.FindAsync(id);
            if (version != null)
            {
                _context.AircraftVersions.Remove(version);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private bool VersionExists(int id)
        {
            return _context.AircraftVersions.Any(e => e.Id == id);
        }

        // Helper method to populate AcType dropdown
        private async Task PopulateAcTypeDropdown(int? selectedAcTypeId = null)
        {
            ViewBag.AcTypes = new SelectList(
                await _context.AcTypes
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .ToListAsync(),
                "AcTypeId",
                "AcTypeName",
                selectedAcTypeId
            );
        }
    }
}
