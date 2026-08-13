// Areas/AircraftMaintenance/Controllers/SnagsController.cs
using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.AircraftMaintenance.Services;
using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.AircraftMaintenance.Controllers
{
    [Area("AircraftMaintenance")]
    [Authorize(Roles = "Admin")]
    public class SnagsController : Controller
    {
        private readonly ISnagService _snagService;
        private readonly ISnagStatisticsService _statsService;
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public SnagsController(ISnagService snagService, ISnagStatisticsService statsService,
            IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _snagService = snagService;
            _statsService = statsService;
            _uow = uow;
            _userManager = userManager;
        }

        // GET /AircraftMaintenance/Snags?status=&aircraftId=
        public async Task<IActionResult> Index(SnagStatus? status, int? aircraftId)
        {
            var snags = await _uow.Snags.GetAllAsync(includeClosed: status == SnagStatus.CLOSED || status == null);

            if (status.HasValue) snags = snags.Where(s => s.Status == status.Value);
            if (aircraftId.HasValue) snags = snags.Where(s => s.AircraftId == aircraftId.Value);

            ViewBag.Aircrafts = (await _uow.Aircraft.GetAllAsync())
                .Select(a => new SelectListItem(a.Registration, a.Id.ToString()));
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedAircraftId = aircraftId;

            return View(snags.OrderByDescending(s => s.DiscoveryDate).ToList());
        }

        // GET /AircraftMaintenance/Snags/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var snag = await _uow.Snags.GetWithDetailsAsync(id);
            if (snag == null) return NotFound();
            return View(snag);
        }

        // --- REPORT ---

        // GET /AircraftMaintenance/Snags/Report
        public async Task<IActionResult> Report()
        {
            await PopulateDropdowns();
            return View(new SnagCreateDto { DiscoveryDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        // POST /AircraftMaintenance/Snags/Report
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(SnagCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(dto);
            }

            var userId = _userManager.GetUserId(User)!;
            var result = await _snagService.ReportAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await PopulateDropdowns();
                return View(dto);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.SnagId });
        }

        // --- FOLLOW-UP ---

        // GET /AircraftMaintenance/Snags/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var snag = await _uow.Snags.GetByIdAsync(id);
            if (snag == null) return NotFound();
            if (snag.Status == SnagStatus.CLOSED)
            {
                TempData["Error"] = "Ce snag est clôturé, il ne peut plus être modifié.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(new SnagUpdateDto { Severity = snag.Severity, Impact = snag.Impact, Description = snag.Description });
        }

        // POST /AircraftMaintenance/Snags/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SnagUpdateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _snagService.UpdateAsync(id, dto);
            if (!result.Success) ModelState.AddModelError("", result.Message);
            else TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /AircraftMaintenance/Snags/Defer/5
        public async Task<IActionResult> Defer(int id)
        {
            var snag = await _uow.Snags.GetByIdAsync(id);
            if (snag == null) return NotFound();
            ViewBag.SnagNumber = snag.SnagNumber;
            return View(new SnagDeferralDto());
        }

        // POST /AircraftMaintenance/Snags/Defer/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Defer(int id, SnagDeferralDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = _userManager.GetUserId(User)!;
            var result = await _snagService.DeferAsync(id, dto, userId);
            if (!result.Success) ModelState.AddModelError("", result.Message);
            else TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /AircraftMaintenance/Snags/LinkToWorkOrder — called from WorkOrder Create screen (AJAX or redirect)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkToWorkOrder(int snagId, int workOrderId)
        {
            var result = await _snagService.LinkToWorkOrderAsync(snagId, workOrderId);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        // --- CLOSE ---

        // POST /AircraftMaintenance/Snags/Close/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _snagService.CloseAsync(id, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // --- STATISTICS ---

        // GET /AircraftMaintenance/Snags/Statistics?from=&to=
        public async Task<IActionResult> Statistics(DateOnly? from, DateOnly? to)
        {
            var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-12));
            var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);

            var mtbf = await _statsService.GetMtbfByAtaAsync(fromDate, toDate);
            var topOffenders = await _statsService.GetTopOffendersAsync(fromDate, toDate, topN: 10);

            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            ViewBag.TopOffenders = topOffenders;

            return View(mtbf);
        }

        // SnagsController.cs — PopulateDropdowns(), corrected
        // SnagsController.cs — PopulateDropdowns(), corrected
        private async Task PopulateDropdowns()
        {
            var aircraft = await _uow.Aircraft.GetAllAsync();
            var acTypes = (await _uow.AcTypes.GetAllAsync()).ToDictionary(t => t.Id);

            ViewBag.Aircrafts = aircraft
                .OrderBy(a => acTypes.TryGetValue(a.AcTypeId, out var t) ? t.Code : "")
                .ThenBy(a => a.Registration)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Registration} — TailNo {a.TailNo}",
                    Group = new SelectListGroup
                    {
                        Name = acTypes.TryGetValue(a.AcTypeId, out var t) ? t.Code : "?"
                    }
                }).ToList();

            ViewBag.AtaChapters = (await _uow.Ata.GetAllAsync())
                .Select(a => new SelectListItem($"{a.Code} — {a.Name}", a.Id.ToString()));

            ViewBag.Bases = (await _uow.Bases.GetAllAsync())
                .Select(b => new SelectListItem(b.BaseName, b.Id.ToString()));
        }
    }
}