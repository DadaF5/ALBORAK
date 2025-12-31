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
            // 1️⃣ Get the most recent AUTHORITATIVE medical check
            // Priority: CEMPN > CONTROL > UNITE
            var lastCheck = await _context.MedicalChecks
                .Where(m => m.CrewMemberId == crewMemberId)
                .OrderByDescending(m =>
                    m.CheckType == MedicalCheckType.CEMPN ? 3 :
                    m.CheckType == MedicalCheckType.CONTROL ? 2 :
                    m.CheckType == MedicalCheckType.UNITE ? 1 : 0)
                .ThenByDescending(m => m.CheckDate)
                .ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            // ❌ No medical check → grounded
            if (lastCheck == null)
            {
                return new MedicalFitnessResult
                {
                    Decision = MedicalDecision.UNFIT,
                    Validity = MedicalValidity.EXPIRED,
                    Notes = "No medical check on record"
                };
            }

            // 2️⃣ Doctor decision (authoritative)
            bool isFitByDecision = lastCheck.Decision == MedicalDecision.FIT;

            // 3️⃣ Compute remaining days (system authority)
            int? remainingDays = null;
            if (lastCheck.NextDueDate.HasValue)
            {
                remainingDays =
                    (lastCheck.NextDueDate.Value.Date - referenceDate.Date).Days;
            }

            // 4️⃣ Validity (system authority)
            MedicalValidity validity;

            if (!remainingDays.HasValue || remainingDays.Value <= 0)
            {
                // Date overdue ALWAYS grounds the crew
                validity = MedicalValidity.EXPIRED;
            }
            else
            {
                validity = MedicalValidity.VALID;
            }

            // 5️⃣ Final decision resolution
            // EXPIRED overrides doctor decision
            var finalDecision =
                validity == MedicalValidity.EXPIRED
                    ? MedicalDecision.UNFIT
                    : lastCheck.Decision;

            // 6️⃣ Build result
            return new MedicalFitnessResult
            {
                MedicalCheckId = lastCheck.Id,
                CheckType = lastCheck.CheckType,
                CheckDate = lastCheck.CheckDate,
                NextDueDate = lastCheck.NextDueDate,
                NextVuDate = lastCheck.NextVuDate,
                RemainingDays = remainingDays,

                Decision = finalDecision,
                Validity = validity,

                Notes = BuildNotes(lastCheck, remainingDays)
            };
            
        }

        // ================================
        // Helper: Notes builder
        // ================================
        private string BuildNotes(MedicalCheck check, int? remainingDays)
        {
            var sb = new StringBuilder();

            if (check.CorrectionOptique == true)
                sb.Append("Optical correction required. ");

            if (check.Obesite == true)
                sb.Append("Obesity flagged. ");

            if (check.Derogation == true)
                sb.Append("Derogation applied. ");

            if (remainingDays.HasValue && remainingDays.Value <= 0)
                sb.Append("Medical check expired.");

            return sb.ToString().Trim();
        }
    }
}

