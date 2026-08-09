using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderFormViewModel
    {
        public int? Id { get; set; }

        [StringLength(20)]
        [Display(Name = "Work Order Number")]
        public string WONumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Aircraft")]
        public int AircraftId { get; set; }

        // ── Replaces the old singular InspectionTypeId ──────────────────
        // A single WorkOrder (one dock visit) can satisfy several
        // coinciding periodic inspections at once (e.g. PE1+PE2+PE4 all
        // due at 1200h — see WorkOrderInspectionType junction).
        [Display(Name = "Inspection Types")]
        public List<int> SelectedInspectionTypeIds { get; set; } = [];

        [Required]
        [StringLength(5)]
        [Display(Name = "WO Type")]
        public string WOType { get; set; } = "F12";

        [Required]
        [StringLength(20)]
        [Display(Name = "WO Kind")]
        public string WOKind { get; set; } = "PLANNED";

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "DRAFT";

        [Display(Name = "Open Hours")]
        public int OpenHours { get; set; }

        [Display(Name = "Open Cycles")]
        public int OpenCycles { get; set; }

        [Display(Name = "Open Date")]
        public DateOnly OpenDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Close Hours")]
        public int? CloseHours { get; set; }

        [Display(Name = "Close Cycles")]
        public int? CloseCycles { get; set; }

        [Display(Name = "Close Date")]
        public DateOnly? CloseDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<AircraftLookupViewModel> Aircrafts { get; set; } = [];
        public List<LookupOptionViewModel> InspectionTypes { get; set; } = [];

        // Used by the Create view's checkbox list + AcType filtering JS.
        // Additive — InspectionTypes above is left populated too, unused
        // by Create.cshtml now but kept in case anything else reads it.
        public List<InspectionTypeCheckItemViewModel> InspectionTypeItems { get; set; } = [];

        public List<string> WOTypeOptions { get; set; } = ["F11", "F12"];

        // Extended to include CORRECTIVE — the original list only had
        // PLANNED, but WOKind's own model comment already allowed both.
        public List<string> WOKindOptions { get; set; } = ["PLANNED", "CORRECTIVE"];

        public List<string> StatusOptions { get; set; } = ["DRAFT", "OPEN", "IN_PROGRESS", "CLOSED"];
    }
}