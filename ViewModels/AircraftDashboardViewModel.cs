using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FRAProject.ViewModels
{
    public class AircraftDashboardViewModel
    {
        public List<SelectListItem> Bases { get; set; } = new();
        public int? SelectedBaseId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
        public int? SelectedCategoryId { get; set; }

        public List<AircraftStatusDashboardViewModel> Data { get; set; } = new();
    }
}
