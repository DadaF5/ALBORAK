using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderSectionPartFormViewModel
    {
        public int? Id { get; set; }
        public int WorkOrderSectionId { get; set; }

        [StringLength(100)]
        [Display(Name = "Nomenclature (ancien)")]
        public string? OldNomenclature { get; set; }

        [StringLength(50)]
        [Display(Name = "Numéro (ancien)")]
        public string? OldNumero { get; set; }

        [StringLength(50)]
        [Display(Name = "Vieillissement (ancien)")]
        public string? OldVieillissement { get; set; }

        [StringLength(100)]
        [Display(Name = "Nomenclature (nouveau)")]
        public string? NewNomenclature { get; set; }

        [StringLength(50)]
        [Display(Name = "Numéro (nouveau)")]
        public string? NewNumero { get; set; }

        [StringLength(50)]
        [Display(Name = "Vieillissement (nouveau)")]
        public string? NewVieillissement { get; set; }

        [StringLength(200)]
        [Display(Name = "Désignation et position")]
        public string? DesignationEtPosition { get; set; }

        [StringLength(200)]
        [Display(Name = "Motif de la dépose")]
        public string? MotifDepose { get; set; }

        [StringLength(10)]
        [Display(Name = "Symbole")]
        public string? Symbole { get; set; }

        [Display(Name = "Temps alloué (min)")]
        public int? TempsAlloueMinutes { get; set; }

        [Display(Name = "Date")]
        public DateOnly? Date { get; set; }

        [Display(Name = "Temps passé (min)")]
        public int? TempsPasseMinutes { get; set; }

        [StringLength(20)]
        [Display(Name = "Spécial")]
        public string? ExecutantSpecial { get; set; }

        [StringLength(100)]
        [Display(Name = "Nom de l'exécutant")]
        public string? ExecutantNom { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 100;
    }

    public class WorkOrderSectionPartListItemViewModel
    {
        public int Id { get; set; }
        public string? OldNomenclature { get; set; }
        public string? OldNumero { get; set; }
        public string? NewNomenclature { get; set; }
        public string? NewNumero { get; set; }
        public string? DesignationEtPosition { get; set; }
        public string? MotifDepose { get; set; }
        public DateOnly? Date { get; set; }
        public string? ExecutantNom { get; set; }
        public bool IsSigned { get; set; }
    }
}