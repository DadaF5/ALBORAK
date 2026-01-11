using FRAProject.Areas.HR.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FRAProject.Areas.HR.Controllers
{
    [Area("HR")]
    public class PersonController : Controller
    {
        private readonly FRAContext _context;

        public PersonController(FRAContext context)
        {
            _context = context;
        }

        // ===== LIST =====
        // Index page (initial load)
        public IActionResult Index()
        {
            // Pass empty list for initial table load
            var emptyList = new List<PersonViewModel>();

            // Populate dropdowns as List<SelectListItem>
            ViewData["Bases"] = _context.Bases
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BaseName
                }).ToList();

            ViewData["Ranks"] = _context.Ranks
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                }).ToList();

            return View(emptyList);
        }

        // AJAX endpoint to return filtered, paginated table
        public async Task<IActionResult> PersonTable(
            int? baseId, int? rankId, bool? activeOnly, string? search, int page = 1, int pageSize = 10)
        {
            var query = _context.Persons
                .Include(p => p.Rank)
                .Include(p => p.SubDepartment)
                    .ThenInclude(sd => sd.Department)
                        .ThenInclude(d => d.Base)
                .AsQueryable();

            // Filters
            if (baseId.HasValue)
                query = query.Where(p => p.SubDepartment.Department.BaseId == baseId.Value);

            if (rankId.HasValue)
                query = query.Where(p => p.RankId == rankId.Value);

            if (activeOnly.HasValue && activeOnly.Value)
                query = query.Where(p => p.Active);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.FirstName.ToLower().Contains(search)
                                      || p.LastName.ToLower().Contains(search)
                                      || p.Matricule.ToLower().Contains(search)
                                      || p.PatrimonialStatus != null && p.PatrimonialStatus.ToLower().Contains(search));
                                      
            }

            var totalCount = await query.CountAsync();

            var persons = await query
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = persons.Select(p => new PersonViewModel
            {
                Id = p.Id,
                RankId = p.RankId,
                RankName = p.Rank?.Name ?? "",
                Matricule = p.Matricule,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Gender = p.Gender,
                SubDepartmentId = p.SubDepartmentId,
                SubDepartmentName = p.SubDepartment?.Name ?? "",
                DepartmentId = p.SubDepartment?.DepartmentId ?? 0,
                DepartmentName = p.SubDepartment?.Department?.Name ?? "",
                BaseId = p.SubDepartment?.Department?.BaseId ?? 0,
                BaseName = p.SubDepartment?.Department?.Base?.BaseName ?? "",
                Active = p.Active,
                Photo = p.Photo,
                PatrimonialStatus=p.PatrimonialStatus
            }).ToList();

            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return PartialView("_PersonTable", model);
        }

        // ===== DETAILS =====
        public async Task<IActionResult> Details(int id)
        {
            var person = await _context.Persons
                .Include(p => p.Rank)
                .Include(p => p.SubDepartment)
                    .ThenInclude(sd => sd.Department)
                        .ThenInclude(d => d.Base)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
                return NotFound();

            var model = new PersonViewModel
            {
                Id = person.Id,
                RankId = person.RankId,
                RankName = person.Rank?.Name ?? "",
                Matricule = person.Matricule,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Gender = person.Gender,
                DateOfBirth = person.DateOfBirth,
                NationalId = person.NationalId,
                Speciality = person.Speciality,
                City = person.City,
                Country = person.Country,
                Active = person.Active,
                Photo = person.Photo,
                PatrimonialStatus = person.PatrimonialStatus,
                // Hierarchy
                BaseId = person.SubDepartment?.Department?.BaseId ?? 0,
                BaseName = person.SubDepartment?.Department?.Base?.BaseName ?? "",
                DepartmentId = person.SubDepartment?.DepartmentId ?? 0,
                DepartmentName = person.SubDepartment?.Department?.Name ?? "",
                SubDepartmentId = person.SubDepartmentId,
                SubDepartmentName = person.SubDepartment?.Name ?? ""
            };

            return View(model);
        }


        // ===== CREATE (GET) =====
        public async Task<IActionResult> Create()
        {
            // Populate Base dropdown
            ViewData["Bases"] = new SelectList(await _context.Bases.ToListAsync(), "Id", "BaseName");

            // Initially empty dropdowns for Department and SubDepartment
            ViewData["Departments"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["SubDepartments"] = new SelectList(Enumerable.Empty<SelectListItem>());

            // Populate Ranks dropdown
            ViewData["Ranks"] = new SelectList(await _context.Ranks.ToListAsync(), "Id", "Name");
            return View();
        }

        // ===== CREATE (POST) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Handle Photo upload
                byte[]? photoBytes = null;
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await model.PhotoFile.CopyToAsync(ms);
                        photoBytes = ms.ToArray();
                    }
                }

                // Map ViewModel to Entity
                var person = new Person
                {
                    RankId = model.RankId,
                    Matricule = model.Matricule,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    SubDepartmentId = model.SubDepartmentId,
                    DateOfBirth = model.DateOfBirth,
                    NationalId = model.NationalId,
                    Speciality = model.Speciality,
                    City = model.City,
                    Country = model.Country,
                    Active = model.Active,
                    Photo = photoBytes,
                    PatrimonialStatus = model.PatrimonialStatus


                };

                // Save to database
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, repopulate dropdowns
            ViewData["Bases"] = new SelectList(await _context.Bases.ToListAsync(), "Id", "BaseName", model.BaseId);

            if (model.BaseId > 0)
                ViewData["Departments"] = new SelectList(
                    await _context.Departments.Where(d => d.BaseId == model.BaseId).ToListAsync(),
                    "Id", "Name", model.DepartmentId);
            else
                ViewData["Departments"] = new SelectList(Enumerable.Empty<SelectListItem>());

            if (model.DepartmentId > 0)
                ViewData["SubDepartments"] = new SelectList(
                    await _context.SubDepartments.Where(sd => sd.DepartmentId == model.DepartmentId).ToListAsync(),
                    "Id", "Name", model.SubDepartmentId);
            else
                ViewData["SubDepartments"] = new SelectList(Enumerable.Empty<SelectListItem>());

            ViewData["Ranks"] = new SelectList(await _context.Ranks.ToListAsync(), "Id", "Name", model.RankId);

            return View(model);
        }


        // ===== EDIT (GET) =====
        public async Task<IActionResult> Edit(int id)
        {
            var person = await _context.Persons
                .Include(p => p.SubDepartment)
                    .ThenInclude(sd => sd.Department)
                        .ThenInclude(d => d.Base)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
                return NotFound();

            var model = new PersonViewModel
            {
                Id = person.Id,
                RankId = person.RankId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Matricule = person.Matricule,
                Gender = person.Gender,
                DateOfBirth = person.DateOfBirth,
                NationalId = person.NationalId,
                Speciality = person.Speciality,
                City = person.City,
                Country = person.Country,
                Active = person.Active,
                Photo = person.Photo,// can be null  

                BaseId = person.SubDepartment?.Department?.BaseId ?? 0,
                DepartmentId = person.SubDepartment?.DepartmentId ?? 0,
                SubDepartmentId = person.SubDepartmentId,
                PatrimonialStatus = person.PatrimonialStatus
            };

            // Populate dropdowns
            ViewData["Bases"] = new SelectList(await _context.Bases.ToListAsync(), "Id", "BaseName", model.BaseId);
            ViewData["Departments"] = new SelectList(await _context.Departments.Where(d => d.BaseId == model.BaseId).ToListAsync(), "Id", "Name", model.DepartmentId);
            ViewData["SubDepartments"] = new SelectList(await _context.SubDepartments.Where(sd => sd.DepartmentId == model.DepartmentId).ToListAsync(), "Id", "Name", model.SubDepartmentId);
            ViewData["Ranks"] = new SelectList(await _context.Ranks.ToListAsync(), "Id", "Name", model.RankId);

            return View(model); // <-- important, cannot return null!
        }



        // ===== EDIT (POST) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Refill dropdowns if validation fails
                ViewData["Bases"] = new SelectList(await _context.Bases.ToListAsync(), "Id", "BaseName", model.BaseId);
                ViewData["Departments"] = new SelectList(await _context.Departments.Where(d => d.BaseId == model.BaseId).ToListAsync(), "Id", "Name", model.DepartmentId);
                ViewData["SubDepartments"] = new SelectList(await _context.SubDepartments.Where(sd => sd.DepartmentId == model.DepartmentId).ToListAsync(), "Id", "Name", model.SubDepartmentId);
                ViewData["Ranks"] = new SelectList(await _context.Ranks.ToListAsync(), "Id", "Name", model.RankId);

                return View(model);
            }

            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return NotFound();

            // Update fields
            person.RankId = model.RankId;
            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.Matricule = model.Matricule;
            person.Gender = model.Gender;
            person.DateOfBirth = model.DateOfBirth;
            person.NationalId = model.NationalId;
            person.Speciality = model.Speciality;
            person.City = model.City;
            person.Country = model.Country;
            person.Active = model.Active;
            person.PatrimonialStatus = model.PatrimonialStatus;

            // Update sub-department
            person.SubDepartmentId = model.SubDepartmentId;

            // Update PHOTO only if a new file is provided
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await model.PhotoFile.CopyToAsync(ms);
                    person.Photo = ms.ToArray(); // replace old photo
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ===== DELETE (GET) =====
        public async Task<IActionResult> Delete(int id)
        {
            var person = await _context.Persons
                .Include(p => p.Rank)
                .Include(p => p.SubDepartment)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null) return NotFound();

            var model = new PersonViewModel
            {
                Id = person.Id,
                RankId = person.RankId,
                RankName = person.Rank?.Name ?? "",
                Matricule = person.Matricule,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Gender = person.Gender,
                SubDepartmentId = person.SubDepartmentId,
                SubDepartmentName = person.SubDepartment?.Name ?? "",
                DateOfBirth = person.DateOfBirth,
                NationalId = person.NationalId,
                Speciality = person.Speciality,
                City = person.City,
                Country = person.Country,
                Active = person.Active,
                Photo = person.Photo,
                PatrimonialStatus = person.PatrimonialStatus
            };

            return View(model);
        }

        // ===== DELETE (POST) =====
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null)
            {
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== HELPER METHOD: Check if Person Exists =====
        // JSON 
        [HttpGet]
        public async Task<JsonResult> GetDepartmentsByBase(int baseId)
        {
            var departments = await _context.Departments
                .Where(d => d.BaseId == baseId)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            return Json(departments);
        }

        [HttpGet]
        public async Task<JsonResult> GetSubDepartmentsByDepartment(int departmentId)
        {
            var subDepartments = await _context.SubDepartments
                .Where(sd => sd.DepartmentId == departmentId)
                .Select(sd => new { sd.Id, sd.Name })
                .ToListAsync();
            return Json(subDepartments);
        }
    }
}
