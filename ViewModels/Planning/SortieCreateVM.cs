using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.Planning
{
    public class SortieCreateVM
    {
        public int? Id { get; set; }
        [Required] public int OdvId { get; set; }
        public string? Callsign { get; set; }
        public TimeSpan? PlannedTOFF { get; set; }
        public string? Configuration { get; set; }
        public decimal? FuelQuantity { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem>? AircraftOptions { get; set; }
    }
}
