using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    /// <summary>
    /// NEW — CRUD form for ComponentLifeLimitDimensionType. This lookup
    /// existed since Revision 13 (see the model's own doc comment) but had
    /// no management UI at all — the only way to add a new counter (C130
    /// APU starts, drop count, ...) was editing
    /// ComponentLifeLimitDimensionTypeSeeder.cs and redeploying. Dadda
    /// asked "what's the link to CRUD this list" while testing Receipt and
    /// found there wasn't one — this closes that gap for real, not just for
    /// the test phase.
    ///
    /// Code is intentionally NOT editable once a row exists (see Edit.cshtml
    /// /_Form.cshtml — rendered read-only there, and the controller's Edit
    /// POST ignores any posted Code and keeps the entity's original). The 7
    /// seeded Codes (FH/CYCLES/CALENDAR_DAYS/TGO_LANDINGS/FULLSTOP_LANDINGS/
    /// CALENDAR_MONTHS/CALENDAR_YEARS) are a STABLE CONTRACT that
    /// IAircraftReadingProvider and ComponentLifeStatusCalculator switch on
    /// by Code — renaming one after the fact would silently break that
    /// special-casing. A brand-new dimension's Code is equally permanent
    /// once anything (a life-limit profile stage, a derogation) references
    /// it, so the same rule is applied uniformly rather than only to the 7
    /// seeded rows.
    /// </summary>
    public class ComponentLifeLimitDimensionTypeFormDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Unité")]
        public ComponentLifeLimitDimensionUnit Unit { get; set; } = ComponentLifeLimitDimensionUnit.Count;

        [Display(Name = "Basée sur le calendrier")]
        public bool IsCalendarBased { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ordre d'affichage")]
        public byte SortOrder { get; set; } = 99;

        /// <summary>Null = universelle (toutes familles). See ComponentLifeLimitDimensionType.AcMainGroupId's own doc comment.</summary>
        [Display(Name = "Famille aéronef (vide = universelle)")]
        public int? AcMainGroupId { get; set; }
    }
}
