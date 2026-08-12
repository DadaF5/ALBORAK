using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class WorkOrderSectionSignOffsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public WorkOrderSectionSignOffsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/WorkOrderSectionSignOffs/Index/5  (5 = WorkOrderSectionId)
        public async Task<IActionResult> Index(int id)
        {
            var section = await _uow.WorkOrderSections.GetByIdWithDetailsAsync(id);
            if (section == null) return NotFound();

            var signOffs = await _uow.WorkOrderSectionSignOffs.GetOrCreateCanonicalAsync(id);
            var labelByLevel = FRAProject.Areas.AircraftMaintenance.Models.WorkOrderSectionSignOff.CanonicalLevels
                .ToDictionary(l => l.Level, l => l.Label);

            var vm = new WorkOrderSectionSignOffPageViewModel
            {
                WorkOrderSectionId = id,
                SectionLabel = $"{section.WorkSection?.Code} — {section.WorkSection?.Name}",
                FormNumber = section.FormNumber,
                SignOffs = signOffs.Select(x => new WorkOrderSectionSignOffItemViewModel
                {
                    Id = x.Id,
                    Level = x.Level,
                    LevelLabel = labelByLevel.GetValueOrDefault(x.Level, x.Level),
                    SortOrder = x.SortOrder,
                    SignedByName = x.SignedByName,
                    StampReference = x.StampReference,
                    SignedAtUtc = x.SignedAtUtc,
                    Remarks = x.Remarks
                }).ToList()
            };

            return View(vm);
        }

        // POST: AircraftMaintenance/WorkOrderSectionSignOffs/Sign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign(int signOffId, string signedByName, string? stampReference, string? remarks)
        {
            var entity = await _uow.WorkOrderSectionSignOffs.GetByIdAsync(signOffId);
            if (entity == null) return NotFound();

            if (string.IsNullOrWhiteSpace(signedByName))
            {
                TempData["Error"] = "Le nom du signataire est requis.";
                return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
            }

            entity.SignedByName = signedByName.Trim();
            entity.StampReference = stampReference;
            entity.Remarks = remarks;
            entity.SignedAtUtc = DateTime.UtcNow;

            _uow.WorkOrderSectionSignOffs.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Visa enregistré avec succès.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }

        // POST: AircraftMaintenance/WorkOrderSectionSignOffs/Unsign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsign(int signOffId)
        {
            var entity = await _uow.WorkOrderSectionSignOffs.GetByIdAsync(signOffId);
            if (entity == null) return NotFound();

            entity.SignedAtUtc = null;

            _uow.WorkOrderSectionSignOffs.Update(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Visa annulé.";
            return RedirectToAction(nameof(Index), new { id = entity.WorkOrderSectionId });
        }
    }
}