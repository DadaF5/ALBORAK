using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Validation annotations live here — never on the Model.
    //  Id == 0 → Create
    //  Id  > 0 → Edit
    //
    //  StringLength limits match LookupBase:
    //    Code → 30, Name → 150, Description → 250
    // ══════════════════════════════════════════════════════════════
    public class AircraftManufacturerFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(30,
            ErrorMessage = "Le code ne peut pas depasser 30 caracteres.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(150,
            ErrorMessage = "Le nom ne peut pas depasser 150 caracteres.")]
        [Display(Name = "Nom du constructeur")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250,
            ErrorMessage = "La description ne peut pas depasser 250 caracteres.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, 255,
            ErrorMessage = "L'ordre doit etre entre 0 et 255.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 99;
        // Stored as byte in LookupBase (0–255).
        // Int in DTO for form binding — cast to (byte) in controller.

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM — one row in the Index table
    // ══════════════════════════════════════════════════════════════
    public class AircraftManufacturerListVm
    {
        public int     Id          { get; set; }
        public string  Code        { get; set; } = string.Empty;
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int     SortOrder   { get; set; }
        public bool    IsActive    { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Wraps paged list + search / sort / page state.
    // ══════════════════════════════════════════════════════════════
    public class AircraftManufacturerIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<AircraftManufacturerListVm> Items      { get; set; } = [];
        public int                              TotalCount { get; set; }
        public int                              TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public string? SearchCode   { get; set; }
        public string? SearchName   { get; set; }
        public bool?   SearchActive { get; set; }

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "Name";
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
