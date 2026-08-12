namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Tableau III ("Travaux Effectués — Systématiques - Retouches -
    // Modifications") from a real Formule 13 — one row per task performed.
    // Real entries are often multi-line free text with embedded T.O.
    // paragraph references (e.g. "1F.SF.2.8.1.1 AIR SPEED MACH INDICATOR
    // INSTALLED"), sometimes referencing a message/extension number
    // instead of a T.O. paragraph — kept as free text rather than trying
    // to structurally parse the reference format, which varies.
    public class WorkOrderSectionTask
    {
        public int Id { get; set; }

        public int WorkOrderSectionId { get; set; }
        public WorkOrderSection? WorkOrderSection { get; set; }

        public string DesignationTravaux { get; set; } = string.Empty;

        public int? TempsAlloueMinutes { get; set; }
        public DateOnly? Date { get; set; }

        // Réel form splits "Temps passé" into Système (systematic/routine
        // work) and Retouches (rework/touch-up) — matches
        // WorkOrderSection's own TempsPasse* split at the header level,
        // just per-task here instead of per-section total.
        public int? TempsPasseSystemeMinutes { get; set; }
        public int? TempsPasseRetouchesMinutes { get; set; }

        public string? ExecutantSpecial { get; set; } // trade/specialty code (e.g. "166", "177")
        public string? ExecutantNom { get; set; }
        public DateTime? ExecutantSignedAtUtc { get; set; } // electronic attestation timestamp — no image capture

        public int SortOrder { get; set; } = 100;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}