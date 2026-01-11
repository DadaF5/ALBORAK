using FRAProject.Enums;
using FRAProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    /// <summary>
    /// DOMAIN: Medical Care Center
    /// Represents a medical examination/check for a crew member.
    /// Educational Purpose: Critical entity that determines if a crew member is medically FIT to fly.
    /// 
    /// Key Relationships:
    /// - MedicalCheck → CrewMember (Many-to-One): Links medical records to operational crew
    /// - MedicalCheck → Base (Many-to-One): Where the medical check was performed
    /// - MedicalCheck → MedicalBilans (One-to-Many): Detailed examination results (lab, physical, etc.)
    /// 
    /// Decision Workflow:
    /// 1. Flight surgeon performs examination and creates MedicalCheck record
    /// 2. Decision (FIT/FIT_RESTRICTIONS/UNFIT) determines operational status
    /// 3. Validity period (DurationYears/Months/Days) determines when next check is due
    /// 4. MedicalFitnessService evaluates most recent check to determine current flight status
    /// 5. Only FIT or FIT_RESTRICTIONS crew members can be assigned to sorties
    /// 
    /// This demonstrates how Medical Care Center domain gates access to Squadron Operations,
    /// ensuring flight safety and regulatory compliance.
    /// </summary>
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
