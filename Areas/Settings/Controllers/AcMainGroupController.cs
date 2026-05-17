using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.ViewModels.AcMainGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FRAProject.Areas.Settings.Controllers
{
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
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
            ViewBag.Bases = new SelectList(
                    await _unitOfWork.AcMainGroups.GetAllBasesAsync(),
                    "Id",
                    "BaseName",
                    baseId
    );
            ViewBag.AcCategories = new SelectList(
                await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(),
                "Id",
                "Name",
                categoryId
                );


            return View(groups);
        }
        // GET: AcMainGroup/Create
        public async Task<IActionResult> Create()
        {
            var model = new AcMainGroupViewModel();
               
            ViewBag.AcCategories = new SelectList(await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(), "Id", "Name");
            ViewBag.Bases = new SelectList(await _unitOfWork.Bases.GetAllAsync(), "Id", "BaseName");
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
                ViewBag.AcCategories = new SelectList(
                     await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(),
                     "Id",
                     "Name",
                     model.AcCategoryId
                 );

                ViewBag.Bases = new SelectList(
                    await _unitOfWork.AcMainGroups.GetAllBasesAsync(),
                    "Id",
                    "BaseName",
                    model.BaseId
                 );
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
                Categories = new SelectList(
                        await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(),
                        "Id",
                        "Name",
                        entity.AcCategoryId
                    ),
                            Bases = new SelectList(
                        await _unitOfWork.AcMainGroups.GetAllBasesAsync(),
                        "Id",
                        "BaseName",
                        entity.BaseId
                    )
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
                model.Categories = new SelectList(
                    await _unitOfWork.AcMainGroups.GetAllCategoriesAsync(),
                    "Id",
                    "Name",
                    model.AcCategoryId
                  );

                model.Bases = new SelectList(
                    await _unitOfWork.AcCategories.GetAllAsync(),
                    "Id",
                    "BaseName",
                    model.BaseId  
                 );

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