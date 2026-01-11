using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Medical.Models
{
    public class MedicalBilan
    {
        [Key]
        public int Id { get; set; }

        // ===============================
        // Parent Medical Check
        // ===============================
        [Required]
        public int MedicalCheckId { get; set; }
        public MedicalCheck? MedicalCheck { get; set; }


        // ===============================
        // Snapshot of check date (SAFE)
        // ===============================
        [Required]
        public DateTime CheckDate { get; set; }

        // ===============================
        // Bilan Request
        // ===============================
        [Required]
        [StringLength(100)]
        [Display(Name = "Bilan Type")]
        public string BilanType { get; set; } = string.Empty;
        // e.g. Blood test, ECG, Cholesterol, Vision test

        [StringLength(500)]
        [Display(Name = "Doctor Instructions")]
        public string? Instructions { get; set; }
        // e.g. "Diet for 4 months then cholesterol test"

        // ===============================
        // Follow-up timing (Doctor logic)
        // ===============================
        [Display(Name = "Follow-up After (Months)")]
        public int? FollowUpMonths { get; set; }

        [Display(Name = "Follow-up After (Days)")]
        public int? FollowUpDays { get; set; }

        // ===============================
        // Completion tracking (non-medical)
        // ===============================
        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        // ===============================
        // Derived helper (NOT persisted)
        // ===============================
        [NotMapped]
        public DateTime? ExpectedReturnDate
        {
            get
            {
                var date = CheckDate;

                if (FollowUpMonths.HasValue)
                    date = date.AddMonths(FollowUpMonths.Value);

                if (FollowUpDays.HasValue)
                    date = date.AddDays(FollowUpDays.Value);

                return date;
            }
        }
    }
}
