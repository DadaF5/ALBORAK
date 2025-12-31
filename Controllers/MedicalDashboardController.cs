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

            // 1️⃣ Load all crew members (even those with no medical check)
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

            // 2️⃣ Evaluate medical fitness per crew member
            foreach (var crew in crewMembers)
            {
                var fitness = await _medicalFitnessService
                    .EvaluateAsync(crew.Id, referenceDate);

                // Map fitness → UI status
                MedicalFitnessStatus status;

                if (fitness.RemainingDays <= 0)
                {
                    status = MedicalFitnessStatus.Expired;
                    expiredCount++;
                }
                
                else
                {
                    status = MedicalFitnessStatus.Fit;
                    fitCount++;
                }

                // Flags from last medical check (if any)
                bool hasObesity = fitness.MedicalCheckId > 0 &&
                                  await _context.MedicalChecks
                                      .Where(m => m.Id == fitness.MedicalCheckId)
                                      .Select(m => m.Obesite == true)
                                      .FirstOrDefaultAsync();

                bool hasOpticalCorrection = fitness.MedicalCheckId > 0 &&
                                  await _context.MedicalChecks
                                      .Where(m => m.Id == fitness.MedicalCheckId)
                                      .Select(m => m.CorrectionOptique == true)
                                      .FirstOrDefaultAsync();

                bool hasOpenBilans = fitness.MedicalCheckId > 0 &&
                                  await _context.MedicalBilans
                                      .AnyAsync(b => b.MedicalCheckId == fitness.MedicalCheckId &&
                                                     b.IsCompleted == false);

                if (hasObesity) obesityCount++;
                if (hasOpticalCorrection) opticalCorrectionCount++;
                if (hasOpenBilans) withBilansCount++;

                rows.Add(new MedicalCrewRowVm
                {
                    CrewMemberId = crew.Id,
                    Name = crew.Person?.FullName ?? "—",
                    Squadron = crew.Squadron?.Name ?? "—",

                    LastCheckDate = fitness.CheckDate,
                    CheckType = fitness.CheckType,

                    Decision = fitness.Decision.ToString(),
                    RemainingDays = fitness.RemainingDays ?? 0,
                    FitnessStatus = status,

                    HasObesity = hasObesity,
                    HasOpticalCorrection = hasOpticalCorrection,
                    HasOpenBilans = hasOpenBilans
                });
            }

            // 3️⃣ Sort (expired first, then closest expiry)
            rows = rows
                .OrderBy(r => r.RemainingDays)
                .ThenBy(r => r.Name)
                .ToList();

            // 4️⃣ Build dashboard VM
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
