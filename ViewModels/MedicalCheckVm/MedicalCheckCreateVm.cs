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

        [Required]
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
        // Doctor Decision
        // ============================

        [Required]
        [Display(Name = "Decision")]
        public MedicalDecision Decision { get; set; }     // FIT / UNFIT

        [StringLength(200)]
        [Display(Name = "Decision (Notes)")]
        public string? DecisionText { get; set; }

        [Display(Name = "Derogation")]
        public bool Derogation { get; set; }

        // ============================
        // Medical Flags (Monitoring)
        // ============================

        [Display(Name = "Obesity")]
        public bool? Obesite { get; set; }

        [Display(Name = "Optical Correction")]
        public bool? CorrectionOptique { get; set; }

        // ============================
        // Administrative / Compliance
        // ============================

        [StringLength(200)]
        [Display(Name = "Late Check Reason")]
        public string? LateCheckReason { get; set; }

        // ============================
        // Bilans (Complementary exams)
        // ============================

        public List<MedicalBilanCreateVm> Bilans { get; set; }
            = new List<MedicalBilanCreateVm>();
    }
}

