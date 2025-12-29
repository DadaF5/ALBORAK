using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    
    public class MedicalBilan
    {
        [Key]
        [Column("BilanID")]
        public int BilanID { get; set; }

        // Foreign Key to MedicalCheck (Parent)
        [Required]
        [Display(Name = "Medical Check")]
        public int MedicalCheckId { get; set; }

        [Required]
        [Column("BilanType")]
        [StringLength(100)]
        [Display(Name = "Type de Bilan")]
        public string BilanType { get; set; } = string.Empty; // Blood Test, X-Ray, etc.

        [Column("BilanDetails")]
        [StringLength(500)]
        [Display(Name = "Détails")]
        public string? Details { get; set; }

        [Column("DurationMonths")]
        [Display(Name = "Durée (Mois)")]
        public int DurationMonths { get; set; }

        [Column("DurationDays")]
        [Display(Name = "Durée (Jours)")]
        public int DurationDays { get; set; }

        [Column("RequiredDate")]
        [DataType(DataType.Date)]
        [Display(Name = "Date Requise")]
        public DateTime? RequiredDate { get; set; }

        [Column("IsCompleted")]
        [Display(Name = "Complété")]
        public bool IsCompleted { get; set; } = false;

        [Column("CompletedDate")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Complétion")]
        public DateTime? CompletedDate { get; set; }

        [Column("Result")]
        [StringLength(500)]
        [Display(Name = "Résultat")]
        public string? Result { get; set; }

        [Column("Remarks")]
        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }

        // =============================================
        // RELATIONSHIPS
        // =============================================

        // Parent Relationship: One MedicalCheck has Many MedicalBilans
        [ForeignKey("MedicalCheckId")]
        public MedicalCheck? MedicalCheck { get; set; }

        
        // =============================================
        // HELPER PROPERTIES
        // =============================================

        [NotMapped]
        public bool IsOverdue => RequiredDate.HasValue &&
                                 RequiredDate.Value < DateTime.Today &&
                                 !IsCompleted;

        [NotMapped]
        public int DaysUntilDue => RequiredDate.HasValue ?
            (RequiredDate.Value - DateTime.Today).Days : int.MaxValue;

        [NotMapped]
        public string Status => IsCompleted ? "COMPLÉTÉ" :
                               IsOverdue ? "EN RETARD" :
                               DaysUntilDue <= 7 ? "URGENT" : "EN ATTENTE";
    }
}