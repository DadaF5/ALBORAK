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
    /// CHANGED (Details-hub-page pass, follow-up) — Positions éligibles moved
    /// OFF this form entirely, following the exact same "moved off Edit onto
    /// its own dedicated Manage page, linked from Details" process already
    /// applied to Life Limits/Sub-assembly Slots/Derogations. See
    /// ComponentTypePositionsFormDto / ComponentTypesController.ManagePositions.
    /// This DTO is now pure catalog fields — nothing else.
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
    }

    /// <summary>
    /// NEW — read-first "hub" page for one PN (same role Component/Details.cshtml
    /// plays for a physical S/N). Edit stays the lean data-entry form (basic
    /// catalog fields only); this DTO backs the page that ties together the
    /// PN's summary plus links/counts for its four "Manage" sub-areas (Life
    /// Limits, Sub-assembly Slots, Derogations, and — as of the follow-up
    /// pass — Positions éligibles), which used to be alert-box links (or, for
    /// Positions, a raw picker) stacked inside the Edit form itself.
    /// </summary>
    public class ComponentTypeDetailsDto
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Nomenclature { get; set; } = string.Empty;
        public string? AtaLabel { get; set; }
        public string? AircraftManufacturerLabel { get; set; }
        public ComponentTrackingMethod TrackingMethod { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Read-only display labels ("F5E — APU — APU") — editing now happens on ManagePositions, not here.</summary>
        public List<string> EligiblePositionLabels { get; set; } = new();

        public int LifeLimitProfileCount { get; set; }
        public int SubAssemblySlotCount { get; set; }
        public int DerogationActiveCount { get; set; }
        public int DerogationTotalCount { get; set; }

        /// <summary>Physical Component (S/N) rows currently under this PN — pure informational count, not a link target.</summary>
        public int ComponentCount { get; set; }
    }

    /// <summary>
    /// NEW — richer shape for the "Positions éligibles" picker than a flat
    /// SelectListItem, so the view can filter/group by Famille aéronef
    /// (AcMainGroup) and Type avion (AcType) client-side instead of showing
    /// one long unfiltered list (real complaint after live-testing with
    /// several AcTypes seeded — F5E/F5F entries interleaved with no way to
    /// narrow down).
    /// </summary>
    public class ComponentPositionOptionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;
        public int AcMainGroupId { get; set; }
        public string AcMainGroupLabel { get; set; } = string.Empty;
    }

    /// <summary>
    /// NEW (Details-hub-page pass, follow-up) — backs the new dedicated
    /// ManagePositions page. Positions éligibles used to post back as part of
    /// ComponentTypeFormDto; now it's its own small form, same "Manage" page
    /// pattern as Life Limits/Sub-assembly Slots/Derogations, reached from a
    /// link on the Details hub instead of being embedded in Edit.
    /// </summary>
    public class ComponentTypePositionsFormDto
    {
        public int ComponentTypeId { get; set; }

        [Display(Name = "Positions éligibles")]
        public List<int> SelectedPositionIds { get; set; } = new();
    }
}
