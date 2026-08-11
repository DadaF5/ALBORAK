using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderSectionFormViewModel
    {
        public int? Id { get; set; }
        public int WorkOrderId { get; set; }

        [Required]
        [Display(Name = "Section")]
        public int WorkSectionId { get; set; }

        [StringLength(30)]
        [Display(Name = "N° Formule 13")]
        public string? FormNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Organisme responsable")]
        public string? OrganismeResponsable { get; set; }

        [Display(Name = "Type de travail")]
        public string? TypeTravail { get; set; }

        [Display(Name = "Date début")]
        public DateOnly? DateDebut { get; set; }

        [Display(Name = "Date fin")]
        public DateOnly? DateFin { get; set; }

        [Display(Name = "Temps alloué (min)")]
        public int? TempsAlloueMinutes { get; set; }

        [Display(Name = "Temps passé — Systématique (min)")]
        public int? TempsPasseSystematiqueMinutes { get; set; }

        [Display(Name = "Temps passé — Retouche (min)")]
        public int? TempsPasseRetoucheMinutes { get; set; }

        [Display(Name = "Vieillissement (heures)")]
        public int? VieillissementHours { get; set; }

        [StringLength(1000)]
        [Display(Name = "Directives de l'autorité responsable")]
        public string? Directives { get; set; }

        [StringLength(200)]
        [Display(Name = "Référence T.O.")]
        public string? TechnicalOrderReference { get; set; }

        [StringLength(100)]
        [Display(Name = "Directive émise par")]
        public string? DirectiveIssuedByName { get; set; }

        [Display(Name = "Statut")]
        public string Status { get; set; } = "PENDING";

        public List<WorkSectionLookupViewModel> AvailableSections { get; set; } = [];

        public static readonly string[] StatusOptions = ["PENDING", "IN_PROGRESS", "DONE"];
        public static readonly string[] TypeTravailOptions = ["DEP", "VP", "MOD"];
    }

    public class WorkOrderSectionListItemViewModel
    {
        public int Id { get; set; }
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string? FormNumber { get; set; }
        public string? TypeTravail { get; set; }
        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }
        public string Status { get; set; } = "PENDING";
    }

    public class WorkOrderSectionDetailsViewModel
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string WONumber { get; set; } = string.Empty;
        public string AircraftLabel { get; set; } = string.Empty;

        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;

        public string? FormNumber { get; set; }
        public string? OrganismeResponsable { get; set; }
        public string? TypeTravail { get; set; }
        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }

        public int? TempsAlloueMinutes { get; set; }
        public int? TempsPasseSystematiqueMinutes { get; set; }
        public int? TempsPasseRetoucheMinutes { get; set; }

        public int? VieillissementHours { get; set; }

        public string? Directives { get; set; }
        public string? TechnicalOrderReference { get; set; }
        public string? DirectiveIssuedByName { get; set; }
        public DateTime? DirectiveIssuedAtUtc { get; set; }

        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        // Placeholders for the follow-up slices — empty for now.
        // public List<WorkOrderSectionPartItemViewModel> Parts { get; set; } = [];
        // public List<WorkOrderSectionTaskItemViewModel> Tasks { get; set; } = [];
        // public List<WorkOrderSectionSignOffItemViewModel> SignOffs { get; set; } = [];
    }

    public class WorkSectionLookupViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayLabel => $"{Code} — {Name}";
    }
}