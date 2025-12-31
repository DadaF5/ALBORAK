using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Enums;

namespace FRAProject.Models
{
    public class MedicalCheck
    {
        [Key]
        public int Id { get; set; }

        // ============================
        // Identity & Scope
        // ============================

        [Required]
        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        [Required]
        public int BaseId { get; set; }
        public Base Base { get; set; } = null!;

        // ============================
        // Medical Check Classification
        // ============================

        [Required]
        public MedicalCheckType CheckType { get; set; }
        // CEMPN / CONTROL / UNITE

        [Required]
        public DateTime CheckDate { get; set; }

        // ============================
        // Medical Decision & Aptitude
        // ============================

        [Required]
        public MedicalDecision Decision { get; set; }
        // FIT / UNFIT / FIT_WITH_RESTRICTIONS

        /// <summary>
        /// Free-text medical decision (official wording)
        /// </summary>
        [Column("Decision")]
        [StringLength(100)]
        [Display(Name = "Décision")]
        public string? DecisionText { get; set; }

        /// <summary>
        /// Medical derogation granted by authority
        /// </summary>
        public bool Derogation { get; set; } = false;

        // ============================
        // Regulatory Dates
        // ============================

        /// <summary>
        /// Next mandatory medical deadline
        /// </summary>
        public DateTime? NextDueDate { get; set; }

        /// <summary>
        /// Next visual / follow-up check
        /// </summary>
        public DateTime? NextVuDate { get; set; }

        // ============================
        // Late Check Governance
        // ============================

        /// <summary>
        /// Justification if CheckDate > NextDueDate
        /// (Training, mission, command authorization, etc.)
        /// </summary>
        [Column("LateCheckReason")]
        [StringLength(100)]
        [Display(Name = "Raison de Retard")]
        public string? LateCheckReason { get; set; }

        // ============================
        // Medical Indicators (Flags)
        // ============================

        [Column("OBESITE")]
        [Display(Name = "Obésité")]
        public bool? Obesite { get; set; }

        [Column("C_Optique")]
        [Display(Name = "Correction Optique")]
        public bool? CorrectionOptique { get; set; }

        // ============================
        // Audit & Authority
        // ============================

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }

        [StringLength(200)]
        public string? CreatedBy { get; set; }

        [StringLength(200)]
        public string? UpdatedBy { get; set; }

        // ============================
        // Concurrency
        // ============================

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation property for related Bilans
        public ICollection<MedicalBilan> Bilans { get; set; } = new List<MedicalBilan>();
    }
}
