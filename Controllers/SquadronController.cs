using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

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

        // ===== INDEX =====
        public async Task<IActionResult> Index(int? wingId)
        {
            var query = _context.Squadrons
                .Include(s => s.Wing)
                .ThenInclude(w => w.Department)
                .AsQueryable();

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
                LogoPath = s.LogoPath,
                Active = s.Active
            }).ToList();

            ViewData["Wings"] = new SelectList(await _context.Wings.ToListAsync(), "Id", "Name", wingId);

            return View(model);
        }

        // ===== CREATE GET =====
        public async Task<IActionResult> Create()
        {
            var model = new SquadronViewModel
            {
                Wings = await _context.Wings
                    .OrderBy(w => w.Name)
                    .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                    .ToListAsync()
            };
            return View(model);
        }

        // ===== CREATE POST =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SquadronViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? logoPath = null;

                // Handle file upload
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/squadrons");
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
                    Name = model.Name,
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

            // Reload Wings if model invalid
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
            if (ModelState.IsValid)
            {
                var squadron = await _context.Squadrons.FindAsync(model.Id);
                if (squadron == null) return NotFound();

                squadron.Name = model.Name;
                squadron.CallSign = model.CallSign;
                squadron.CallSignShort = model.CallSignShort;
                squadron.FrenchName = model.FrenchName;
                squadron.WingId = model.WingId;
                squadron.Active = model.Active;

                // Handle Logo file upload
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/squadrons");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(model.LogoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fs);
                    }

                    squadron.LogoPath = "/uploads/squadrons/" + fileName;
                }

                _context.Squadrons.Update(squadron);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload Wings if model invalid
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
                .ThenInclude(w => w.Department)
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
                LogoPath = squadron.LogoPath,
                Active = squadron.Active
            };

            return View(model);
        }

        // ===== DELETE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var squadron = await _context.Squadrons.FindAsync(id);
            if (squadron != null)
            {
                _context.Squadrons.Remove(squadron);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
