using FRAProject.Data;
using FRAProject.Models;
using FRAProject.Services.Medical;
using FRAProject.ViewModels.MedicalCheckVm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,CEMPN,MedicalAdmin")]
public class MedicalCheckController : Controller
{
    private readonly FRAContext _context;
    private readonly IMedicalFitnessService _medicalFitnessService;

    public MedicalCheckController(FRAContext context, IMedicalFitnessService medicalFitnessService)
    {
        _context = context;
        _medicalFitnessService = medicalFitnessService;
    }

    // ============================
    // CREATE (GET)
    // ============================
    [HttpGet]
    public async Task<IActionResult> Create(int crewMemberId)
    {
        var crewContext = await _context.CrewMembers
            .Where (c => c.Id == crewMemberId)
            .Select (c => new
            {
                c.Id,
                CrewMemberName = c.Person != null ? c.Person.FullName : "—",
                BaseId = c.Squadron
                        .Wing
                        .Base
                        .Id
            })
            .FirstOrDefaultAsync();
        if (crewContext == null)
            return NotFound();
        var vm = new MedicalCheckCreateVm
            {
            CrewMemberId = crewContext.Id,
            CrewMemberName = crewContext.CrewMemberName,
            BaseId = crewContext.BaseId,
            CheckDate = DateTime.Today
        }

        ;        

        return View(vm);
    }

    // ============================
    // CREATE (POST)
    // ============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicalCheckCreateVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // ============================
        // ⛔ Duration validation (MANDATORY)
        // ============================
        if (!model.HasDuration)
        {
            ModelState.AddModelError(
                string.Empty,
                "Medical validity duration must be specified."
            );
            return View(model);
        }

        // ============================
        // ⛔ Overlap prevention (±7 days, same type)
        // ============================
        var overlapExists = await _context.MedicalChecks.AnyAsync(m =>
            m.CrewMemberId == model.CrewMemberId &&
            m.CheckType == model.CheckType &&
            Math.Abs(EF.Functions.DateDiffDay(m.CheckDate, model.CheckDate)) <= 7
        );

        if (overlapExists)
        {
            ModelState.AddModelError(
                string.Empty,
                "A similar medical check already exists within the allowed period."
            );
            return View(model);
        }

        // ============================
        // ⛔ Late check justification (AUTHORITY-AWARE)
        // ============================
        var lastSameTypeCheck = await _context.MedicalChecks
            .Where(m =>
                m.CrewMemberId == model.CrewMemberId &&
                m.CheckType == model.CheckType)
            .OrderByDescending(m => m.CheckDate)
            .FirstOrDefaultAsync();

        if (lastSameTypeCheck != null)
        {
            // Expected date = last check + its OWN duration
            var expectedDate = lastSameTypeCheck.CheckDate
                .AddYears(lastSameTypeCheck.DurationYears)
                .AddMonths(lastSameTypeCheck.DurationMonths)
                .AddDays(lastSameTypeCheck.DurationDays);

            if (model.CheckDate.Date > expectedDate.Date &&
                string.IsNullOrWhiteSpace(model.LateCheckReason))
            {
                ModelState.AddModelError(
                    nameof(model.LateCheckReason),
                    "Late medical checks require a justification."
                );
                return View(model);
            }
        }

        // ============================
        // 🔑 Resolve BaseId (AUTHORITATIVE)
        // ============================
        var resolvedBaseId = await _context.CrewMembers
            .Where(c => c.Id == model.CrewMemberId)
            .Select(c => c.Squadron.Wing.Base.Id)
            .FirstAsync();

        // ============================
        // ✅ Create MedicalCheck
        // ============================
        var medicalCheck = new MedicalCheck
        {
            CrewMemberId = model.CrewMemberId,
            BaseId = resolvedBaseId,

            CheckType = model.CheckType,
            CheckDate = model.CheckDate,

            Decision = model.Decision,
            DecisionText = model.DecisionText,

            Derogation = model.Derogation,
            Obesite = model.Obesite,
            CorrectionOptique = model.CorrectionOptique,

            // 🔑 VALIDITY DURATION (CRITICAL)
            DurationYears = model.DurationYears,
            DurationMonths = model.DurationMonths,
            DurationDays = model.DurationDays,

            LateCheckReason = model.LateCheckReason,

            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = User.Identity?.Name
        };

        _context.MedicalChecks.Add(medicalCheck);
        await _context.SaveChangesAsync();

        // ============================
        // 🧪 Optional Bilans (NON-AUTHORITATIVE)
        // ============================
        if (model.Bilans != null && model.Bilans.Any())
        {
            foreach (var b in model.Bilans)
            {
                var bilan = new MedicalBilan
                {
                    MedicalCheckId = medicalCheck.Id,

                    // 🔑 snapshot of authority date
                    CheckDate = medicalCheck.CheckDate,

                    BilanType = b.BilanType,
                    Instructions = b.Instructions,
                    FollowUpMonths = b.FollowUpMonths,
                    FollowUpDays = b.FollowUpDays,

                    IsCompleted = false
                };

                _context.MedicalBilans.Add(bilan);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "MedicalDashboard");
    }


    // ============================
    // DETAILS (READ-ONLY)
    // ============================
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var check = await _context.MedicalChecks
            .Include(m => m.CrewMember)
                .ThenInclude(c => c.Person)
            .Include(m => m.Bilans)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (check == null)
            return NotFound();

        return View(check);
    }
}
