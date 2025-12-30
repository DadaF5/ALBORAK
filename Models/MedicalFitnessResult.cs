using FRAProject.Enums;

namespace FRAProject.Models
{
    public class MedicalFitnessResult
    {
        // Doctor decision
        public MedicalDecision Decision { get; set; }

        // System-calculated validity
        public MedicalValidity Validity { get; set; }

        // Convenience flags
        public bool IsFit => Decision == MedicalDecision.FIT;
        public bool IsExpired => Validity == MedicalValidity.EXPIRED;

        // Dates
        public DateTime? CheckDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public DateTime? NextVuDate { get; set; }

        // Remaining days
        public int? RemainingDays { get; set; }

        public string Notes { get; set; } = "";

        // Source info
        public int MedicalCheckId { get; set; }
        public MedicalCheckType CheckType { get; set; }
    }

}
