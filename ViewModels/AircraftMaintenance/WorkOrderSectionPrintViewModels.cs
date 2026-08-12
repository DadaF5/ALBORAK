namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderSectionPrintViewModel
    {
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

        public List<WorkOrderSectionPartPrintItemViewModel> Parts { get; set; } = [];
        public List<WorkOrderSectionTaskPrintItemViewModel> Tasks { get; set; } = [];
        public List<WorkOrderSectionSignOffItemViewModel> SignOffs { get; set; } = [];
    }

    public class WorkOrderSectionPartPrintItemViewModel
    {
        public string? OldNomenclature { get; set; }
        public string? OldNumero { get; set; }
        public string? OldVieillissement { get; set; }
        public string? NewNomenclature { get; set; }
        public string? NewNumero { get; set; }
        public string? NewVieillissement { get; set; }
        public string? DesignationEtPosition { get; set; }
        public string? MotifDepose { get; set; }
        public string? Symbole { get; set; }
        public int? TempsAlloueMinutes { get; set; }
        public DateOnly? Date { get; set; }
        public int? TempsPasseMinutes { get; set; }
        public string? ExecutantSpecial { get; set; }
        public string? ExecutantNom { get; set; }
    }

    public class WorkOrderSectionTaskPrintItemViewModel
    {
        public string DesignationTravaux { get; set; } = string.Empty;
        public int? TempsAlloueMinutes { get; set; }
        public DateOnly? Date { get; set; }
        public int? TempsPasseSystemeMinutes { get; set; }
        public int? TempsPasseRetouchesMinutes { get; set; }
        public string? ExecutantSpecial { get; set; }
        public string? ExecutantNom { get; set; }
    }
}
