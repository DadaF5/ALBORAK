using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ── LIST ViewModel ───────────────────────────────────────────────────
    // What the Index view receives — only the columns the table needs.

    public class AircraftVersionListVm
    {
        public int    Id       { get; set; }
        public string Code     { get; set; } = string.Empty;
        public string Name     { get; set; } = string.Empty;
        public int    SortOrder { get; set; }
        public bool   IsActive { get; set; }
    }

    // ── INDEX PAGE ViewModel ─────────────────────────────────────────────
    // Wraps the paged list + the current search/sort/page state.
    // The view binds to this single object.

    public class AircraftVersionIndexVm
    {
        // Data
        public List<AircraftVersionListVm> Items     { get; set; } = [];
        public int                         TotalCount { get; set; }
        public int                         TotalPages { get; set; }

        // Search criteria — bound from query string
        public string? SearchCode    { get; set; }
        public string? SearchName    { get; set; }
        public bool?   SearchActive  { get; set; }   // null = all, true = active only, false = inactive only

        // Sorting — column name + direction
        public string SortColumn    { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc";   // "asc" | "desc"

        // Paging
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

        // Convenience flags for Razor view
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage     => PageNumber < TotalPages;
        public int  FirstItem       => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
        public int  LastItem        => Math.Min(PageNumber * PageSize, TotalCount);

        // Helper: return "asc" or "desc" for a column header link
        // so clicking the same column toggles direction
        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        // Helper: CSS class for sort indicator icon in column headers
        public string SortIconFor(string column) =>
            SortColumn != column ? "fa-sort text-muted"
            : SortDirection == "asc" ? "fa-sort-up"
            : "fa-sort-down";
    }

    // ── CREATE / EDIT DTO ────────────────────────────────────────────────
    // What the Create/Edit form posts — with validation attributes.

    public class AircraftVersionFormDto
    {
        public int Id { get; set; }   // 0 for Create, >0 for Edit

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le code ne peut pas d&eacute;passer 20 caract&egrave;res.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas d&eacute;passer 100 caract&egrave;res.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ordre d'affichage")]
        [Range(0, 9999, ErrorMessage = "L'ordre doit &ecirc;tre entre 0 et 9999.")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }
}
