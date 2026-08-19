using FRAProject.Infrastructure.Interfaces;
using FRAProject.Models;
using FRAProject.Support.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize] // any logged-in user can report
public class BugReportsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public BugReportsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _env = env;
    }

    // GET: /BugReports  — list, visible to all, but only Admin sees triage controls
    //
    // Defaults to OPEN bugs only (NEW/CONFIRMED/IN_PROGRESS) so the list
    // doesn't fill up with resolved history over time. includeClosed
    // brings FIXED/WONT_FIX/DUPLICATE back into the unfiltered view —
    // explicitly picking a specific status (e.g. "Corrigé") always shows
    // that status regardless of the checkbox, since that's an explicit
    // ask, not the default "what needs attention" view.
    //
    // reportedBy is a free-text match against the reporter's display name
    // — deliberately not a user-picker dropdown, since the point is
    // catching things like "maybe this person is reporting normal
    // behavior as a bug", not precise identity lookup.
    private static readonly BugStatus[] ClosedStatuses =
    {
        BugStatus.FIXED, BugStatus.WONT_FIX, BugStatus.DUPLICATE
    };

    public async Task<IActionResult> Index(
        BugStatus? status,
        bool includeClosed = false,
        string? reportedBy = null,
        string? sortOrder = null)
    {
        var bugs = status.HasValue
            ? await _unitOfWork.BugReports.GetByStatusAsync(status.Value)
            : await _unitOfWork.BugReports.GetAllAsync();

        IEnumerable<BugReport> query = bugs;

        if (!status.HasValue && !includeClosed)
        {
            query = query.Where(b => !ClosedStatuses.Contains(b.Status));
        }

        if (!string.IsNullOrWhiteSpace(reportedBy))
        {
            var term = reportedBy.Trim();
            query = query.Where(b =>
                !string.IsNullOrEmpty(b.ReportedBy?.FullLabel) &&
                b.ReportedBy.FullLabel.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = query.Select(b => new BugReportDto
        {
            Id = b.Id,
            ReportNumber = b.ReportNumber,
            Title = b.Title,
            ScreenOrModule = b.ScreenOrModule,
            Severity = b.Severity,
            Status = b.Status,
            ReportedByName = b.ReportedBy?.FullLabel ?? "—",
            ReportedAt = b.ReportedAt
        });

        dtos = sortOrder switch
        {
            "date_asc" => dtos.OrderBy(b => b.ReportedAt),
            "severity_asc" => dtos.OrderBy(b => b.Severity),
            "severity_desc" => dtos.OrderByDescending(b => b.Severity),
            "status_asc" => dtos.OrderBy(b => b.Status),
            "status_desc" => dtos.OrderByDescending(b => b.Status),
            "reportedby_asc" => dtos.OrderBy(b => b.ReportedByName),
            "reportedby_desc" => dtos.OrderByDescending(b => b.ReportedByName),
            _ => dtos.OrderByDescending(b => b.ReportedAt) // "date_desc" and default
        };

        ViewBag.StatusFilter = status;
        ViewBag.IncludeClosed = includeClosed;
        ViewBag.ReportedBy = reportedBy;
        ViewBag.SortOrder = sortOrder;
        return View(dtos.ToList());
    }

    // GET: /BugReports/Create?returnUrl=...
    public IActionResult Create(string? returnUrl)
    {
        var dto = new BugReportCreateDto
        {
            PageUrl = returnUrl,
            UserIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            HttpMethod = "GET"
        };

        if (!string.IsNullOrEmpty(returnUrl))
        {
            var (controller, action) = TryResolveRoute(returnUrl);
            dto.ControllerName = controller;
            dto.ActionName = action;
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(dto);
    }

    // POST: /BugReports/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BugReportCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var userId = _userManager.GetUserId(User);
        string? screenshotPath = null;

        if (dto.Screenshot != null && dto.Screenshot.Length > 0)
        {
            screenshotPath = await SaveScreenshotAsync(dto.Screenshot);
        }

        var bug = new BugReport
        {
            ReportNumber = await _unitOfWork.BugReports.GenerateNextReportNumberAsync(),
            Title = dto.Title,
            Description = dto.Description,
            ExpectedBehavior = dto.ExpectedBehavior,
            ActualBehavior = dto.ActualBehavior,
            ScreenOrModule = dto.ScreenOrModule,
            Severity = dto.Severity,
            Status = BugStatus.NEW,
            ReportedByUserId = userId!,
            ReportedAt = DateTime.Now,
            ScreenshotPath = screenshotPath,
            UserIpAddress = dto.UserIpAddress,
            PageUrl = dto.PageUrl,
            ControllerName = dto.ControllerName,
            ActionName = dto.ActionName,
            UserAgent = dto.UserAgent,
            HttpMethod = dto.HttpMethod
        };

        await _unitOfWork.BugReports.AddAsync(bug);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = $"Signalement {bug.ReportNumber} enregistré. Merci !";
        return RedirectToAction(nameof(Index));
    }

    // GET: /BugReports/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var bug = await _unitOfWork.BugReports.GetByIdWithDetailsAsync(id);
        if (bug == null) return NotFound();

        var dto = new BugReportDetailsDto
        {
            Id = bug.Id,
            ReportNumber = bug.ReportNumber,
            Title = bug.Title,
            Description = bug.Description,
            ExpectedBehavior = bug.ExpectedBehavior,
            ActualBehavior = bug.ActualBehavior,
            ScreenOrModule = bug.ScreenOrModule,
            Severity = bug.Severity,
            Status = bug.Status,
            ReportedByName = bug.ReportedBy?.FullLabel ?? "—",
            ReportedAt = bug.ReportedAt,
            ScreenshotPath = bug.ScreenshotPath,
            AdminNotes = bug.AdminNotes,
            ResolvedAt = bug.ResolvedAt,
            ResolvedByName = bug.ResolvedBy?.FullLabel,
            PageUrl = bug.PageUrl,
            UserIpAddress = bug.UserIpAddress,
            ControllerName = bug.ControllerName,
            ActionName = bug.ActionName,
            UserAgent = bug.UserAgent,
            HttpMethod = bug.HttpMethod
        };

        return View(dto);
    }

    // POST: /BugReports/Triage — Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Triage(BugReportTriageDto dto)
    {
        var bug = await _unitOfWork.BugReports.GetByIdAsync(dto.Id);
        if (bug == null) return NotFound();

        bug.Status = dto.Status;
        bug.AdminNotes = dto.AdminNotes;

        if (dto.Status == BugStatus.FIXED)
        {
            bug.ResolvedAt = DateTime.Now;
            bug.ResolvedByUserId = _userManager.GetUserId(User);
        }

        _unitOfWork.BugReports.Update(bug);
        await _unitOfWork.CompleteAsync();

        TempData["Success"] = $"Statut mis à jour : {bug.ReportNumber}";
        return RedirectToAction(nameof(Details), new { id = bug.Id });
    }

    private async Task<string> SaveScreenshotAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "bugreports");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/bugreports/{fileName}";
    }

    private (string? Controller, string? Action) TryResolveRoute(string path)
    {
        try
        {
            var uri = path.StartsWith("http") ? new Uri(path).AbsolutePath : path;
            var segments = uri.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
            {
                var isArea = IsKnownArea(segments[0]);
                var controller = segments.Length >= 3 && isArea ? segments[1] : segments[0];
                var action = segments.Length >= 3 && isArea ? segments[2] : segments[1];
                return (controller, action);
            }
            return (segments.FirstOrDefault(), "Index");
        }
        catch
        {
            return (null, null);
        }
    }

    private bool IsKnownArea(string segment) =>
        new[] { "Settings", "AircraftMaintenance", "SquadronOps", "HR", "Healthcare" }.Contains(segment);
}