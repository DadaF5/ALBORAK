using FRAProject.Data;
using FRAProject.Mapping;
using FRAProject.Models;
using FRAProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FRAProject.Controllers
{
    public class SortiesController : Controller
    {
        private readonly FRAContext _context;

        public SortiesController(FRAContext context)
        {
            _context = context;
        }

        // GET: Sorties/Create?odvId=123
        [HttpGet]
        public async Task<IActionResult> Create(int odvId)
        {
            var odv = await _context.Odvs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == odvId);

            if (odv == null)
                return NotFound();

            var vm = new SortieCreateVm
            {
                OdvId = odvId,
                Sequence = 1 // default, can be changed later
            };

            ViewBag.OdvInfo = $"{odv.MissionId} | {odv.OdvDate:yyyy-MM-dd}";
            await PopulateAcTypes();
            return View(vm);
        }

        // POST: Sorties/Create?odvId=123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SortieCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAcTypes();
                return View(model);
            }

            var sortie = new Sortie
            {
                OdvId = model.OdvId,
                SortieCode = model.SortieCode,
                Configuration = model.Configuration,
                Sequence = model.Sequence,
                AcTypeId = model.AcTypeId,
                Status = SortieStatus.Planned,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Sorties.Add(sortie);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "OdvPlanning",
                new { odvDate = DateTime.UtcNow.ToString("yyyy-MM-dd") }
            );
        }
        // GET: Sorties/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sortie = await _context.Sorties
                .Include(s => s.Odv)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sortie == null)
                return NotFound();


            var vm = new SortieCreateVm
            {
                Id = sortie.Id,              // 🔑 add Id to VM
                OdvId = sortie.OdvId,
                SortieCode = sortie.SortieCode,
                Configuration = sortie.Configuration,
                Sequence = sortie.Sequence,
                AcTypeId = sortie.AcTypeId,
                FuelQuantity = sortie.FuelQuantity
            };
            await PopulateAcTypes();
            return View(vm);
        }

        // POST: Sorties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SortieCreateVm model)
        {
            
                if (id != model.Id)
                return BadRequest();


            if (!ModelState.IsValid)
            {
                await PopulateAcTypes();
                return View(model);
            }

            var sortie = await _context.Sorties
                .FirstOrDefaultAsync(s => s.Id == model.Id);


            if (sortie == null)
                return NotFound();

            sortie.SortieCode = model.SortieCode;
            sortie.Configuration = model.Configuration;
            sortie.Sequence = model.Sequence;
            sortie.AcTypeId = model.AcTypeId;
            sortie.FuelQuantity = model.FuelQuantity;
            sortie.UpdatedAtUtc = DateTime.UtcNow;


            
            await _context.SaveChangesAsync();
            return RedirectToAction(
                "Index",
                "OdvPlanning"
               
            );
        }




        // Populate aircraft types for dropdowns
        private async Task PopulateAcTypes()
        {
            ViewBag.AcTypes = await _context.AcTypes
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();
        }
    }
}