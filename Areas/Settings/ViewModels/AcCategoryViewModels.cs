using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Id == 0 → Create
    //  Id  > 0 → Edit
    // ══════════════════════════════════════════════════════════════
    public class AcCategoryFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(10, MinimumLength = 2,
            ErrorMessage = "Le code doit contenir entre 2 et 10 caracteres.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(50,
            ErrorMessage = "Le nom ne peut pas depasser 50 caracteres.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200,
            ErrorMessage = "La description ne peut pas depasser 200 caracteres.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(10,
            ErrorMessage = "L'icone ne peut pas depasser 10 caracteres.")]
        [Display(Name = "Icone")]
        public string? IconKey { get; set; }

        [Range(0, 9999,
            ErrorMessage = "L'ordre doit etre entre 0 et 9999.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM
    //  One row in the Index table.
    // ══════════════════════════════════════════════════════════════
    public class AcCategoryListVm
    {
        public int     Id          { get; set; }
        public string  Code        { get; set; } = string.Empty;
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconKey     { get; set; }
        public int     SortOrder   { get; set; }
        public bool    IsActive    { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Wraps paged list + search / sort / page state.
    // ══════════════════════════════════════════════════════════════
    public class AcCategoryIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<AcCategoryListVm> Items      { get; set; } = [];
        public int                    TotalCount { get; set; }
        public int                    TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public string? SearchCode   { get; set; }
        public string? SearchName   { get; set; }
        public bool?   SearchActive { get; set; }

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "SortOrder";
        public string SortDirection { get; set; } = "asc";

        // ── Paging ───────────────────────────────────────────────
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

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
