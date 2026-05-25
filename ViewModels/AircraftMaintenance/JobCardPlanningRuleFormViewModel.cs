using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardPlanningRuleFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Job Card")]
        public int JobCardId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Rule Name")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Condition Text")]
        public string? ConditionText { get; set; }

        [Display(Name = "Applicable")]
        public bool IsApplicable { get; set; } = true;

        [Display(Name = "Initial Hours")]
        public int? InitialHours { get; set; }

        [Display(Name = "Initial Cycles")]
        public int? InitialCycles { get; set; }

        [Display(Name = "Initial Calendar Value")]
        public int? InitialCalendarValue { get; set; }

        [StringLength(10)]
        [Display(Name = "Initial Calendar Unit")]
        public string? InitialCalendarUnit { get; set; }

        [Display(Name = "Recurring Hours")]
        public int? RecurringHours { get; set; }

        [Display(Name = "Recurring Cycles")]
        public int? RecurringCycles { get; set; }

        [Display(Name = "Recurring Calendar Value")]
        public int? RecurringCalendarValue { get; set; }

        [StringLength(10)]
        [Display(Name = "Recurring Calendar Unit")]
        public string? RecurringCalendarUnit { get; set; }

        [Display(Name = "Manufacturer Serial From")]
        public int? ManufacturerSerialFrom { get; set; }

        [Display(Name = "Manufacturer Serial To")]
        public int? ManufacturerSerialTo { get; set; }

        [StringLength(100)]
        [Display(Name = "Required Compliance Code")]
        public string? RequiredComplianceCode { get; set; }

        [StringLength(100)]
        [Display(Name = "Forbidden Compliance Code")]
        public string? ForbiddenComplianceCode { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<LookupOptionViewModel> JobCards { get; set; } = [];
        public List<string> CalendarUnitOptions { get; set; } = ["DAY", "MONTH", "YEAR"];
    }
}