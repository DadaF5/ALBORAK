using FRAProject.Areas.HR.Models;
using FRAProject.Data;
using FRAProject.DTOs;
using FRAProject.Models;
using FRAProject.ViewModels.SubDepartment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Areas.HR.Controllers
{
    [Area("HR")]
    public class SubDepartmentController : Controller
    {
        private readonly FRAContext _context;

        public SubDepartmentController(FRAContext context)
        {
            _context = context;
        }

        // GET: SubDepartment
        public async Task<IActionResult> Index(SubDepartmentIndexViewModel vm)
        {
            var query = _context.SubDepartments
                .Include(sd => sd.Department)
                    .ThenInclude(d => d.Base)
                .AsQueryable();

            // ------------------------------
            // FILTERS
            // ------------------------------
            if (vm.BaseId.HasValue)
                query = query.Where(sd => sd.Department.BaseId == vm.BaseId);

            if (vm.DepartmentId.HasValue)
                query = query.Where(sd => sd.DepartmentId == vm.DepartmentId);

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
            {
                string term = vm.SearchTerm.Trim().ToLower();
                query = query.Where(sd => sd.Name.ToLower().Contains(term));
            }

            // ------------------------------
            // COUNT BEFORE PAGING
            // ------------------------------
            vm.TotalItems = await query.CountAsync();

            // ------------------------------
            // PAGINATION
            // ------------------------------
            int skip = (vm.PageNumber - 1) * vm.PageSize;

            vm.SubDepartments = await query
                .OrderBy(sd => sd.Name)
                .Skip(skip)
                .Take(vm.PageSize)
                .Select(sd => new SubDepartmentDto
                {
                    Id = sd.Id,
                    Name = sd.Name,
                    DepartmentId = sd.DepartmentId,
                    DepartmentName = sd.Department.Name,
                    BaseName = sd.Department.Base.BaseName
                })
                .ToListAsync();

            // ------------------------------
            // Populate dropdowns
            // ------------------------------
            vm.Bases = await _context.Bases
                .OrderBy(b => b.BaseName)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BaseName
                })
                .ToListAsync();

            vm.Departments = await _context.Departments
                .Where(d => !vm.BaseId.HasValue || d.BaseId == vm.BaseId)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();

            return View(vm);
        }



        // GET: SubDepartment/Create
        public async Task<IActionResult> Create()
        {
            var vm = new SubDepartmentViewModel
            {
                Departments = await GetDepartmentSelectList()
            };
            return View(vm);
        }

        // POST: SubDepartment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new SubDepartmentViewModel
                {
                    Name = dto.Name,
                    DepartmentId = dto.DepartmentId,
                    Departments = await GetDepartmentSelectList()
                };
                return View(vm);
            }

            var entity = new SubDepartment
            {
                Name = dto.Name,
                DepartmentId = dto.DepartmentId
            };

            _context.SubDepartments.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SubDepartment/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var sd = await _context.SubDepartments
                    .Include(s => s.Department)
                    .ThenInclude(d => d.Base)
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (sd == null)
                return NotFound();

            var vm = new SubDepartmentEditViewModel
            {
                Id = sd.Id,
                Name = sd.Name,
                BaseId = sd.Department.BaseId,
                DepartmentId = sd.DepartmentId,

                Bases = await _context.Bases
                    .OrderBy(b => b.BaseName)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.BaseName
                    }).ToListAsync(),

                Departments = await _context.Departments
                    .Where(d => d.BaseId == sd.Department.BaseId)
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToListAsync()
            };

            return View(vm);
        }

        // POST: SubDepartment/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var vm = new SubDepartmentViewModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    DepartmentId = dto.DepartmentId,
                    Departments = await GetDepartmentSelectList()
                };
                return View(vm);
            }

            var entity = await _context.SubDepartments.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.Name = dto.Name;
            entity.DepartmentId = dto.DepartmentId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: SubDepartment/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var sd = await _context.SubDepartments
                    .Include(s => s.Department)
                    .ThenInclude(d => d.Base)
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (sd == null)
                return NotFound();

            var vm = new SubDepartmentDeleteViewModel
            {
                Id = sd.Id,
                Name = sd.Name,
                DepartmentName = sd.Department.Name,
                BaseName = sd.Department.Base.BaseName
            };

            return View(vm);
        }

        // POST: SubDepartment/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.SubDepartments.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.SubDepartments.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helper: get Departments for dropdown
        private async Task<IEnumerable<SelectListItem>> GetDepartmentSelectList()
        {
            return await _context.Departments
                .Include(d => d.Base)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Name} ({d.Base.BaseName})"
                })
                .ToListAsync();
        }
    }
}
