using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderSectionTaskFormViewModel
    {
        public int? Id { get; set; }
        public int WorkOrderSectionId { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Désignation des travaux")]
        public string DesignationTravaux { get; set; } = string.Empty;

        [Display(Name = "Temps alloué (min)")]
        public int? TempsAlloueMinutes { get; set; }

        [Display(Name = "Date")]
        public DateOnly? Date { get; set; }

        [Display(Name = "Temps passé — Système (min)")]
        public int? TempsPasseSystemeMinutes { get; set; }

        [Display(Name = "Temps passé — Retouches (min)")]
        public int? TempsPasseRetouchesMinutes { get; set; }

        [StringLength(20)]
        [Display(Name = "Spécial")]
        public string? ExecutantSpecial { get; set; }

        [StringLength(100)]
        [Display(Name = "Nom de l'exécutant")]
        public string? ExecutantNom { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 100;
    }

    public class WorkOrderSectionTaskListItemViewModel
    {
        public int Id { get; set; }
        public string DesignationTravaux { get; set; } = string.Empty;
        public int? TempsAlloueMinutes { get; set; }
        public DateOnly? Date { get; set; }
        public int? TempsPasseSystemeMinutes { get; set; }
        public int? TempsPasseRetouchesMinutes { get; set; }
        public string? ExecutantNom { get; set; }
        public bool IsSigned { get; set; }
    }
}