using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using FRAProject.ViewModels.AcType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{

    [Area("AircraftMaintenance")]
    public class AcTypeController : Controller
    {
        private readonly FRAContext _context;

        public AcTypeController(FRAContext context)
        {
            _context = context;
        }

        // ----------------------------
        // INDEX
        // ----------------------------
        public async Task<IActionResult> Index()
        {
            var list = await _context.AcTypes
                .Include(t => t.AcMainGroup)
                .OrderBy(t => t.AcMainGroup.Name)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(list);
        }

        // ----------------------------
        // CREATE GET
        // ----------------------------
        public async Task<IActionResult> Create()
        {
            var vm = new AcTypeViewModel
            {
                AcMainGroups = await GetAcMainGroupsSelectList()
            };
            return View(vm);
        }

        // ----------------------------
        // CREATE POST
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcTypeViewModel vm)
        {
            // Duplicate check: same Name under same AcMainGroup
            if (await _context.AcTypes.AnyAsync(t =>
                    t.Name == vm.Name && t.AcMainGroupId == vm.AcMainGroupId))
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "This Type already exists under the selected Main Group.");
            }

            if (!ModelState.IsValid)
            {
                vm.AcMainGroups = await GetAcMainGroupsSelectList();
                return View(vm);
            }

            var entity = new AcType
            {
                Name = vm.Name,
                Description = vm.Description,
                AcMainGroupId = vm.AcMainGroupId
            };

            _context.AcTypes.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ----------------------------
        // EDIT GET
        // ----------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _context.AcTypes.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = new AcTypeViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                AcMainGroupId = entity.AcMainGroupId,
                AcMainGroups = await GetAcMainGroupsSelectList()
            };

            return View(vm);
        }

        // ----------------------------
        // EDIT POST
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcTypeViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            // Duplicate check excluding current entity
            if (await _context.AcTypes.AnyAsync(t =>
                    t.Id != vm.Id &&
                    t.Name == vm.Name &&
                    t.AcMainGroupId == vm.AcMainGroupId))
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "Another Type with the same Name exists under the selected Main Group.");
            }

            if (!ModelState.IsValid)
            {
                vm.AcMainGroups = await GetAcMainGroupsSelectList();
                return View(vm);
            }

            var entity = await _context.AcTypes.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Name = vm.Name;
            entity.Description = vm.Description;
            entity.AcMainGroupId = vm.AcMainGroupId;

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AcTypes.Any(e => e.Id == vm.Id)) return NotFound();
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

            var entity = await _context.AcTypes
                .Include(t => t.AcMainGroup)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null) return NotFound();

            return View(entity);
        }

        // ----------------------------
        // DELETE POST
        // ----------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.AcTypes.FindAsync(id);
            if (entity != null)
            {
                _context.AcTypes.Remove(entity);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ----------------------------
        // HELPER - Dropdown list
        // ----------------------------
        private async Task<IEnumerable<SelectListItem>> GetAcMainGroupsSelectList()
        {
            return await _context.AcMainGroups
                .OrderBy(mg => mg.Name)
                .Select(mg => new SelectListItem
                {
                    Value = mg.Id.ToString(),
                    Text = mg.Name
                }).ToListAsync();
        }
    }

}