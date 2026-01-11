using FRAProject.Areas.HR.Models;
using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Helpers;
using FRAProject.Models;
using FRAProject.ViewModels.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.HR.Controllers
{
    [Area("HR")]
    public class BaseController : Controller
    {
        private readonly FRAContext _context;

        public BaseController(FRAContext context)
        {
            _context = context;
        }

        // ============================
        // INDEX: Search + Pagination
        // ============================
        public async Task<IActionResult> Index(string search, int pageNumber = 1)
        {
            int pageSize = 10;

            // Base query
            var query = _context.Bases
                .AsQueryable();

            // APPLY SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(b =>
                    b.BaseName.ToLower().Contains(term) ||
                    b.BaseNameLocal != null && b.BaseNameLocal.ToLower().Contains(term)
                );
            }

            // Get total count
            int count = await query.CountAsync();

            // Get paginated list
            var baseList = await query
                .OrderBy(b => b.BaseName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BaseDto
                {
                    Id = b.Id,
                    BaseName = b.BaseName,
                    BaseNameLocal = b.BaseNameLocal
                })
                .ToListAsync();

            // Wrap in paginated list
            var paginated = new PaginatedList<BaseDto>(baseList, count, pageNumber, pageSize);

            ViewBag.Search = search;

            return View(paginated);
        }



        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> Details(int id)
        {
            var b = await _context.Bases.FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return NotFound();

            var dto = new BaseDto
            {
                Id = b.Id,
                BaseName = b.BaseName,
                BaseNameLocal = b.BaseNameLocal
            };

            return View(dto);
        }

        // ============================
        // CREATE
        // ============================
        // ----------------------------
        // CREATE GET
        // ----------------------------
       
        public IActionResult Create()
        {
            var vm = new BaseViewModel()
            {
                Base = new BaseCreateDto()
            };

            return View(vm);   // ✔ FIXED → return BaseViewModel
        }

        // ----------------------------
        // CREATE POST
        // ----------------------------
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaseViewModel vm)
        {
            // DUPLICATE CHECK
            bool exists = await _context.Bases
                .AnyAsync(x => x.BaseName.ToLower() == vm.Base.BaseName.ToLower());

            if (exists)
            {
                ModelState.AddModelError("Base.BaseName", "This Base Name already exists.");
                return View(vm);
            }

            // CREATE ENTITY
            var entity = new Base
            {
                BaseName = vm.Base.BaseName,
                BaseNameLocal = vm.Base.BaseNameLocal
            };

            _context.Bases.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ============================
        // EDIT
        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var baseEntity = await _context.Bases.FindAsync(id);
            if (baseEntity == null)
                return NotFound();

            var vm = new BaseViewModel
            {
                Base = new BaseCreateDto
                {
                    Id = baseEntity.Id,
                    BaseName = baseEntity.BaseName,
                    BaseNameLocal = baseEntity.BaseNameLocal
                }
            };

            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BaseViewModel vm)
        {
            // DUPLICATE CHECK (ignore same Id)
            bool exists = await _context.Bases
                .AnyAsync(x =>
                    x.BaseName.ToLower() == vm.Base.BaseName.ToLower() &&
                    x.Id != vm.Base.Id);

            if (exists)
            {
                ModelState.AddModelError("Base.BaseName", "This Base Name is already used by another Base.");
                return View(vm);
            }

            // UPDATE ENTITY
            var entity = await _context.Bases.FindAsync(vm.Base.Id);
            if (entity == null)
                return NotFound();

            entity.BaseName = vm.Base.BaseName;
            entity.BaseNameLocal = vm.Base.BaseNameLocal;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // ============================
        // DELETE
        // ============================
        public async Task<IActionResult> Delete(int id)
        {
            var b = await _context.Bases.FindAsync(id);
            if (b == null) return NotFound();

            var dto = new BaseDto
            {
                Id = b.Id,
                BaseName = b.BaseName,
                BaseNameLocal = b.BaseNameLocal
            };

            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var b = await _context.Bases.FindAsync(id);
            if (b == null) return NotFound();

            _context.Bases.Remove(b);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

