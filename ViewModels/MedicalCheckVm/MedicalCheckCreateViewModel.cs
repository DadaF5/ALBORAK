//using System.ComponentModel.DataAnnotations;

//namespace FRAProject.ViewModels.MedicalCheck
//{
//    public class MedicalCheckCreateViewModel
//    {
//    }
//}

// MedicalCheckCreateViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class MedicalCheckCreateViewModel
    {
        // MedicalCheck properties
        public int CrewMemberId { get; set; }

        [Required(ErrorMessage = "Le type de visite est requis")]
        [Display(Name = "Type de Visite")]
        public string MedCheckType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de personnel est requis")]
        [Display(Name = "Type de Personnel")]
        public string CaptainType { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de visite est requise")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de Visite")]
        public DateTime? CheckDate { get; set; }

        [Required(ErrorMessage = "La durée de validité est requise")]
        [Range(1, 730, ErrorMessage = "La durée doit être entre 1 et 730 jours")]
        [Display(Name = "Jours Valides")]
        public int? DaysValid { get; set; }

        [Display(Name = "Observations")]
        public string? Obs { get; set; }

        [Display(Name = "Décision")]
        public string? Decision { get; set; }

        [Display(Name = "Prochaine Échéance")]
        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        [Required(ErrorMessage = "La spécialité est requise")]
        [Display(Name = "Spécialité")]
        public string? Speciality { get; set; }

        [Display(Name = "Constatations")]
        public string? Constatations { get; set; }

        [Display(Name = "Obésité")]
        public bool OBESITE { get; set; }

        [Display(Name = "Correction Optique")]
        public bool C_Optique { get; set; }

        [Required(ErrorMessage = "L'aptitude est requise")]
        [Display(Name = "Aptitude")]
        public string? Aptitude { get; set; }

        [Display(Name = "Prochaine Visite Unité")]
        [DataType(DataType.Date)]
        public DateTime? Next_VU_Date { get; set; }

        [Display(Name = "Date Visite Unité")]
        [DataType(DataType.Date)]
        public DateTime? VU_Date { get; set; }

        [Display(Name = "Raison de Retard")]
        public string? LateCheckReason { get; set; }

        [Display(Name = "Raison Retard VU")]
        public string? VuLateCheckReason { get; set; }

        // For specialty-specific fields
        [Display(Name = "Heures de vol totales")]
        public decimal? FlightHours { get; set; }

        [Display(Name = "Type d'appareil")]
        public string? AircraftType { get; set; }

        [Display(Name = "Certifications spécifiques")]
        public string? MN_Certifications { get; set; }

        [Display(Name = "Numéro de certification CCA")]
        public string? CCA_Number { get; set; }

        // CrewMember and Person display properties (read-only)
        public string? Captain { get; set; }
        public string? NickName { get; set; }
        public string? Matricule { get; set; }
        public string? FullName { get; set; }
        public string? Grade { get; set; }
        public string? Unit { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Role { get; set; }
        public string? CrewMemberType { get; set; }
        public string? Squadron { get; set; }

        // For dropdowns
        public List<string> MedCheckTypes { get; set; } = new List<string> { "CEMPN", "CONTROL", "VISITE A L'UNITE" };
        public List<string> CaptainTypes { get; set; } = new List<string> { "PILOT", "CONTROLLER", "DRIVER", "TECHNICIAN" };
        public List<string> Specialities { get; set; } = new List<string> { "PH", "PC", "MN", "CCA" };
        public List<string> Aptitudes { get; set; } = new List<string> { "APTE", "APTE PAR DEROGATION", "INAPTE" };
    }
}
