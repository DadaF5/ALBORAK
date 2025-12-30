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
                .OrderByDescending(m => m.CheckDate)
                .ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            if (lastCheck == null)
            {
                // No medical record = grounded
                return new MedicalFitnessResult
                {
                    Decision = MedicalDecision.UNFIT,
                    Validity = MedicalValidity.EXPIRED,
                    Notes = "No medical check on record"
                };
            }

            // 2️⃣ Doctor decision (authoritative)
            var decision = lastCheck.Decision?.ToUpper() == "FIT"
                ? MedicalDecision.FIT
                : MedicalDecision.UNFIT;

            // 3️⃣ Compute remaining days
            int? remainingDays = null;
            if (lastCheck.NextDueDate.HasValue)
            {
                remainingDays =
                    (lastCheck.NextDueDate.Value.Date - referenceDate.Date).Days;
            }

            // 4️⃣ Validity (system authority)
            var validity =
                remainingDays.HasValue && remainingDays.Value <= 0
                    ? MedicalValidity.EXPIRED
                    : MedicalValidity.VALID;

            // 5️⃣ Build result
            var result = new MedicalFitnessResult
            {
                MedicalCheckId = lastCheck.Id,
                CheckType = lastCheck.CheckType,
                CheckDate = lastCheck.CheckDate,
                NextDueDate = lastCheck.NextDueDate,
                NextVuDate = lastCheck.NextVuDate,
                RemainingDays = remainingDays,
                Decision = decision,
                Validity = validity,
                Notes = BuildNotes(lastCheck, remainingDays)
            };

            return result;
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

