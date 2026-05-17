using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Validation annotations live here — never on the Model.
    //  Id == 0 → Create
    //  Id  > 0 → Edit
    // ══════════════════════════════════════════════════════════════
    public class EmployingAuthorityFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(10, MinimumLength = 2,
            ErrorMessage = "Le code doit contenir entre 2 et 10 caracteres.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100,
            ErrorMessage = "Le nom ne peut pas depasser 100 caracteres.")]
        [Display(Name = "Nom officiel")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 9999,
            ErrorMessage = "L'ordre d'affichage doit etre entre 0 et 9999.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM
    //  One row in the Index table — only columns the view needs.
    // ══════════════════════════════════════════════════════════════
    public class EmployingAuthorityListVm
    {
        public int    Id        { get; set; }
        public string Code      { get; set; } = string.Empty;
        public string Name      { get; set; } = string.Empty;
        public int    SortOrder { get; set; }
        public bool   IsActive  { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Wraps paged list + search / sort / page state.
    //  Simpler than CountryIndexVm — fewer search criteria.
    // ══════════════════════════════════════════════════════════════
    public class EmployingAuthorityIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<EmployingAuthorityListVm> Items      { get; set; } = [];
        public int                            TotalCount { get; set; }
        public int                            TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public string? SearchCode   { get; set; }
        public string? SearchName   { get; set; }
        public bool?   SearchActive { get; set; }  // null = all

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

        // ── Sort helpers for column header links ──────────────────
        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        public string SortIconFor(string column) =>
            SortColumn != column     ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up"
                                     : "fa-sort-down";
    }
}
