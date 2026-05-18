using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    //  FORM DTO  (shared by Create and Edit)
    //  Validation annotations live here — never on the Model.
    //  Id == 0 → Create  |  Id > 0 → Edit
    //
    //  Two sections:
    //    Section 1 — Identification (FKs, Code, Name, Description)
    //    Section 2 — Spécifications techniques (specs for Ops + MRO)
    // ══════════════════════════════════════════════════════════════
    public class AcTypeFormDto
    {
        public int Id { get; set; }

        // ── Section 1 — Identification ────────────────────────────

        [Required(ErrorMessage = "Le groupe principal est obligatoire.")]
        [Display(Name = "Groupe principal")]
        public int? AcMainGroupId { get; set; }

        [Display(Name = "Constructeur")]
        public int? AircraftManufacturerId { get; set; }   // optional

        [Required(ErrorMessage = "Le code est obligatoire.")]
        [StringLength(30,
            ErrorMessage = "Le code ne peut pas depasser 30 caracteres.")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100,
            ErrorMessage = "Le nom ne peut pas depasser 100 caracteres.")]
        [Display(Name = "Nom du type")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250,
            ErrorMessage = "La description ne peut pas depasser 250 caracteres.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, 255,
            ErrorMessage = "L'ordre doit etre entre 0 et 255.")]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 99;
        // byte in model — int in DTO for form binding, cast in controller.

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        // ── Section 2 — Spécifications techniques ─────────────────
        // Used by SquadronOps (sortie planning) and MRO2 (maintenance).

        [Required(ErrorMessage = "La masse maximale est obligatoire.")]
        [Range(0, 999999,
            ErrorMessage = "La masse doit etre entre 0 et 999 999 kg.")]
        [Display(Name = "Masse maximale (kg)")]
        public double MaxGrossWeight { get; set; }

        [Required(ErrorMessage = "Le nombre de moteurs est obligatoire.")]
        [Range(1, 8,
            ErrorMessage = "Le nombre de moteurs doit etre entre 1 et 8.")]
        [Display(Name = "Nombre de moteurs")]
        public int MaxEngines { get; set; }

        [Required(ErrorMessage = "Le nombre de sieges est obligatoire.")]
        [Range(1, 999,
            ErrorMessage = "Le nombre de sieges doit etre entre 1 et 999.")]
        [Display(Name = "Nombre de sièges")]
        public int SeatCount { get; set; }

        [Required(ErrorMessage = "La capacite passagers est obligatoire.")]
        [Range(0, 999,
            ErrorMessage = "La capacite doit etre entre 0 et 999.")]
        [Display(Name = "Capacité passagers")]
        public int MaxPassengers { get; set; }

        // ── Dropdowns — populated by controller ───────────────────
        public IEnumerable<SelectListItem> AcMainGroupOptions        { get; set; } = [];
        public IEnumerable<SelectListItem> ManufacturerOptions       { get; set; } = [];
    }

    // ══════════════════════════════════════════════════════════════
    //  LIST ITEM VM — one row in the Index table
    // ══════════════════════════════════════════════════════════════
    public class AcTypeListVm
    {
        public int     Id                    { get; set; }
        public string  Code                  { get; set; } = string.Empty;
        public string  Name                  { get; set; } = string.Empty;
        public string? Description           { get; set; }
        public int     AcMainGroupId         { get; set; }
        public string? AcMainGroupName       { get; set; }   // joined
        public int?    AircraftManufacturerId { get; set; }
        public string? ManufacturerName      { get; set; }   // joined
        public double  MaxGrossWeight        { get; set; }
        public int     MaxEngines            { get; set; }
        public int     SeatCount             { get; set; }
        public int     MaxPassengers         { get; set; }
        public int     SortOrder             { get; set; }
        public bool    IsActive              { get; set; }

        // ── Computed for view ──────────────────────────────────────
        /// <summary>
        /// AircraftVersions count — loaded separately in controller.
        /// Shows how many variants exist for this type.
        /// </summary>
        public int VersionCount { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INDEX PAGE VM
    //  Two FK filters: AcMainGroup + Manufacturer
    // ══════════════════════════════════════════════════════════════
    public class AcTypeIndexVm
    {
        // ── Data ─────────────────────────────────────────────────
        public List<AcTypeListVm> Items      { get; set; } = [];
        public int                TotalCount { get; set; }
        public int                TotalPages { get; set; }

        // ── Search criteria ───────────────────────────────────────
        public string? SearchCode             { get; set; }
        public string? SearchName             { get; set; }
        public int?    SearchAcMainGroupId    { get; set; }
        public int?    SearchManufacturerId   { get; set; }
        public bool?   SearchActive           { get; set; }

        // ── Sorting ───────────────────────────────────────────────
        public string SortColumn    { get; set; } = "SortOrder";
        public string SortDirection { get; set; } = "asc";

        // ── Paging ───────────────────────────────────────────────
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 10;

        // ── Filter dropdowns ──────────────────────────────────────
        public IEnumerable<SelectListItem> AcMainGroupOptions  { get; set; } = [];
        public IEnumerable<SelectListItem> ManufacturerOptions { get; set; } = [];

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
