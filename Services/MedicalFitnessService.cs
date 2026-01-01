using FRAProject.Data;
using FRAProject.Enums;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRAProject.Services.Medical
{
    public class MedicalFitnessService : IMedicalFitnessService
    {
        private readonly FRAContext _context;

        public MedicalFitnessService(FRAContext context)
        {
            _context = context;
        }

        public async Task<MedicalFitnessResult> EvaluateAsync(
    int crewMemberId,
    DateTime referenceDate)
        {
            // 1️⃣ Select last authoritative medical check
            var check = await _context.MedicalChecks
                .Where(m => m.CrewMemberId == crewMemberId)
                .OrderByDescending(m =>
                    m.CheckType == MedicalCheckType.CEMPN ? 3 :
                    m.CheckType == MedicalCheckType.CONTROL ? 2 :
                    m.CheckType == MedicalCheckType.UNITE ? 1 : 0)
                .ThenByDescending(m => m.CheckDate)
                .FirstOrDefaultAsync();

            // 2️⃣ No medical check = grounded
            if (check == null)
            {
                return new MedicalFitnessResult
                {
                    Decision = MedicalDecision.UNFIT,
                    Validity = MedicalValidity.EXPIRED,
                    RemainingDays = 0,
                    ExpiryDate = null,
                    SourceCheckType = null,
                    MedicalCheckId = null
                };
            }

            // 3️⃣ Compute expiry date using Duration
            var expiryDate = check.CheckDate
                .AddYears(check.DurationYears)
                .AddMonths(check.DurationMonths)
                .AddDays(check.DurationDays);

            // 4️⃣ Remaining days (date-only logic)
            int remainingDays = (expiryDate.Date - referenceDate.Date).Days;

            // 5️⃣ Validity (system authority)
            var validity = remainingDays > 0
                ? MedicalValidity.VALID
                : MedicalValidity.EXPIRED;

            // 6️⃣ Final decision
            // Expired ALWAYS grounds the crew member
            var finalDecision = validity == MedicalValidity.EXPIRED
                ? MedicalDecision.UNFIT
                : check.Decision;

            return new MedicalFitnessResult
            {
                Decision = finalDecision,
                Validity = validity,
                RemainingDays = remainingDays,
                ExpiryDate = expiryDate,
                SourceCheckType = check.CheckType,
                MedicalCheckId = check.Id
            };
        }


    }
}

