using System.ComponentModel.DataAnnotations;
using FRAProject.Areas.AircraftMaintenance.Models;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class ComponentPositionListDto
    {
        public int Id { get; set; }
        public string AcTypeName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AtaLabel { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComponentPositionFormDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le type d'aéronef est requis.")]
        [Display(Name = "Type d'aéronef")]
        public int AcTypeId { get; set; }

        [Required(ErrorMessage = "Le code est requis.")]
        [StringLength(30)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(150)]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Chapitre ATA")]
        public int? AtaId { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        public byte SortOrder { get; set; }
    }

    /// <summary>
    /// NEW — backs ComponentPositionsController.Tree, one node per AcType
    /// under a given AcMainGroup, holding its ComponentPosition leaves
    /// directly (same "reuse the entity, no extra flattening" convention
    /// Index.cshtml already uses for its own List&lt;ComponentPosition&gt;).
    /// AcMainGroup/AcType themselves are read-only here — they're managed in
    /// the Réglages module, not this one; this tree only lets you add/edit/
    /// deactivate the Position leaves underneath them.
    /// </summary>
    public class AcTypePositionsNodeDto
    {
        public int AcTypeId { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;
        public List<ComponentPosition> Positions { get; set; } = new();
    }

    /// <summary>NEW — one node per AcMainGroup, see AcTypePositionsNodeDto's doc comment.</summary>
    public class AcMainGroupTreeNodeDto
    {
        public int AcMainGroupId { get; set; }
        public string AcMainGroupLabel { get; set; } = string.Empty;
        public List<AcTypePositionsNodeDto> AcTypes { get; set; } = new();
    }
}
