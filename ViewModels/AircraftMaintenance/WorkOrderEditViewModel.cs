using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class WorkOrderEditViewModel
    {
        public int Id { get; set; }
        public string WONumber { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Remarques")]
        public string? Remarks { get; set; }
    }
}