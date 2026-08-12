namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string WONumber { get; set; } = string.Empty;

        public int AircraftId { get; set; }
        public string AircraftLabel { get; set; } = string.Empty;
        public int AcTypeId { get; set; }

        // Additive — used by the Print view header (static aircraft data)
        public string? AircraftSerialNumber { get; set; }
        public string? AircraftIntCode { get; set; }
        public int AircraftTailNo { get; set; }
        public string AcTypeLabel { get; set; } = string.Empty;
        public string? ManufacturerLabel { get; set; }
        public int MaxEngines { get; set; }

        // Replaces singular InspectionTypeId/InspectionTypeLabel
        public List<string> InspectionTypeLabels { get; set; } = [];

        public string WOType { get; set; } = string.Empty;
        public string WOKind { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int OpenHours { get; set; }
        public int OpenCycles { get; set; }
        public int OpenLandings { get; set; }
        public DateOnly OpenDate { get; set; }

        public int? CloseHours { get; set; }
        public int? CloseCycles { get; set; }
        public int? CloseLandings { get; set; }
        public DateOnly? CloseDate { get; set; }

        public string? OpenedByUserName { get; set; }
        public string? ClosedByUserName { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public List<WorkOrderJobCardItemViewModel> JobCards { get; set; } = [];

        // Additive — populated only for Print (Details/Delete don't need
        // this, avoids unnecessary querying on every page load).
        public List<WorkOrderSectionPrintViewModel> Sections { get; set; } = [];

        // ── Workflow helpers (additive — used by Details.cshtml to show/
        // hide action buttons) ──────────────────────────────────────────
        public bool CanOpen => Status == "DRAFT";
        public bool CanPopulateJobCards => Status == "OPEN" || Status == "IN_PROGRESS";
        public bool CanClose => Status == "OPEN" || Status == "IN_PROGRESS";
        public bool CanDelete => Status == "DRAFT";

        public bool AllMandatoryDone => JobCards
            .Where(jc => jc.IsMandatory)
            .All(jc => jc.Status == "DONE" || jc.Status == "N_A");
    }
}