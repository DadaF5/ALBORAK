using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.Areas.Settings.ViewModels
{
    public class AircraftFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La marque d'immatriculation est obligatoire.")]
        [StringLength(50, ErrorMessage = "La marque ne peut pas depasser 50 caracteres.")]
        [Display(Name = "Marque d'immatriculation")]
        public string Registration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numero de queue est obligatoire.")]
        [Range(1, 9999, ErrorMessage = "Le numero de queue doit etre entre 1 et 9999.")]
        [Display(Name = "Numéro de queue")]
        public int TailNo { get; set; }

        [StringLength(100)]
        [Display(Name = "N° de série constructeur")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "Le type d'aeronef est obligatoire.")]
        [Display(Name = "Type d'aéronef")]
        public int? AcTypeId { get; set; }

        [Display(Name = "Version / Variante")]
        public int? AircraftVersionId { get; set; }

        [Display(Name = "Constructeur")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "Pays d'origine")]
        public int? OriginCountryId { get; set; }

        [Display(Name = "Date de fabrication")]
        public DateTime? ManufactureDate { get; set; }

        [Display(Name = "Date d'entrée en service")]
        public DateOnly? ServiceEntryDate { get; set; }

        [Display(Name = "Date d'immatriculation DAM")]
        public DateOnly? RegistrationDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Obs { get; set; }

        [Range(0, 255)]
        [Display(Name = "Ordre d'affichage")]
        public int SortOrder { get; set; } = 99;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Le statut est obligatoire.")]
        [Display(Name = "Statut opérationnel")]
        public int? AcStatusTypeId { get; set; }

        [Required(ErrorMessage = "La base d'affectation est obligatoire.")]
        [Display(Name = "Base d'affectation (Port d'attache)")]
        public int? BaseId { get; set; }

        [Display(Name = "Rôle et mission")]
        public int? MissionRoleId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Le compteur HdV ne peut pas etre negatif.")]
        [Display(Name = "Potentiel HdV (minutes)")]
        public int TotalFlightMinutes { get; set; } = 0;

        [Range(0, int.MaxValue)]
        [Display(Name = "Cycles moteur")]
        public int TotalCycles { get; set; } = 0;

        [Range(0, int.MaxValue)]
        [Display(Name = "Atterrissages")]
        public int TotalLandings { get; set; } = 0;

        [Display(Name = "N° Dossier immatriculation")]
        public int? DossierId { get; set; }

        public string? DossierNumber { get; set; }

        public IEnumerable<SelectListItem> AcTypeOptions { get; set; } = [];
        public IEnumerable<SelectListItem> VersionOptions { get; set; } = [];
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions { get; set; } = [];
        public IEnumerable<SelectListItem> MissionRoleOptions { get; set; } = [];
        public IEnumerable<SelectListItem> ManufacturerOptions { get; set; } = [];
        public IEnumerable<SelectListItem> CountryOptions { get; set; } = [];

        public int FlightHours => TotalFlightMinutes / 60;
        public int FlightMinutes => TotalFlightMinutes % 60;
    }

    public class AircraftListVm
    {
        public int Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public int TailNo { get; set; }
        public string? SerialNumber { get; set; }

        public string? AcTypeName { get; set; }
        public string? VersionName { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusName { get; set; }
        public string? BaseName { get; set; }
        public string? MissionRoleName { get; set; }

        public int TotalFlightMinutes { get; set; }
        public int TotalCycles { get; set; }
        public int TotalLandings { get; set; }

        public DateOnly? ServiceEntryDate { get; set; }
        public DateOnly? RegistrationDate { get; set; }

        public bool IsActive { get; set; }

        public string FlightHoursDisplay
        {
            get
            {
                var h = TotalFlightMinutes / 60;
                var m = TotalFlightMinutes % 60;
                return $"{h}:{m:D2}";
            }
        }

        public string StatusBadgeClass => StatusCode switch
        {
            "OPR" => "bg-success",
            "MNT" => "bg-warning text-dark",
            "AOG" => "bg-danger",
            "STK" => "bg-secondary",
            "RAD" => "bg-dark border border-secondary",
            _ => "bg-secondary"
        };
    }

    public class AircraftIndexVm
    {
        public List<AircraftListVm> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public string? SearchRegistration { get; set; }
        public int? SearchAcTypeId { get; set; }
        public int? SearchStatusId { get; set; }
        public int? SearchBaseId { get; set; }
        public bool? SearchActive { get; set; }

        public string SortColumn { get; set; } = "Registration";
        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        public IEnumerable<SelectListItem> AcTypeOptions { get; set; } = [];
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions { get; set; } = [];

        public int TotalAircraft { get; set; }
        public int TotalOpr { get; set; }
        public int TotalMnt { get; set; }
        public int TotalAog { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int FirstItem => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
        public int LastItem => Math.Min(PageNumber * PageSize, TotalCount);

        public string SortDirectionFor(string column) =>
            SortColumn == column && SortDirection == "asc" ? "desc" : "asc";

        public string SortIconFor(string column) =>
            SortColumn != column ? "fa-sort text-secondary"
            : SortDirection == "asc" ? "fa-sort-up"
            : "fa-sort-down";
    }
}