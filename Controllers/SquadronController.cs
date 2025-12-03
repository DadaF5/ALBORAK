using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class SquadronController : Controller
    {
        private readonly FRAContext _context;
        private readonly IWebHostEnvironment _env;

        public SquadronController(FRAContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Helper to delete a file given a web-relative path like "/uploads/squadrons/abc.png"
        private void DeletePhysicalFileIfExists(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath)) return;

            // Normalize and convert to physical path
            var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physical = Path.Combine(_env.WebRootPath ?? string.Empty, relative);

            try
            {
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
            }
            catch
            {
                // swallow exceptions for now; you might want to log
            }
        }

        // ===== INDEX ===== (unchanged from your previous Index)
        public async Task<IActionResult> Index(int? baseId, int? wingId)
        {
            var query = _context.Squadrons
                .Include(s => s.Wing)
                .ThenInclude(w => w.Base)
                .AsQueryable();

            if (baseId.HasValue)
            {
                query = query.Where(s => s.Wing != null && s.Wing.BaseId == baseId.Value);
            }

            if (wingId.HasValue)
            {
                query = query.Where(s => s.WingId == wingId.Value);
            }

            var squadrons = await query.OrderBy(s => s.Name).ToListAsync();

            var model = squadrons.Select(s => new SquadronViewModel
            {
                Id = s.Id,
                Name = s.Name,
                CallSign = s.CallSign,
                CallSignShort = s.CallSignShort,
                FrenchName = s.FrenchName,
                WingId = s.WingId,
                WingName = s.Wing?.Name ?? "",
                BaseId = s.Wing?.BaseId,
                BaseName = s.Wing?.Base?.BaseName ?? "",
                LogoPath = s.LogoPath,
                Active = s.Active
            }).ToList();

            var bases = await _context.Bases.OrderBy(b => b.BaseName).ToListAsync();
            ViewData["Bases"] = new SelectList(bases, "Id", "BaseName", baseId);

            IQueryable<Wing> wingsQuery = _context.Wings;
            if (baseId.HasValue)
            {
                wingsQuery = wingsQuery.Where(w => w.BaseId == baseId.Value);
            }
            var wingsList = await wingsQuery.OrderBy(w => w.Name).ToListAsync();
            ViewData["Wings"] = new SelectList(wingsList, "Id", "Name", wingId);

            return View(model);
        }

        // ===== CREATE GET =====
        public async Task<IActionResult> Create()
        {
            var model = new SquadronViewModel
            {
                // Populate Bases and Wings so the view can render the selects if needed later
                Bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                    .ToListAsync(),

                Wings = await _context.Wings
                    .OrderBy(w => w.Name)
                    .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                    .ToListAsync(),

                Active = true
            };

            return View(model);
        }

        // ===== CREATE POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SquadronViewModel model)
        {
            // server-side uniqueness check
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var nameTrim = model.Name.Trim().ToLower();
                if (await _context.Squadrons.AnyAsync(s => s.WingId == model.WingId && s.Name.ToLower() == nameTrim))
                {
                    ModelState.AddModelError("Name", "A squadron with this name already exists in the selected wing.");
                }
            }

            if (ModelState.IsValid)
            {
                string? logoPath = null;

                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads", "squadrons");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fs);
                    }

                    logoPath = "/uploads/squadrons/" + fileName;
                }

                var squadron = new Squadron
                {
                    Name = model.Name.Trim(),
                    CallSign = model.CallSign,
                    CallSignShort = model.CallSignShort,
                    FrenchName = model.FrenchName,
                    WingId = model.WingId,
                    LogoPath = logoPath,
                    Active = model.Active
                };

                _context.Squadrons.Add(squadron);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // repopulate dropdowns if validation failed
            model.Bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BaseName })
                    .ToListAsync();

            model.Wings = await _context.Wings
                    .OrderBy(w => w.Name)
                    .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                    .ToListAsync();

            return View(model);
        }

        // ===== EDIT GET =====
        public async Task<IActionResult> Edit(int id)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron == null) return NotFound();

            var model = new SquadronViewModel
            {
                Id = squadron.Id,
                Name = squadron.Name,
                CallSign = squadron.CallSign,
                CallSignShort = squadron.CallSignShort,
                FrenchName = squadron.FrenchName,
                WingId = squadron.WingId,
                LogoPath = squadron.LogoPath,
                Active = squadron.Active,
                Wings = await _context.Wings
                    .OrderBy(w => w.Name)
                    .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                    .ToListAsync()
            };

            return View(model);
        }

        // ===== EDIT POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SquadronViewModel model)
        {
            // uniqueness check: exclude current record by Id
            if (await _context.Squadrons.AnyAsync(s => s.Id != model.Id && s.WingId == model.WingId && s.Name.ToLower() == model.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", "A squadron with this name already exists in the selected wing.");
            }

            if (ModelState.IsValid)
            {
                var squadron = await _context.Squadrons.FindAsync(model.Id);
                if (squadron == null) return NotFound();

                // If the user asked to remove the existing logo, delete the file and clear the path
                if (model.RemoveLogo && !string.IsNullOrWhiteSpace(squadron.LogoPath))
                {
                    DeletePhysicalFileIfExists(squadron.LogoPath);
                    squadron.LogoPath = null;
                }

                // If a new file was uploaded, remove previous file (if any) then save new file
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    // delete old
                    if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
                    {
                        DeletePhysicalFileIfExists(squadron.LogoPath);
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads", "squadrons");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fs);
                    }

                    squadron.LogoPath = "/uploads/squadrons/" + fileName;
                }

                squadron.Name = model.Name.Trim();
                squadron.CallSign = model.CallSign;
                squadron.CallSignShort = model.CallSignShort;
                squadron.FrenchName = model.FrenchName;
                squadron.WingId = model.WingId;
                squadron.Active = model.Active;

                _context.Squadrons.Update(squadron);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate Wings if validation failed
            model.Wings = await _context.Wings
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(model);
        }

        // ===== DETAILS =====
        public async Task<IActionResult> Details(int id)
        {
            var squadron = await _context.Squadrons
                .Include(s => s.Wing)
                .ThenInclude(w => w.Base)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (squadron == null) return NotFound();

            var model = new SquadronViewModel
            {
                Id = squadron.Id,
                Name = squadron.Name,
                CallSign = squadron.CallSign,
                CallSignShort = squadron.CallSignShort,
                FrenchName = squadron.FrenchName,
                WingId = squadron.WingId,
                WingName = squadron.Wing?.Name ?? "",
                BaseName = squadron.Wing?.Base?.BaseName ?? "",
                LogoPath = squadron.LogoPath,
                Active = squadron.Active
            };

            return View(model);
        }

        // ===== Delete Logo action (POST) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo(int id, string? returnUrl)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
            {
                DeletePhysicalFileIfExists(squadron.LogoPath);
                squadron.LogoPath = null;
                _context.Squadrons.Update(squadron);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ===== DELETE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron != null)
            {
                // delete logo file if present
                if (!string.IsNullOrWhiteSpace(squadron.LogoPath))
                {
                    DeletePhysicalFileIfExists(squadron.LogoPath);
                }

                _context.Squadrons.Remove(squadron);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}