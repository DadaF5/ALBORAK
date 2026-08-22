using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class ComponentTypeListDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Nomenclature { get; set; } = string.Empty;
        public string? AtaLabel { get; set; }
        public ComponentTrackingMethod TrackingMethod { get; set; }
        public int LifeLimitProfileCount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Basic catalog fields only. Life-limit schedules (one or more staged,
    /// S/N-resolvable profiles) are managed on their own page — see
    /// ComponentLifeLimitProfileFormDto / ComponentTypesController.ManageLifeLimits
    /// — same "dedicated Manage page" pattern already used for
    /// InspectionType.ManagePrograms, rather than cramming a whole staged
    /// schedule into this Create/Edit form.
    /// </summary>
    public class ComponentTypeFormDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le numéro de pièce est requis.")]
        [StringLength(50)]
        [Display(Name = "Numéro de pièce (P/N)")]
        public string PartNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nomenclature est requise.")]
        [StringLength(200)]
        [Display(Name = "Nomenclature")]
        public string Nomenclature { get; set; } = string.Empty;

        [Display(Name = "Chapitre ATA")]
        public int? AtaId { get; set; }

        [Display(Name = "Fabricant")]
        public int? AircraftManufacturerId { get; set; }

        [Required]
        [Display(Name = "Méthode de suivi")]
        public ComponentTrackingMethod TrackingMethod { get; set; } = ComponentTrackingMethod.OnCondition;

        [Display(Name = "Sérialisé")]
        public bool IsSerialized { get; set; } = true;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; }

        [Display(Name = "Positions éligibles")]
        public List<int> EligiblePositionIds { get; set; } = new();
    }
}
