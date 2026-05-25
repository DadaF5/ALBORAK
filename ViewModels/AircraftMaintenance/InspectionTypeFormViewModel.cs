using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Kind")]
        public string Kind { get; set; } = "PLANNED";

        [Display(Name = "Interval Hours")]
        public int? IntervalHours { get; set; }

        [Display(Name = "Interval Cycles")]
        public int? IntervalCycles { get; set; }

        [Display(Name = "Calendar Value")]
        public int? CalendarValue { get; set; }

        [StringLength(10)]
        [Display(Name = "Calendar Unit")]
        public string? CalendarUnit { get; set; }

        [Display(Name = "Tolerance Hours")]
        public int? ToleranceHours { get; set; }

        [Display(Name = "Tolerance Cycles")]
        public int? ToleranceCycles { get; set; }

        [Display(Name = "Tolerance Calendar Value")]
        public int? ToleranceCalendarValue { get; set; }

        [StringLength(10)]
        [Display(Name = "Tolerance Calendar Unit")]
        public string? ToleranceCalendarUnit { get; set; }

        [Display(Name = "Next Inspection Type")]
        public int? NextInspectionTypeId { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<AcTypeLookupViewModel> AcTypes { get; set; } = [];
        public List<LookupOptionViewModel> NextInspectionTypes { get; set; } = [];
        public List<string> KindOptions { get; set; } = ["PLANNED"];
        public List<string> CalendarUnitOptions { get; set; } = ["DAY", "MONTH", "YEAR"];
    }
}