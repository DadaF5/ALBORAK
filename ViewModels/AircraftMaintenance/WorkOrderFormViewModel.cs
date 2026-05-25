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

        [Required]
        [Display(Name = "Inspection Type")]
        public int InspectionTypeId { get; set; }

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

        public List<string> WOTypeOptions { get; set; } = ["F11", "F12"];
        public List<string> WOKindOptions { get; set; } = ["PLANNED"];
        public List<string> StatusOptions { get; set; } = ["DRAFT", "OPEN", "IN_PROGRESS", "CLOSED"];
    }
}