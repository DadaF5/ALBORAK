using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  All validation annotations live here — never on the Model.
    //  Id == 0 → Create
    //  Id  > 0 → Edit
    // ══════════════════════════════════════════════════════════════
    public class CountryFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code ISO est obligatoire.")]
        [StringLength(2, MinimumLength = 2,
            ErrorMessage = "Le code ISO doit contenir exactement 2 caracteres.")]
        [Display(Name = "Code ISO")]
        public string IsoCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du pays est obligatoire.")]
        [StringLength(100,
            ErrorMessage = "Le nom ne peut pas depasser 100 caracteres.")]
        [Display(Name = "Nom du pays")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50,
            ErrorMessage = "Le continent ne peut pas depasser 50 caracteres.")]
        [Display(Name = "Continent")]
        public string? Continent { get; set; }

        [Range(0, 9999,
            ErrorMessage = "L'ordre d'affichage doit etre entre 0 et 9999.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM
    //  One row in the Index table.
    //  Projected directly from DB via Select() — minimal fields.
    // ══════════════════════════════════════════════════════════════
    public class CountryListVm
    {
        public int     Id         { get; set; }
        public string  IsoCode    { get; set; } = string.Empty;
        public string  Name       { get; set; } = string.Empty;
        public string? Continent  { get; set; }
        public int     SortOrder  { get; set; }
        public bool    IsActive   { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Passed to the Index view — wraps paged list + all
    //  search / sort / page state so the view can rebuild links
    //  without losing context between requests.
    // ══════════════════════════════════════════════════════════════
    public class CountryIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<CountryListVm> Items      { get; set; } = [];
        public int                 TotalCount { get; set; }
        public int                 TotalPages { get; set; }

        // ── Search criteria — bound from query string ─────────────
        public string? SearchIsoCode   { get; set; }
        public string? SearchName      { get; set; }
        public string? SearchContinent { get; set; }
        public bool?   SearchActive    { get; set; }  // null = all

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc";

        // ── Paging ───────────────────────────────────────────────
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

        // ── Convenience flags for Razor pager ────────────────────
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage     => PageNumber < TotalPages;

        public int FirstItem => TotalCount == 0
                                    ? 0
                                    : (PageNumber - 1) * PageSize + 1;

        public int LastItem  => Math.Min(PageNumber * PageSize, TotalCount);

        // ── Sort helpers for column header links ──────────────────

        /// <summary>
        /// Returns the direction the next click should sort.
        /// Used to build href links on sortable column headers.
        /// </summary>
        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        /// <summary>
        /// FontAwesome icon class for the sort indicator arrow.
        ///   fa-sort            = not the current sort column (muted)
        ///   fa-sort-up         = sorted ASC on this column
        ///   fa-sort-down       = sorted DESC on this column
        /// </summary>
        public string SortIconFor(string column) =>
            SortColumn != column     ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up"
                                     : "fa-sort-down";
    }
}
