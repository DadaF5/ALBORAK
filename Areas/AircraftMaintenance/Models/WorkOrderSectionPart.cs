namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Tableau II ("Équipements changés ou remis en état") from a real
    // Formule 13 — one row per equipment exchange event. The paper form
    // alternates "Ancien"/"Nouveau" as two rows, but that's a single
    // transaction (old part removed, new part installed) — modeled here
    // as one row with old/new fields together, matching the cleaner
    // side-by-side recap layout seen on the Formule 12.
    public class WorkOrderSectionPart
    {
        public int Id { get; set; }

        public int WorkOrderSectionId { get; set; }
        public WorkOrderSection? WorkOrderSection { get; set; }

        // ── Old (removed) part ──────────────────────────────────────────
        public string? OldNomenclature { get; set; }
        public string? OldNumero { get; set; }
        public string? OldVieillissement { get; set; } // free text — component's own accumulated time, format varies

        // ── New (installed) part ────────────────────────────────────────
        public string? NewNomenclature { get; set; }
        public string? NewNumero { get; set; }
        public string? NewVieillissement { get; set; }

        public string? DesignationEtPosition { get; set; } // what the part is / where it's fitted
        public string? MotifDepose { get; set; }            // reason for removal
        public string? Symbole { get; set; }                // "(1)" column — S (suivi) | F (fiche matricule)

        public int? TempsAlloueMinutes { get; set; }
        public DateOnly? Date { get; set; }
        public int? TempsPasseMinutes { get; set; }

        public string? ExecutantSpecial { get; set; } // trade/specialty code (e.g. "156", "177")
        public string? ExecutantNom { get; set; }
        public DateTime? ExecutantSignedAtUtc { get; set; } // electronic attestation timestamp — no image capture

        public int SortOrder { get; set; } = 100;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}