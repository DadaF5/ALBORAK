using FRAProject.Areas.Settings.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class JobCard
    {
        public int Id { get; set; }

        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }

        public string? AtaCode { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? Specialty { get; set; } // MECA | AVION | ELEC | STRUCT | APG | OTHER
        public int AllocatedTimeMinutes { get; set; }

        // ── Added fields — confirmed against real TO XX1F-5E-6WC-3 card ──
        // Additive only, all nullable — no impact on existing rows.
        public string? WorkAreas { get; set; }              // e.g. "1" — card's WORK AREA(S)
        public int? MechNo { get; set; }                    // number of mechanics required
        public string? ElectricalPowerRequired { get; set; } // ON | OFF | NA
        public string? FigureRef { get; set; }               // illustration ref, e.g. "N1-M143"

        public string? ToReference { get; set; }
        public string? DocReference { get; set; }
        public string? Edition { get; set; }
        public int? ChangeNo { get; set; }
        public DateOnly? ChangeDate { get; set; }

        public int SortOrder { get; set; } = 100;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public string DisplayLabel => $"{CardCode} — {Title}";

        public ICollection<ProgramJobCard> ProgramJobCards { get; set; } = [];
        public ICollection<JobCardPlanningRule> PlanningRules { get; set; } = [];
        public ICollection<JobCardAttachment> Attachments { get; set; } = [];
        public ICollection<AircraftJobCardState> AircraftJobCardStates { get; set; } = [];
        public ICollection<WorkOrderJobCard> WorkOrderJobCards { get; set; } = [];

        // ── FUTURE — deferred, not built yet ──────────────────────────────
        // Real cards show a per-instruction-line breakdown (ManMin, WorkArea,
        // WorkUnitCode SYS/SUB) — e.g. 4 separate lines each with their own
        // values. That's a one-to-many child entity (JobCardStep), not a flat
        // field on JobCard. Add when the paper-card replication becomes a
        // priority — not required for current CRUD/scheduling scope.
        // public ICollection<JobCardStep> Steps { get; set; } = [];
    }
}