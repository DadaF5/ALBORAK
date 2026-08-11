using FRAProject.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // One row per responsible section's Formule 13 completion record,
    // under a parent WorkOrder (= Formule 12). Real form fields, per
    // scanned examples: N° Formule (e.g. "25128/INST"), Organisme
    // responsable ("SGMA1"), Type de travail ("DEP"/"VP"/"MOD"), dates,
    // allocated/spent time (Systématique + Retouche), aircraft aging at
    // time of this section's work, directives from the responsible
    // authority + T.O. reference.
    //
    // Tableau II (equipment changed), Tableau III (travaux effectués),
    // and the final 4-level sign-off chain are separate entities
    // (WorkOrderSectionPart / WorkOrderSectionTask / WorkOrderSectionSignOff)
    // — built as follow-up slices, referenced here via collections once
    // they exist.
    public class WorkOrderSection
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public int WorkSectionId { get; set; }
        public WorkSection? WorkSection { get; set; }

        public string? FormNumber { get; set; }              // e.g. "25128/INST"
        public string? OrganismeResponsable { get; set; }     // e.g. "SGMA1"
        public string? TypeTravail { get; set; }              // e.g. "DEP" | "VP" | "MOD"

        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }

        public int? TempsAlloueMinutes { get; set; }
        public int? TempsPasseSystematiqueMinutes { get; set; }
        public int? TempsPasseRetoucheMinutes { get; set; }

        public int? VieillissementHours { get; set; }         // aircraft hours at time of this section's work

        public string? Directives { get; set; }               // Tableau I free text
        public string? TechnicalOrderReference { get; set; }  // e.g. "IAW TO 1F-5F-2-8-1-1 ed: 30.3.2006"

        public string? DirectiveIssuedByName { get; set; }    // Tableau I signature (e.g. "Chef ST")
        public DateTime? DirectiveIssuedAtUtc { get; set; }

        public string Status { get; set; } = "PENDING";       // PENDING | IN_PROGRESS | DONE

        public string? OpenedByUserId { get; set; }
        public ApplicationUser? OpenedByUser { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public static readonly string[] StatusOptions = ["PENDING", "IN_PROGRESS", "DONE"];
        public static readonly string[] TypeTravailOptions = ["DEP", "VP", "MOD"];
    }
}