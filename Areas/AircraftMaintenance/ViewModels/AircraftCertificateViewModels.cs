using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  CERT TYPE OPTIONS — static, used by form and index filter
    // ══════════════════════════════════════════════════════════════
    public static class CertTypeOptions
    {
        public static readonly (string Code, string Label)[] All =
        [
            ("CdN", "Certificat de Navigabilité (CdN)"),
            ("CEN", "Compte Rendu d'Examen de Navigabilité (CEN)"),
            ("PEA", "Programme d'Entretien Agréé (PEA)"),
            ("LME", "Liste des Modifications et Équipements (LME)"),
            ("CDL", "Configuration Deviation List (CDL)")
        ];

        public static IEnumerable<SelectListItem> ToSelectList(
            string? selectedCode = null,
            string  placeholder  = "— Type de certificat —")
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = placeholder,
                        Selected = string.IsNullOrEmpty(selectedCode) }
            };
            items.AddRange(All.Select(c => new SelectListItem
            {
                Value    = c.Code,
                Text     = c.Label,
                Selected = c.Code == selectedCode
            }));
            return items;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Id == 0 → Create  |  Id > 0 → Edit
    // ══════════════════════════════════════════════════════════════
    public class AircraftCertificateFormDto
    {
        public int Id { get; set; }

        // AircraftId passed via route — not in form body
        public int AircraftId { get; set; }

        // Aircraft display info — read-only in form
        public string? AircraftCode    { get; set; }
        public string? AircraftTail    { get; set; }
        public string? AircraftTypeName { get; set; }

        [Required(ErrorMessage = "Le type de certificat est obligatoire.")]
        [Display(Name = "Type de certificat")]
        public string? CertType { get; set; }

        [Required(ErrorMessage = "La référence est obligatoire.")]
        [StringLength(80,
            ErrorMessage = "La reference ne peut pas depasser 80 caracteres.")]
        [Display(Name = "Référence")]
        public string Reference { get; set; } = string.Empty;

        [StringLength(80)]
        [Display(Name = "Autorité de délivrance")]
        public string? IssuingAuthority { get; set; }

        [Display(Name = "Date de délivrance")]
        public DateOnly? IssueDate { get; set; }

        [Display(Name = "Date d'expiration")]
        public DateOnly? ExpiryDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes / Observations")]
        public string? Notes { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        // File upload — not stored, handled separately
        [Display(Name = "Document numérisé (PDF)")]
        public IFormFile? DocumentFile { get; set; }

        // Current document info — read-only
        public string? DocumentPath { get; set; }
        public string? DocumentName { get; set; }

        // Dropdown
        public IEnumerable<SelectListItem> CertTypeOptions { get; set; } =
            ViewModels.CertTypeOptions.ToSelectList();
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM — one row in the Index table
    // ══════════════════════════════════════════════════════════════
    public class AircraftCertificateListVm
    {
        public int      Id               { get; set; }
        public int      AircraftId       { get; set; }
        public string   AircraftCode     { get; set; } = string.Empty;
        public string   AircraftTail     { get; set; } = string.Empty;
        public string?  AircraftTypeName { get; set; }
        public string   CertType         { get; set; } = string.Empty;
        public string?  CertTypeLabel    { get; set; }
        public string   Reference        { get; set; } = string.Empty;
        public string?  IssuingAuthority { get; set; }
        public DateOnly? IssueDate       { get; set; }
        public DateOnly? ExpiryDate      { get; set; }
        public bool     HasDocument      { get; set; }
        public bool     IsActive         { get; set; }
        public int      DaysRemaining    { get; set; }
        public string   StatusLabel      { get; set; } = string.Empty;
        public string   StatusClass      { get; set; } = string.Empty;

        // Badge color for expiry alert in table
        public string ExpiryBadgeClass =>
            DaysRemaining == int.MaxValue ? "bg-secondary" :
            DaysRemaining < 0            ? "bg-danger"    :
            DaysRemaining <= 30          ? "bg-warning text-dark" :
                                           "bg-success";
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    // ══════════════════════════════════════════════════════════════
    public class AircraftCertificateIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<AircraftCertificateListVm> Items      { get; set; } = [];
        public int                             TotalCount { get; set; }
        public int                             TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public int?    SearchAircraftId { get; set; }
        public string? SearchCertType   { get; set; }
        public bool?   SearchActive     { get; set; }
        // Quick filter: show only expiring soon
        public bool    SearchExpiringSoon { get; set; } = false;

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "ExpiryDate";
        public string SortDirection { get; set; } = "asc";

        // ── Paging ───────────────────────────────────────────────
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 15;

        // ── Filter dropdowns ──────────────────────────────────────
        public IEnumerable<SelectListItem> AircraftOptions  { get; set; } = [];
        public IEnumerable<SelectListItem> CertTypeOptions  { get; set; } =
            ViewModels.CertTypeOptions.ToSelectList(
                placeholder: "— Tous les types —");

        // ── Summary counters ──────────────────────────────────────
        public int CountExpired     { get; set; }
        public int CountExpiringSoon { get; set; }  // ≤ 30 days
        public int CountValid       { get; set; }

        // ── Convenience flags ─────────────────────────────────────
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage     => PageNumber < TotalPages;
        public int  FirstItem       => TotalCount == 0
                                           ? 0
                                           : (PageNumber - 1) * PageSize + 1;
        public int  LastItem        => Math.Min(PageNumber * PageSize, TotalCount);

        // ── Sort helpers ──────────────────────────────────────────
        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        public string SortIconFor(string column) =>
            SortColumn != column     ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up"
                                     : "fa-sort-down";
    }
}
