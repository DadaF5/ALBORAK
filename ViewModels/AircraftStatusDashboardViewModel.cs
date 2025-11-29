
using System.Collections.Generic;

namespace FRAProject.ViewModels
{
    public class AircraftStatusDashboardViewModel
    {
        public int BaseId { get; set; }
        public string BaseName { get; set; }
        public List<AcCategoryStatus> Categories { get; set; } = new();
    }
}