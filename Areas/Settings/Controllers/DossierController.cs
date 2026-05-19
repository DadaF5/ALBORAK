using FRAProject.Areas.Settings.ViewModels;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRAProject.Areas.Settings.Controllers
{
    /// <summary>
    /// Thin controller — routes requests and returns views.
    /// All business logic lives in IDossierService.
    /// All file I/O lives in IFileUploadService (via IDossierService).
    ///
    /// Single responsibility: HTTP in → service call → HTTP out.
    /// </summary>
    [Area("Settings")]
    [Authorize(Roles = "Admin")]
    public class DossierController : Controller
    {
        private readonly IDossierService    _dossierService;
        private readonly IUnitOfWork        _uow;

        public DossierController(IDossierService dossierService, IUnitOfWork uow)
        {
            _dossierService = dossierService;
            _uow            = uow;
        }

        // ════════════════════════════════════════════════════════════════
        //  INDEX
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
            string? searchNumber  = null,
            string? searchImmat   = null,
            string? searchStatus  = null,
            string  sortColumn    = "CreatedAt",
            string  sortDirection = "desc",
            int     pageNumber    = 1,
            int     pageSize      = 10)
        {
            var vm = await _dossierService.BuildIndexVmAsync(
                searchNumber, searchImmat, searchStatus,
                sortColumn, sortDirection, pageNumber, pageSize);

            return View(vm);
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 1 — GET (new dossier)
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Create()
        {
            var dto = await _dossierService.BuildStep1DtoAsync();
            return View("Step1", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 1 — GET (back navigation — existing dossier)
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Step1(int id)
        {
            var dossier = await _dossierService.GetEditableDossierAsync(id);
            if (dossier == null) return NotFound();

            var dto = await _dossierService.BuildStep1DtoAsync(id);
            return View("Step1", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 1 — POST
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStep1(DossierStep1Dto dto)
        {
            if (!ModelState.IsValid)
            {
                await _dossierService.RepopulateStep1Async(dto);
                return View("Step1", dto);
            }

            var dossier = await _dossierService
                .SaveStep1Async(dto, User.Identity?.Name);

            return RedirectToAction(nameof(Step2), new { id = dossier.Id });
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 2 — GET
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Step2(int id)
        {
            var dossier = await _dossierService.GetEditableDossierAsync(id);
            if (dossier == null) return NotFound();

            var dto = await _dossierService.BuildStep2DtoAsync(id);
            ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
            return View("Step2", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 2 — POST
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStep2(int id, DossierStep2Dto dto)
        {
            dto.DossierId = id;

            // Uniqueness check on ImmatriculationSuffix
            if (ModelState.IsValid &&
                !string.IsNullOrWhiteSpace(dto.ImmatriculationSuffix))
            {
                var suffix = dto.ImmatriculationSuffix.Trim().ToUpper();
                var exists = await _uow.DossierAircrafts.AnyAsync(a =>
                    a.ImmatriculationSuffix == suffix && a.DossierId != id);

                if (exists)
                    ModelState.AddModelError(
                        nameof(dto.ImmatriculationSuffix),
                        $"L'immatriculation 'CN-{suffix}' est deja utilisee.");
            }

            if (!ModelState.IsValid)
            {
                await _dossierService.RepopulateStep2Async(dto);
                ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
                return View("Step2", dto);
            }

            await _dossierService.SaveStep2Async(id, dto);
            return RedirectToAction(nameof(Step3), new { id });
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 3 — GET
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Step3(int id)
        {
            var dossier = await _dossierService.GetEditableDossierAsync(id);
            if (dossier == null) return NotFound();

            var dto = await _dossierService.BuildStep3DtoAsync(id);
            ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
            return View("Step3", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 3 — POST
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStep3(int id, DossierStep3Dto dto)
        {
            dto.DossierId = id;

            // Conditional validation — enforced here since annotations
            // cannot express field-conditional rules
            if (dto.HasAirworthinessDoc && !dto.CdnDocTypeId.HasValue)
                ModelState.AddModelError(nameof(dto.CdnDocTypeId),
                    "Le type de document est obligatoire.");

            if (dto.WasForeignRegistered && !dto.ForeignCountryId.HasValue)
                ModelState.AddModelError(nameof(dto.ForeignCountryId),
                    "L'etat d'origine est obligatoire.");

            if (!ModelState.IsValid)
            {
                await _dossierService.RepopulateStep3Async(dto);
                ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
                return View("Step3", dto);
            }

            await _dossierService.SaveStep3Async(id, dto);
            return RedirectToAction(nameof(Step4), new { id });
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 4 — GET
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Step4(int id)
        {
            var dossier = await _dossierService.GetEditableDossierAsync(id);
            if (dossier == null) return NotFound();

            var dto = await _dossierService.BuildStep4DtoAsync(id);
            ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
            return View("Step4", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  UPLOAD DOCUMENT — AJAX POST
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(
            int id, int documentTypeId, IFormFile file)
        {
            var (success, error, payload) = await _dossierService
                .UploadDocumentAsync(id, documentTypeId, file,
                    User.Identity?.Name);

            if (!success)
                return Json(new { success = false, message = error });

            return Json(new { success = true, data = payload });
        }

        // ════════════════════════════════════════════════════════════════
        //  DELETE DOCUMENT — AJAX POST
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id, int documentId)
        {
            var success = await _dossierService.DeleteDocumentAsync(id, documentId);

            return Json(success
                ? new { success = true }
                : new { success = false, message = "Document introuvable." });
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 4 — POST (advance to Step 5)
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStep4(int id)
        {
            var allUploaded = await _dossierService.AllRequiredDocsUploadedAsync(id);

            if (!allUploaded)
            {
                var dto = await _dossierService.BuildStep4DtoAsync(id);
                ModelState.AddModelError(string.Empty,
                    "Veuillez telecharger tous les documents obligatoires avant de continuer.");
                ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
                return View("Step4", dto);
            }

            await _dossierService.SaveStep4AdvanceAsync(id);
            return RedirectToAction(nameof(Step5), new { id });
        }

        // ════════════════════════════════════════════════════════════════
        //  STEP 5 — GET
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Step5(int id)
        {
            var dossier = await _dossierService.GetEditableDossierAsync(id);
            if (dossier == null) return NotFound();

            var dto = await _dossierService.BuildStep5DtoAsync(id);
            ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
            return View("Step5", dto);
        }

        // ════════════════════════════════════════════════════════════════
        //  SUBMIT — POST (FIXED)
        //
        //  FIX: AttestationConfirmed validated manually here instead of
        //  relying on [Range(typeof(bool),"true","true")] annotation.
        //
        //  Root cause: ASP.NET Core renders asp-for checkbox as:
        //    <input type="checkbox" name="AttestationConfirmed" value="true" />
        //    <input type="hidden"   name="AttestationConfirmed" value="false" />
        //  The hidden input always posts "false". The Range validator
        //  reads this and fails even when the checkbox is checked.
        //
        //  Manual check reads the actual bound bool value — reliable.
        // ════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, DossierStep5Dto dto)
        {
            dto.DossierId = id;

            // FIX: manual check replaces [Range(typeof(bool),"true","true")]
            if (!dto.AttestationConfirmed)
                ModelState.AddModelError(
                    nameof(dto.AttestationConfirmed),
                    "Vous devez confirmer l'attestation avant de soumettre.");

            // Check all required documents uploaded
            var allUploaded = await _dossierService.AllRequiredDocsUploadedAsync(id);
            if (!allUploaded)
                ModelState.AddModelError(string.Empty,
                    "Des documents obligatoires sont manquants.");

            if (!ModelState.IsValid)
            {
                var rebuilt = await _dossierService.BuildStep5DtoAsync(id);
                rebuilt.AttestationCity = dto.AttestationCity;
                rebuilt.AttestationDate = dto.AttestationDate;
                rebuilt.SignatoryName = dto.SignatoryName;
                rebuilt.AttestationConfirmed = dto.AttestationConfirmed;
                ViewBag.Progress = await _dossierService.GetProgressVmAsync(id);
                return View("Step5", rebuilt);
            }

            var dossierNumber = await _dossierService.SubmitAsync(id, dto);

            TempData["SuccessMessage"] =
                $"Dossier {dossierNumber} soumis avec succes a la DAM.";

            return RedirectToAction(nameof(Confirmation), new { id });
        }

        // ════════════════════════════════════════════════════════════════
        //  CONFIRMATION
        // ════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Confirmation(int id)
        {
            var dossier  = await _uow.Dossiers.GetByIdAsync(id);
            var aircraft = await _uow.DossierAircrafts.GetByIdAsync(id);

            ViewBag.DossierNumber       = dossier?.DossierNumber;
            ViewBag.FullImmatriculation = aircraft?.FullImmatriculation;
            return View();
        }

        // ════════════════════════════════════════════════════════════════
        //  AJAX — CASCADING DROPDOWNS (FIXED)
        //
        //  GetAcTypes was returning Array.Empty — TODO comment never done.
        //  Now queries _uow.AcTypes via AcMainGroup → AcCategory chain.
        // ════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetAcTypes(int? categoryId)
        {
            if (!categoryId.HasValue)
                return Json(Array.Empty<object>());

            // AcType → AcMainGroup → AcCategory (2-hop)
            // Step 1: get AcMainGroup Ids for this category
            var groups = await _uow.AcMainGroups.GetWhereAsync(g =>
                g.IsActive && g.AcCategoryId == categoryId.Value);

            var groupIds = groups.Select(g => g.Id).ToHashSet();

            // Step 2: get AcTypes belonging to those groups
            var types = await _uow.AcTypes.GetWhereAsync(t =>
                t.IsActive && groupIds.Contains(t.AcMainGroupId));

            return Json(types
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .Select(t => new { value = t.Id, text = t.DisplayLabel }));
        }

        [HttpGet]
        public async Task<IActionResult> GetVersions(int? acTypeId)
        {
            if (!acTypeId.HasValue)
                return Json(Array.Empty<object>());

            // FIX: filter by AcTypeId — was returning ALL versions
            var versions = await _uow.AircraftVersions
                .GetWhereAsync(v => v.IsActive && v.AcTypeId == acTypeId.Value);

            return Json(versions
                .OrderBy(v => v.SortOrder)
                .ThenBy(v => v.Name)
                .Select(v => new { value = v.Id, text = v.Name }));
        }

        [HttpGet]
        public async Task<IActionResult> GetMissionRoles(int? categoryId)
        {
            var roles = await _uow.MissionRoles.GetWhereAsync(r =>
                r.IsActive &&
                (!categoryId.HasValue ||
                 r.AcCategoryId == null ||
                 r.AcCategoryId == categoryId));

            return Json(roles
                .OrderBy(r => r.SortOrder)
                .Select(r => new { value = r.Id, text = r.Name }));
        }
    }
}
