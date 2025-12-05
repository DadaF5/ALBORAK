using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Models;
using FRAProject.ViewModels.Rank;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    public class RankController : Controller
    {
        private readonly FRAContext _context;

        public RankController(FRAContext context)
        {
            _context = context;
        }

        // GET: Rank
        public async Task<IActionResult> Index()
        {
            var ranks = await _context.Ranks
                .Include(r => r.RankType)
                .OrderBy(r => r.Sequence)
                .Select(r => new RankViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    FullRank = r.FullRank,
                    Sequence = r.Sequence,                   
                    RankTypeId = r.RankTypeId
                })
                .ToListAsync();

            return View(ranks);
        }

        // GET: Rank/Create
        public async Task<IActionResult> Create()
        {
            var vm = new RankViewModel
            {
                RankTypes = await GetRankTypeSelectList()
            };

            return View(vm);
        }

        // POST: Rank/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RankDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new RankViewModel
                {
                    Name = dto.Name,
                    FullRank = dto.FullRank,
                    Sequence = dto.Sequence,
                    RankTypeId = dto.RankTypeId,
                    RankTypes = await GetRankTypeSelectList()
                };
                return View(vm);
            }

            var entity = new Rank
            {
                Name = dto.Name,
                FullRank = dto.FullRank,
                Sequence = dto.Sequence,
                RankTypeId = dto.RankTypeId
            };

            _context.Ranks.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Rank/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Ranks.FindAsync(id);
            if (entity == null)
                return NotFound();

            var vm = new RankViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                FullRank = entity.FullRank,
                Sequence = entity.Sequence,
                RankTypeId = entity.RankTypeId,
                RankTypes = await GetRankTypeSelectList()
            };

            return View(vm);
        }

        // POST: Rank/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RankDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new RankViewModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    FullRank = dto.FullRank,
                    Sequence = dto.Sequence,
                    RankTypeId = dto.RankTypeId,
                    RankTypes = await GetRankTypeSelectList()
                };
                return View(vm);
            }

            var entity = await _context.Ranks.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.Name = dto.Name;
            entity.FullRank = dto.FullRank;
            entity.Sequence = dto.Sequence;
            entity.RankTypeId = dto.RankTypeId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Rank/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Ranks
                .Include(r => r.RankType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null)
                return NotFound();

            return View(entity);
        }

        // POST: Rank/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.Ranks.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Ranks.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helper: Get RankType select list
        private async Task<IEnumerable<SelectListItem>> GetRankTypeSelectList()
        {
            return await _context.RankTypes
                .OrderBy(rt => rt.Name)
                .Select(rt => new SelectListItem
                {
                    Value = rt.Id.ToString(),
                    Text = rt.Name
                })
                .ToListAsync();
        }
    }
}

