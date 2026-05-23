using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FRAProject.Data;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    public class InspectionTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FRAContext _context;

        public InspectionTypesController(IUnitOfWork unitOfWork, FRAContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET: AircraftMaintenance/InspectionTypes
        public async Task<IActionResult> Index()
        {
            var list = await _unitOfWork.InspectionTypes.GetAllWithDetailsAsync();
            return View(list);
        }

        // GET: AircraftMaintenance/InspectionTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _unitOfWork.InspectionTypes.GetByIdWithDetailsAsync(id.Value);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // GET: AircraftMaintenance/InspectionTypes/Create
        public async Task<IActionResult> Create()
        {
            var vm = new InspectionTypeViewModel();
            await PopulateDropdownsAsync(vm, null);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InspectionTypeViewModel vm)
        {
            if (await _unitOfWork.InspectionTypes.ExistsByCodeAsync(vm.AcTypeId, vm.Code))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "An inspection type with this code already exists for the selected aircraft type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm, null);
                return View(vm);
            }

            var entity = new InspectionType
            {
                Code = vm.Code.Trim().ToUpper(),
                Name = vm.Name.Trim(),
                Description = vm.Description?.Trim(),
                AcTypeId = vm.AcTypeId,
                NextInspectionTypeId = vm.NextInspectionTypeId,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive
            };

            await _unitOfWork.InspectionTypes.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/InspectionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _unitOfWork.InspectionTypes.GetByIdWithDetailsAsync(id.Value);
            if (entity == null) return NotFound();

            var vm = MapToViewModel(entity);
            await PopulateDropdownsAsync(vm, entity.Id);
            return View(vm);
        }

        // POST: AircraftMaintenance/InspectionTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InspectionTypeViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (await _unitOfWork.InspectionTypes.ExistsByCodeAsync(vm.AcTypeId, vm.Code, vm.Id))
            {
                ModelState.AddModelError(nameof(vm.Code),
                    "Another inspection type with this code already exists for the selected aircraft type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm, vm.Id);
                return View(vm);
            }

            var entity = await _unitOfWork.InspectionTypes.GetByIdWithDetailsAsync(id);
            if (entity == null) return NotFound();

            // Prevent self-reference cycle: NextInspectionTypeId cannot point to itself
            if (vm.NextInspectionTypeId == entity.Id)
            {
                ModelState.AddModelError(nameof(vm.NextInspectionTypeId),
                    "An inspection type cannot reference itself as the next type.");
                await PopulateDropdownsAsync(vm, vm.Id);
                return View(vm);
            }

            entity.Code = vm.Code.Trim().ToUpper();
            entity.Name = vm.Name.Trim();
            entity.Description = vm.Description?.Trim();
            entity.AcTypeId = vm.AcTypeId;
            entity.NextInspectionTypeId = vm.NextInspectionTypeId;
            entity.SortOrder = vm.SortOrder;
            entity.IsActive = vm.IsActive;

            _unitOfWork.InspectionTypes.Update(entity);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: AircraftMaintenance/InspectionTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entity = await _unitOfWork.InspectionTypes.GetByIdWithDetailsAsync(id.Value);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST: AircraftMaintenance/InspectionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _unitOfWork.InspectionTypes.GetByIdAsync(id);
            if (entity != null)
            {
                _unitOfWork.InspectionTypes.Delete(entity);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------
        // Helpers
        // -----------------------------------------------

        private static InspectionTypeViewModel MapToViewModel(InspectionType entity) =>
            new InspectionTypeViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                AcTypeId = entity.AcTypeId,
                NextInspectionTypeId = entity.NextInspectionTypeId,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

        private async Task PopulateDropdownsAsync(InspectionTypeViewModel vm, int? editingId)
        {
            vm.AcTypes = await _context.AcTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            // NextInspectionType options: only same AcType, excluding self during edit
            if (vm.AcTypeId > 0)
            {
                vm.NextInspectionTypes = await _context.InspectionTypes
                    .Where(x => x.AcTypeId == vm.AcTypeId && (!editingId.HasValue || x.Id != editingId.Value))
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Code)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = $"{x.Code} – {x.Name}"
                    })
                    .ToListAsync();
            }
            else
            {
                vm.NextInspectionTypes = Enumerable.Empty<SelectListItem>();
            }
        }
    }
}
