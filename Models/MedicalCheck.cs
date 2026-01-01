using FRAProject.Enums;
using FRAProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class MedicalCheck
    {
        // ============================
        // Identity & Scope
        // ============================

        [Key]
        public int Id { get; set; }

        [Required]
        public int CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } 

        [Required]
        public int BaseId { get; set; }

        // ============================
        // Check Classification
        // ============================

        [Required]
        public MedicalCheckType CheckType { get; set; }

        [Required]
        public DateTime CheckDate { get; set; }

        // ============================
        // Medical Decision (Doctor)
        // ============================

        /// <summary>
        /// Authoritative doctor decision (FIT / UNFIT).
        /// System may override to UNFIT if expired.
        /// </summary>
        [Required]
        public MedicalDecision Decision { get; set; }

        /// <summary>
        /// Optional free-text medical wording.
        /// </summary>
        [StringLength(200)]
        public string? DecisionText { get; set; }

        // ============================
        // Flags / Medical Conditions
        // ============================

        /// <summary>
        /// Derogation granted by medical authority.
        /// </summary>
        public bool Derogation { get; set; } = false;

        /// <summary>
        /// Obesity flagged during medical check.
        /// </summary>
        [Column("OBESITE")]
        public bool Obesite { get; set; } = false;

        /// <summary>
        /// Optical correction required.
        /// </summary>
        [Column("C_Optique")]
        public bool CorrectionOptique { get; set; } = false;

        // ============================
        // Validity Duration (Doctor-defined)
        // ============================

        /// <summary>
        /// Number of validity years (0–2).
        /// </summary>
        public int DurationYears { get; set; } = 0;

        /// <summary>
        /// Number of validity months (0–11).
        /// </summary>
        public int DurationMonths { get; set; } = 0;

        /// <summary>
        /// Number of validity days (0–30).
        /// </summary>
        public int DurationDays { get; set; } = 0;

        // ============================
        // Regulatory / Follow-up Dates
        // ============================

        /// <summary>
        /// Next regulatory due date (computed, not authoritative for fitness).
        /// </summary>
        public DateTime? NextDueDate { get; set; }

        /// <summary>
        /// Next unit-level visit date (computed).
        /// </summary>
        public DateTime? NextVuDate { get; set; }

        /// <summary>
        /// Required reason when a medical check is performed late.
        /// </summary>
        [StringLength(300)]
        public string? LateCheckReason { get; set; }

        // ============================
        // Related Bilans
        // ============================

        public ICollection<MedicalBilan> Bilans { get; set; } = new List<MedicalBilan>();

        // ============================
        // Audit & Concurrency
        // ============================

        [Required]
        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
