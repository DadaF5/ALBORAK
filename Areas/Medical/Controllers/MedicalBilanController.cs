using FRAProject.Areas.Medical.Models;
using FRAProject.Data;
using FRAProject.Models;
using FRAProject.ViewModels.MedicalCheckVm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FRAProject.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class MedicalBilanController : Controller
    {
        private readonly FRAContext _context;

        public MedicalBilanController(FRAContext context)
        {
            _context = context;
        }

        // ===============================
        // GET: Create Bilan
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Create(int medicalCheckId)
        {
            var medicalCheck = await _context.MedicalChecks
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == medicalCheckId);

            if (medicalCheck == null)
                return NotFound();

            var vm = new MedicalBilanCreateVm
            {
                MedicalCheckId = medicalCheckId
            };

            return View(vm);
        }

        // ===============================
        // POST: Create Bilan
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalBilanCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var medicalCheck = await _context.MedicalChecks
                .FirstOrDefaultAsync(m => m.Id == vm.MedicalCheckId);

            if (medicalCheck == null)
                return NotFound();

            // 🔒 Safety rule: at least one timing must be provided
            if (!vm.FollowUpMonths.HasValue && !vm.FollowUpDays.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    "You must specify a follow-up delay (months or days)."
                );
                return View(vm);
            }

            var bilan = new MedicalBilan
            {
                MedicalCheckId = medicalCheck.Id,
                CheckDate = medicalCheck.CheckDate, // snapshot
                BilanType = vm.BilanType,
                Instructions = vm.Instructions,
                FollowUpMonths = vm.FollowUpMonths,
                FollowUpDays = vm.FollowUpDays,
                IsCompleted = false
            };

            _context.MedicalBilans.Add(bilan);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "MedicalCheck",
                new { id = medicalCheck.Id }
            );
        }
    }
}

