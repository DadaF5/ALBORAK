using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.MedicalCheckVm
{
    public class MedicalBilanCreateVm
    {
        // Parent Medical Check
        [Required]
        public int MedicalCheckId { get; set; }

        // ===============================
        // What examination is required
        // ===============================
        [Required]
        [StringLength(100)]
        [Display(Name = "Bilan Type")]
        public string BilanType { get; set; } = string.Empty;

        // ===============================
        // Follow-up timing (Doctor language)
        // ===============================
        [Range(0, 24)]
        [Display(Name = "Follow-up (Months)")]
        public int? FollowUpMonths { get; set; }

        [Range(0, 90)]
        [Display(Name = "Follow-up (Days)")]
        public int? FollowUpDays { get; set; }

        // ===============================
        // Doctor instructions
        // ===============================
        [StringLength(300)]
        [Display(Name = "Doctor Instructions")]
        public string? Instructions { get; set; }
    }
}
