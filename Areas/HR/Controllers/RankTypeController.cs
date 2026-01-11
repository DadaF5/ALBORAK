using FRAProject.Areas.HR.Models;
using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Models;
using FRAProject.ViewModels.RankType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.HR.Controllers
{
    [Area("HR")]
    public class RankTypeController : Controller
    {
        private readonly FRAContext _context;

        public RankTypeController(FRAContext context) => _context = context;

        // GET: RankType
        public async Task<IActionResult> Index()
        {
            var items = await _context.RankTypes
                .Select(x => new RankTypeViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToListAsync();

            return View(items);
        }

        // GET: RankType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RankType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RankTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var entity = new RankType
            {
                Name = dto.Name ,
                Description = dto.Description
            };

            _context.RankTypes.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: RankType/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.RankTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            var vm = new RankTypeViewModel
            {
                Id = entity.Id,
                Name = entity.Name ,
                Description = entity.Description
            };

            return View(vm);
        }

        // POST: RankType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RankTypeDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var entity = await _context.RankTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.Name = dto.Name;
            entity.Description=dto.Description;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: RankType/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.RankTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            return View(entity);
        }

        // POST: RankType/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.RankTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.RankTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
