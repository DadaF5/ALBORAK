using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.ViewModels.AircraftMaintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class ProgramJobCardsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ProgramJobCardsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: AircraftMaintenance/ProgramJobCards/Manage/5  (5 = MaintenanceProgramId)
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Manage(int id)
        {
            var vm = await BuildManageVmAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: AircraftMaintenance/ProgramJobCards/BulkAssign
        // Assigns every JobCard of the program's own AcType whose CardCode
        // falls within [fromCode, toCode] (ordinal string comparison) and
        // isn't already assigned. This is the primary workflow — real PE
        // programs span dozens of cards (e.g. PE1: 1-001 to 1-085).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssign(int maintenanceProgramId, string fromCode, string toCode)
        {
            var program = await _uow.MaintenancePrograms.GetByIdAsync(maintenanceProgramId);
            if (program == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fromCode) || string.IsNullOrWhiteSpace(toCode))
            {
                TempData["Error"] = "Veuillez indiquer un code de début et un code de fin.";
                return RedirectToAction(nameof(Manage), new { id = maintenanceProgramId });
            }

            fromCode = fromCode.Trim().ToUpper();
            toCode = toCode.Trim().ToUpper();

            var allCards = await _uow.JobCards.GetAllWithDetailsAsync();
            var candidates = allCards
                .Where(jc => jc.AcTypeId == program.AcTypeId)
                .Where(jc => string.CompareOrdinal(jc.CardCode, fromCode) >= 0
                          && string.CompareOrdinal(jc.CardCode, toCode) <= 0)
                .ToList();

            if (!candidates.Any())
            {
                TempData["Error"] =
                    $"Aucune job card trouvée entre {fromCode} et {toCode} pour ce type d'aéronef. " +
                    "Vérifiez le format des codes (comparaison alphabétique — assurez-vous que les codes ont un format cohérent, ex: préfixes numériques de même longueur).";
                return RedirectToAction(nameof(Manage), new { id = maintenanceProgramId });
            }

            int added = 0, skipped = 0;
            foreach (var jc in candidates)
            {
                if (await _uow.ProgramJobCards.ExistsAsync(maintenanceProgramId, jc.Id))
                {
                    skipped++;
                    continue;
                }

                await _uow.ProgramJobCards.AddAsync(new ProgramJobCard
                {
                    MaintenanceProgramId = maintenanceProgramId,
                    JobCardId = jc.Id,
                    IsMandatory = true,
                    SortOrder = 100
                });
                added++;
            }

            await _uow.CompleteAsync();

            TempData["Success"] = skipped > 0
                ? $"{added} job card(s) assignée(s). {skipped} déjà présente(s), ignorée(s)."
                : $"{added} job card(s) assignée(s) avec succès.";

            return RedirectToAction(nameof(Manage), new { id = maintenanceProgramId });
        }

        // POST: AircraftMaintenance/ProgramJobCards/AddSingle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingle(int maintenanceProgramId, int jobCardId)
        {
            if (!await _uow.ProgramJobCards.ExistsAsync(maintenanceProgramId, jobCardId))
            {
                await _uow.ProgramJobCards.AddAsync(new ProgramJobCard
                {
                    MaintenanceProgramId = maintenanceProgramId,
                    JobCardId = jobCardId,
                    IsMandatory = true,
                    SortOrder = 100
                });
                await _uow.CompleteAsync();
                TempData["Success"] = "Job card ajoutée avec succès.";
            }
            else
            {
                TempData["Error"] = "Cette job card est déjà assignée à ce programme.";
            }

            return RedirectToAction(nameof(Manage), new { id = maintenanceProgramId });
        }

        // POST: AircraftMaintenance/ProgramJobCards/Remove/5  (5 = ProgramJobCard.Id)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var entity = await _uow.ProgramJobCards.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var programId = entity.MaintenanceProgramId;

            _uow.ProgramJobCards.Delete(entity);
            await _uow.CompleteAsync();

            TempData["Success"] = "Job card retirée du programme.";
            return RedirectToAction(nameof(Manage), new { id = programId });
        }

        // POST: AircraftMaintenance/ProgramJobCards/ToggleMandatory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMandatory(int id)
        {
            var entity = await _uow.ProgramJobCards.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.IsMandatory = !entity.IsMandatory;
            _uow.ProgramJobCards.Update(entity);
            await _uow.CompleteAsync();

            return RedirectToAction(nameof(Manage), new { id = entity.MaintenanceProgramId });
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task<ProgramJobCardManageViewModel?> BuildManageVmAsync(int maintenanceProgramId)
        {
            var program = await _uow.MaintenancePrograms.GetByIdWithDetailsAsync(maintenanceProgramId);
            if (program == null) return null;

            var assigned = await _uow.ProgramJobCards.GetByProgramIdWithDetailsAsync(maintenanceProgramId);
            var assignedJobCardIds = assigned.Select(x => x.JobCardId).ToHashSet();

            var allCardsForType = (await _uow.JobCards.GetAllWithDetailsAsync())
                .Where(jc => jc.AcTypeId == program.AcTypeId && jc.IsActive)
                .Where(jc => !assignedJobCardIds.Contains(jc.Id))
                .OrderBy(jc => jc.CardCode)
                .ToList();

            return new ProgramJobCardManageViewModel
            {
                MaintenanceProgramId = program.Id,
                ProgramCode = program.Code,
                ProgramName = program.Name,
                AcTypeLabel = program.AcType != null ? $"{program.AcType.Code} — {program.AcType.Name}" : "—",
                AssignedCards = assigned.Select(x => new ProgramJobCardItemViewModel
                {
                    Id = x.Id,
                    JobCardId = x.JobCardId,
                    CardCode = x.JobCard?.CardCode ?? "—",
                    Title = x.JobCard?.Title ?? "—",
                    IsMandatory = x.IsMandatory,
                    SortOrder = x.SortOrder
                }).ToList(),
                AvailableCards = allCardsForType.Select(jc => new JobCardLookupViewModel
                {
                    Id = jc.Id,
                    Code = jc.CardCode,
                    Title = jc.Title
                }).ToList()
            };
        }
    }
}