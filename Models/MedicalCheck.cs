using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    
    public class MedicalCheck
    {
        [Key]
        [Column("MedCheckID")]
        public int MedCheckID { get; set; }

        // Foreign Key to CrewMember (Parent)
        
        [Required]
        [Display(Name = "Crew Member")]
        public int CrewMemberId { get; set; }
        public CrewMember? CrewMember { get; set; }

        [Required]
        [Column("MedCheckType")]
        [StringLength(30)]
        [Display(Name = "Type de Visite")]
        public string MedCheckType { get; set; } = string.Empty; // CEMPN, CONTROL, VISITE A L'UNITE

        [Column("CheckDate")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Visite")]
        public DateTime? CheckDate { get; set; }

        [Column("DaysValid")]
        [Display(Name = "Jours Valides")]
        public int? DaysValid { get; set; }

        [Column("Obs")]
        [StringLength(200)]
        [Display(Name = "Observations")]
        public string? Obs { get; set; }

        [Column("Decision")]
        [StringLength(100)]
        [Display(Name = "Décision")]
        public string? Decision { get; set; }

        [Column("NextDueDate")]
        [DataType(DataType.Date)]
        [Display(Name = "Prochaine Échéance")]
        public DateTime? NextDueDate { get; set; }

        [Column("Speciality")]
        [StringLength(10)]
        [Display(Name = "Spécialité")]
        public string? Speciality { get; set; } // PH, PC, MN, CCA

        [Column("Constatations")]
        [Display(Name = "Constatations")]
        public string? Constatations { get; set; }

        [Column("OBESITE")]
        [Display(Name = "Obésité")]
        public bool? OBESITE { get; set; }

        [Column("C_Optique")]
        [Display(Name = "Correction Optique")]
        public bool? C_Optique { get; set; }

        [Column("Aptitude")]
        [StringLength(30)]
        [Display(Name = "Aptitude")]
        public string? Aptitude { get; set; } // APTE, APTE PAR DEROGATION, INAPTE

        [Column("Next_VU_Date")]
        [DataType(DataType.Date)]
        [Display(Name = "Prochaine Visite Unité")]
        public DateTime? Next_VU_Date { get; set; }

        [Column("VU_Date")]
        [DataType(DataType.Date)]
        [Display(Name = "Date Visite Unité")]
        public DateTime? VU_Date { get; set; }

        [Column("LateCheckReason")]
        [StringLength(100)]
        [Display(Name = "Raison de Retard")]
        public string? LateCheckReason { get; set; }

        [Column("CaptainType")]
        [StringLength(20)]
        [Display(Name = "Type de Personnel")]
        public string? CaptainType { get; set; } // PILOT, CONTROLLER, DRIVER

        [Column("vu_LateCheckReason")]
        [StringLength(100)]
        [Display(Name = "Raison Retard VU")]
        public string? VuLateCheckReason { get; set; }

        // =============================================
        // RELATIONSHIPS
        // =============================================
                

        // 2. Children Relationship: One MedicalCheck has Many MedicalBilans
        public ICollection<MedicalBilan> MedicalBilans { get; set; } = new List<MedicalBilan>();

        // =============================================
        // HELPER PROPERTIES (Not in Database)
        // =============================================

        [NotMapped]
        public int? D_ToGo => NextDueDate.HasValue ?
            (int?)(NextDueDate.Value - DateTime.Today).Days : null;

        [NotMapped]
        public int? D_ToGo_VU => Next_VU_Date.HasValue ?
            (int?)(Next_VU_Date.Value - DateTime.Today).Days : null;

        [NotMapped]
        public string NextDueDateFormatted => NextDueDate?.ToString("dd-MMM-yy") ?? string.Empty;

        [NotMapped]
        public string Next_VU_DateFormatted => Next_VU_Date?.ToString("dd-MMM-yy") ?? string.Empty;

        [NotMapped]
        public string CheckDateFormatted => CheckDate?.ToString("dd-MMM-yy") ?? string.Empty;

        [NotMapped]
        public bool IsExpired => NextDueDate.HasValue && NextDueDate.Value < DateTime.Today;

        [NotMapped]
        public bool IsVU_Overdue => Next_VU_Date.HasValue && Next_VU_Date.Value < DateTime.Today;

        [NotMapped]
        public string Status => IsExpired ? "EXPIRÉ" :
                               D_ToGo <= 30 ? "À RENOUVELER" : "VALIDE";
    }
}