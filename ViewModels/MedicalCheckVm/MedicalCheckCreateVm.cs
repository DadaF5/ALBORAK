using FRAProject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.MedicalCheckVm
{
    public class MedicalCheckCreateVm
    {
        // ============================
        // Context / Identity
        // ============================

        [Required]
        public int CrewMemberId { get; set; }

        // Display only (not posted back)
        public string CrewMemberName { get; set; } = "";

        public int BaseId { get; set; }

        // ============================
        // Medical Check Core
        // ============================

        [Required]
        [Display(Name = "Check Type")]
        public MedicalCheckType CheckType { get; set; }   // CEMPN / CONTROL / UNITE

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check Date")]
        public DateTime CheckDate { get; set; } = DateTime.Today;

        // ============================
        // Validity Duration
        [Range(0, 2)]
        [Display(Name = "Duration (Years)")]
        public int DurationYears { get; set; } = 0;

        [Range(0, 11)]
        [Display(Name = "Duration (Months)")]
        public int DurationMonths { get; set; } = 0;

        [Range(0, 30)]
        [Display(Name = "Duration (Days)")]
        public int DurationDays { get; set; } = 0;


        // ============================
        // Doctor Decision
        // ============================

        [Required]
        [Display(Name = "Decision")]
        public MedicalDecision Decision { get; set; }     // FIT / UNFIT

        [StringLength(200)]
        [Display(Name = "Decision (Notes)")]
        public string? DecisionText { get; set; }

        [Display(Name = "Derogation")]
        public bool Derogation { get; set; } = false;

        // ============================
        // Medical Flags (Monitoring)
        // ============================

        [Display(Name = "Obesity")]
        public bool Obesite { get; set; } = false;

        [Display(Name = "Optical Correction")]
        public bool CorrectionOptique { get; set; } = false;

        // ============================
        // Administrative / Compliance
        // ============================

        [StringLength(200)]
        [Display(Name = "Late Check Reason")]
        public string? LateCheckReason { get; set; }

        // ============================
        // Bilans (Complementary exams)
        // ============================
        public bool HasDuration =>
                DurationYears > 0 ||
                DurationMonths > 0 ||
                DurationDays > 0;
        public List<MedicalBilanCreateVm> Bilans { get; set; }
            = new List<MedicalBilanCreateVm>();
    }
}

