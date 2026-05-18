using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Validation annotations live here — never on the Model.
    //  Id == 0 → Create
    //  Id  > 0 → Edit
    //
    //  Name/Description limits match existing DB columns (50 chars).
    // ══════════════════════════════════════════════════════════════
    public class AcMainGroupFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(30,
            ErrorMessage = "Le code ne peut pas depasser 30 caracteres.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(50,
            ErrorMessage = "Le nom ne peut pas depasser 50 caracteres.")]
        [Display(Name = "Nom du groupe")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50,
            ErrorMessage = "La description ne peut pas depasser 50 caracteres.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La categorie d'aeronef est obligatoire.")]
        [Display(Name = "Categorie d'aeronef")]
        public int? AcCategoryId { get; set; }

        [Required(ErrorMessage = "La base aerienne est obligatoire.")]
        [Display(Name = "Base aerienne")]
        public int? BaseId { get; set; }

        [Range(0, 255,
            ErrorMessage = "L'ordre doit etre entre 0 et 255.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 99;
        // Stored as byte — int in DTO for form binding, cast in controller.

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        // ── Dropdowns — populated by controller ───────────────────
        public IEnumerable<SelectListItem> AcCategoryOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions        { get; set; } = [];
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM — one row in the Index table
    // ══════════════════════════════════════════════════════════════
    public class AcMainGroupListVm
    {
        public int     Id               { get; set; }
        public string  Code             { get; set; } = string.Empty;
        public string  Name             { get; set; } = string.Empty;
        public string? Description      { get; set; }
        public int     AcCategoryId     { get; set; }
        public string? AcCategoryName   { get; set; }   // joined from AcCategory
        public int     BaseId           { get; set; }
        public string? BaseName         { get; set; }   // joined from Base
        public int     SortOrder        { get; set; }
        public bool    IsActive         { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Two FKs → two search filters (category + base).
    // ══════════════════════════════════════════════════════════════
    public class AcMainGroupIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<AcMainGroupListVm> Items      { get; set; } = [];
        public int                     TotalCount { get; set; }
        public int                     TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public string? SearchCode         { get; set; }
        public string? SearchName         { get; set; }
        public int?    SearchAcCategoryId { get; set; }
        public int?    SearchBaseId       { get; set; }
        public bool?   SearchActive       { get; set; }

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "SortOrder";
        public string SortDirection { get; set; } = "asc";

        // ── Paging ───────────────────────────────────────────────
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

        // ── Filter dropdowns ──────────────────────────────────────
        public IEnumerable<SelectListItem> AcCategoryOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions        { get; set; } = [];

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
