using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.ViewModels.AcMainGroup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class AcMainGroupController : Controller
    {
        
        private readonly IUnitOfWork _unitOfWork;

        public AcMainGroupController(IUnitOfWork unitOfWork)
        {
            _unitOfWork=unitOfWork;        }

        // Get: AcMainGroup
        public async Task<IActionResult> Index(int? baseId, int? categoryId)
        {
            var groups= await _unitOfWork.AcMainGroups.GetAllAsync();
            
            if ( baseId.HasValue)
            {
                groups = groups.Where(g => g.BaseId == baseId);
            }  

            if (categoryId.HasValue)
            {
                groups = groups.Where(g => g.AcCategoryId == categoryId);
            }

            // Populate dropdowns
            ViewBag.Bases = new SelectList(await _unitOfWork.AcMainGroups.GetByBaseIdAsync(baseId ?? 0),
            ViewBag.AcCategories = new SelectList(await _unitOfWork.AcMainGroups.GetByAcCategoryIdAsync(categoryId ?? 0), "Id", "Name"));
                       
            return View(groups);
        }
        // GET: AcMainGroup/Create
        public async Task<IActionResult> Create()
        {
            var model = new AcMainGroupViewModel();
               
            ViewBag.AcCategories = new SelectList(await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(), "Id", "Name");
            ViewBag.Bases = new SelectList(await _unitOfWork.AcMainGroups.GetAllBasesAsync(), "Id", "BaseName");
            return View(model);
        }

        // POST: AcMainGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcMainGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns on error
                ViewBag.AcCategories = await _unitOfWork.AcMainGroups
                    .GetByAcCategoryIdAsync(0);
                ViewBag.Bases = await _unitOfWork.AcMainGroups
                    .GetByBaseIdAsync(0);
                return View(model);
                
            }

            var entity = new AcMainGroup
            {
                Name = model.Name,
                AcCategoryId = model.AcCategoryId,
                BaseId = model.BaseId,
                Description = model.Description,
                Active = model.IsActive
            };

            await _unitOfWork.AcMainGroups.AddAsync(entity);
            await _unitOfWork.CompleteAsync();          

            return RedirectToAction(nameof(Index));
        }


        // GET: AcMainGroup/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _unitOfWork.AcMainGroups.GetByIdAsync(id);
              
            if (entity == null)
                return NotFound();

            var vm = new AcMainGroupViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                AcCategoryId = entity.AcCategoryId,
                BaseId = entity.BaseId,
                IsActive = entity.Active,
                Categories= new SelectList(await _unitOfWork.AcMainGroups.GetByAcCategoryIdAsync(0)),
                Bases= new SelectList(await _unitOfWork.AcMainGroups.GetByBaseIdAsync(0))
            };

            return View(vm);
        }

        // POST: AcMainGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcMainGroupViewModel model)
        {
            if (id != model.Id || !ModelState.IsValid)
            {
                model.Categories = new SelectList(await _unitOfWork.AcMainGroups.GetByAcCategoryIdAsync(0));
                model.Bases = new SelectList(await _unitOfWork.AcMainGroups.GetByBaseIdAsync(0));
                return View(model);
            }

            // Load entity to update
            var entity = await _unitOfWork.AcMainGroups.GetByIdAsync(id);

            if (entity == null)
                return NotFound();

            // Update the entity
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.AcCategoryId = model.AcCategoryId;
            entity.BaseId = model.BaseId;
            entity.Active = model.IsActive;

            _unitOfWork.AcMainGroups.Update(entity);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        
    }
}