using FRAProject.Data;
using FRAProject.Enums;
using FRAProject.Models;
using FRAProject.Services.Medical;
using FRAProject.ViewModels.MedicalCheckVm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Controllers
{
    [Authorize(Roles = "Admin,CEMPN,MedicalAdmin")]
    public class MedicalDashboardController : Controller
    {
        private readonly FRAContext _context;
        private readonly IMedicalFitnessService _medicalFitnessService;

        public MedicalDashboardController(
            FRAContext context,
            IMedicalFitnessService medicalFitnessService)
        {
            _context = context;
            _medicalFitnessService = medicalFitnessService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var referenceDate = DateTime.Today;

            var crewMembers = await _context.CrewMembers
                .Include(c => c.Person)
                .Include(c => c.Squadron)
                .AsNoTracking()
                .ToListAsync();

            var rows = new List<MedicalCrewRowVm>();

            int fitCount = 0;
            int expiringCount = 0;
            int expiredCount = 0;

            int obesityCount = 0;
            int opticalCorrectionCount = 0;
            int withBilansCount = 0;

            foreach (var crew in crewMembers)
            {
                var fitness = await _medicalFitnessService
                    .EvaluateAsync(crew.Id, referenceDate);

                var status = fitness.IsExpired
                    ? MedicalFitnessStatus.Expired
                    : MedicalFitnessStatus.Fit;

                if (status == MedicalFitnessStatus.Expired)
                    expiredCount++;
                else if (fitness.RemainingDays <= 10)
                    expiringCount++;
                else
                    fitCount++;

                MedicalCheck? lastCheck = null;

                if (fitness.MedicalCheckId.HasValue)
                {
                    lastCheck = await _context.MedicalChecks
                        .Include(m => m.Bilans)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == fitness.MedicalCheckId.Value);
                }

                bool hasObesity = lastCheck?.Obesite ?? false;
                bool hasOpticalCorrection = lastCheck?.CorrectionOptique ?? false;
                bool hasOpenBilans = lastCheck?.Bilans.Any(b => !b.IsCompleted) ?? false;

                if (hasObesity) obesityCount++;
                if (hasOpticalCorrection) opticalCorrectionCount++;
                if (hasOpenBilans) withBilansCount++;

                rows.Add(new MedicalCrewRowVm
                {
                    CrewMemberId = crew.Id,
                    Name = crew.Person?.FullName ?? "—",
                    Squadron = crew.Squadron?.Name ?? "—",

                    LastCheckDate = lastCheck?.CheckDate,
                    CheckType = fitness.SourceCheckType,
                    Decision = fitness.Decision.ToString(),

                    RemainingDays = fitness.RemainingDays,
                    FitnessStatus = status,

                    // NEW (Expiry & Duration)
                    ExpiryDate = fitness.ExpiryDate,
                    DurationYears = lastCheck?.DurationYears ?? 0,
                    DurationMonths = lastCheck?.DurationMonths ?? 0,
                    DurationDays = lastCheck?.DurationDays ?? 0,


                    HasObesity = hasObesity,
                    HasOpticalCorrection = hasOpticalCorrection,
                    HasOpenBilans = hasOpenBilans
                });
            }

            rows = rows
                .OrderBy(r => r.RemainingDays)
                .ThenBy(r => r.Name)
                .ToList();

            var vm = new MedicalDashboardVm
            {
                FitCount = fitCount,
                ExpiringCount = expiringCount,
                ExpiredCount = expiredCount,

                ObesityCount = obesityCount,
                OpticalCorrectionCount = opticalCorrectionCount,
                WithBilansCount = withBilansCount,

                Crew = rows
            };

            return View(vm);
        }

    }
}
