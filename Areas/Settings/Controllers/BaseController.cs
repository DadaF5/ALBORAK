using FRAProject.Areas.HR.Models;
using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class BaseController : Controller
    {
        private readonly FRAContext _context;

        public BaseController(FRAContext context)
        {
            _context = context;
        }

        // Return partial view with list (AJAX)
        public async Task<IActionResult> List()
        {
            var bases = await _context.Bases
                .OrderBy(b => b.BaseName)
                .Select(b => new BaseDto
                {
                    Id = b.Id,
                    BaseName = b.BaseName,
                    Longitude = b.Longitude,
                    Latitude = b.Latitude,
                    BaseNameLocal = b.BaseCode + " - " + b.Location
                })
                .ToListAsync();

            return PartialView("_BaseList", bases);
        }

        // GET: Settings/Base/Create
        public IActionResult Create(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = GetSafeReturnUrl(returnUrl);
            var dto = new BaseCreateDto { IsActive = true };
            return View(dto);
        }

        // POST: Settings/Base/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaseCreateDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = GetSafeReturnUrl(returnUrl);

            // Check for duplicate BaseCode
            if (await _context.Bases.AnyAsync(b => b.BaseCode == dto.BaseCode))
            {
                ModelState.AddModelError("BaseCode", $"Le code '{dto.BaseCode}' existe déjà. Veuillez choisir un code unique.");
            }

            // Check for duplicate BaseName
            if (await _context.Bases.AnyAsync(b => b.BaseName == dto.BaseName))
            {
                ModelState.AddModelError("BaseName", $"Le nom '{dto.BaseName}' existe déjà.");
            }

            if (ModelState.IsValid)
            {
                var baseEntity = new Base
                {
                    BaseCode = dto.BaseCode.Trim().ToUpper(),
                    BaseName = dto.BaseName.Trim(),
                    Location = dto.Location.Trim(),
                    IsActive = dto.IsActive,
                    Latitude = dto.Latitude,    // ← Added
                    Longitude = dto.Longitude   // ← Added
                };

                _context.Bases.Add(baseEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Base créée avec succès";
                if (IsAjaxRequest())
                {
                    return Json(new { success = true, redirectUrl = GetSafeReturnUrl(returnUrl) });
                }

                return Redirect(GetSafeReturnUrl(returnUrl));
            }

            return View(dto);
        }

        // GET: Settings/Base/Edit/5
        public async Task<IActionResult> Edit(int? id, string? returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baseEntity = await _context.Bases.FindAsync(id);
            if (baseEntity == null)
            {
                return NotFound();
            }

            var dto = new BaseCreateDto
            {
                Id = baseEntity.Id,
                BaseCode = baseEntity.BaseCode,
                BaseName = baseEntity.BaseName,
                Location = baseEntity.Location,
                IsActive = baseEntity.IsActive,
                Latitude = baseEntity.Latitude,     // ← Added
                Longitude = baseEntity.Longitude    // ← Added
            };

            ViewData["ReturnUrl"] = GetSafeReturnUrl(returnUrl);
            return View(dto);
        }

        // POST: Settings/Base/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BaseCreateDto dto, string? returnUrl = null)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = GetSafeReturnUrl(returnUrl);

            // Check for duplicate BaseCode (excluding current record)
            if (await _context.Bases.AnyAsync(b => b.BaseCode == dto.BaseCode && b.Id != id))
            {
                ModelState.AddModelError("BaseCode", $"Le code '{dto.BaseCode}' est déjà utilisé par une autre base.");
            }

            // Check for duplicate BaseName (excluding current record)
            if (await _context.Bases.AnyAsync(b => b.BaseName == dto.BaseName && b.Id != id))
            {
                ModelState.AddModelError("BaseName", $"Le nom '{dto.BaseName}' est déjà utilisé par une autre base.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var baseEntity = await _context.Bases.FindAsync(id);
                    if (baseEntity == null)
                    {
                        return NotFound();
                    }

                    baseEntity.BaseCode = dto.BaseCode.Trim().ToUpper();
                    baseEntity.BaseName = dto.BaseName.Trim();
                    baseEntity.Location = dto.Location.Trim();
                    baseEntity.IsActive = dto.IsActive;
                    baseEntity.Latitude = dto.Latitude;     // ← Added
                    baseEntity.Longitude = dto.Longitude;   // ← Added

                    _context.Update(baseEntity);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Base modifiée avec succès";
                    if (IsAjaxRequest())
                    {
                        return Json(new { success = true, redirectUrl = GetSafeReturnUrl(returnUrl) });
                    }

                    return Redirect(GetSafeReturnUrl(returnUrl));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaseExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(dto);
        }

        // POST: Settings/Base/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var baseEntity = await _context.Bases.FindAsync(id);
            if (baseEntity != null)
            {
                _context.Bases.Remove(baseEntity);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private bool BaseExists(int id)
        {
            return _context.Bases.Any(e => e.Id == id);
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return Url.Action("Index", "Home", new { area = "Settings", tab = "bases" })!;
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}
