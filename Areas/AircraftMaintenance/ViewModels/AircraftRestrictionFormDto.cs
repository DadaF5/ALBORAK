using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    // ════════════════════════════════════════════════════════════════
    //  STATIC OPTION LISTS
    // ════════════════════════════════════════════════════════════════

    public static class RestrictionOptions
    {
        public static readonly (string Code, string Label)[] Types =
        [
            ("OPS", "Opérationnelle (OPS)"),
            ("MNT", "Maintenance (MNT)")
        ];

        public static readonly (string Code, string Label)[] Severities =
        [
            ("CRITICAL", "Critique — vol interdit"),
            ("HIGH",     "Élevée — impact majeur"),
            ("MEDIUM",   "Moyenne — suivi requis")
        ];
    }

    

    // ════════════════════════════════════════════════════════════════
    //  AIRCRAFT RESTRICTION — FORM DTO
    // ════════════════════════════════════════════════════════════════
    public class AircraftRestrictionFormDto
    {
        public int Id         { get; set; }
        public int AircraftId { get; set; }

        // Aircraft info — read-only display
        public string? AircraftRegistration { get; set; }
        public string? AircraftTypeName     { get; set; }

        [Required(ErrorMessage = "Le type de restriction est obligatoire.")]
        [Display(Name = "Type")]
        public string? RestrictionType { get; set; }

        [Required(ErrorMessage = "La sévérité est obligatoire.")]
        [Display(Name = "Sévérité")]
        public string Severity { get; set; } = "HIGH";

        [Required(ErrorMessage = "La référence est obligatoire.")]
        [StringLength(80)]
        [Display(Name = "Référence")]
        public string Reference { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire.")]
        [StringLength(500)]
        [Display(Name = "Description de la restriction")]
        public string Description { get; set; } = string.Empty;

        [StringLength(80)]
        [Display(Name = "Émis par")]
        public string? IssuedBy { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [Display(Name = "Date de début")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Date d'expiration")]
        public DateOnly? ExpiryDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Certificat lié (optionnel)")]
        public int? CertificateId { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        // Dropdowns
        public IEnumerable<SelectListItem> CertificateOptions { get; set; } = [];

        public IEnumerable<SelectListItem> TypeOptions =>
            RestrictionOptions.Types
                .Select(t => new SelectListItem
                {
                    Value    = t.Code,
                    Text     = t.Label,
                    Selected = t.Code == RestrictionType
                });

        public IEnumerable<SelectListItem> SeverityOptions =>
            RestrictionOptions.Severities
                .Select(s => new SelectListItem
                {
                    Value    = s.Code,
                    Text     = s.Label,
                    Selected = s.Code == Severity
                });
    }

    // ════════════════════════════════════════════════════════════════
    //  AIRCRAFT RESTRICTION — LIST VM
    // ════════════════════════════════════════════════════════════════
    public class AircraftRestrictionListVm
    {
        public int      Id                   { get; set; }
        public int      AircraftId           { get; set; }
        public string   AircraftRegistration { get; set; } = string.Empty;
        public string?  AircraftTypeName     { get; set; }
        public string   RestrictionType      { get; set; } = string.Empty;
        public string   TypeLabel            { get; set; } = string.Empty;
        public string   Severity             { get; set; } = string.Empty;
        public string   Reference            { get; set; } = string.Empty;
        public string   Description          { get; set; } = string.Empty;
        public string?  IssuedBy             { get; set; }
        public DateOnly StartDate            { get; set; }
        public DateOnly? ExpiryDate          { get; set; }
        public int      DaysRemaining        { get; set; }
        public bool     IsExpired            { get; set; }
        public bool     IsActive             { get; set; }
        public string?  CertificateReference { get; set; }

        public string SeverityBadgeClass => Severity switch
        {
            "CRITICAL" => "bg-danger",
            "HIGH"     => "bg-warning text-dark",
            "MEDIUM"   => "bg-info text-dark",
            _          => "bg-secondary"
        };
    }

    // ════════════════════════════════════════════════════════════════
    //  AIRCRAFT RESTRICTION — INDEX VM
    // ════════════════════════════════════════════════════════════════
    public class AircraftRestrictionIndexVm
    {
        public List<AircraftRestrictionListVm> Items      { get; set; } = [];
        public int                             TotalCount { get; set; }
        public int                             TotalPages { get; set; }

        public int?    SearchAircraftId      { get; set; }
        public string? SearchType            { get; set; }
        public string? SearchSeverity        { get; set; }
        public bool?   SearchActive          { get; set; }
        public bool    SearchActiveOnly      { get; set; } = true;

        public string SortColumn    { get; set; } = "StartDate";
        public string SortDirection { get; set; } = "desc";
        public int    PageNumber    { get; set; } = 1;
        public int    PageSize      { get; set; } = 15;

        public IEnumerable<SelectListItem> AircraftOptions { get; set; } = [];

        // Summary
        public int CountCritical { get; set; }
        public int CountHigh     { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage     => PageNumber < TotalPages;
        public int  FirstItem       => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
        public int  LastItem        => Math.Min(PageNumber * PageSize, TotalCount);

        public string SortDirectionFor(string col) =>
            SortColumn == col && SortDirection == "asc" ? "desc" : "asc";
        public string SortIconFor(string col) =>
            SortColumn != col     ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up" : "fa-sort-down";
    }

}
