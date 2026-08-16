// Areas/AircraftMaintenance/ViewModels/FleetStatusRowVm.cs
namespace FRAProject.Areas.AircraftMaintenance.ViewModels
{
    public class FleetStatusRowVm
    {
        public int AircraftId { get; set; }
        public string Registration { get; set; } = string.Empty;
        public int TailNo { get; set; }
        public string AcTypeCode { get; set; } = string.Empty;
        public string AcTypeName { get; set; } = string.Empty;
        public bool Serviceable { get; set; }
        public string StatusCode { get; set; } = "—";
        public string StatusName { get; set; } = "—";

        public string ServiceableBadgeClass => Serviceable ? "badge-success" : "badge-warning";
        public string ServiceableLabel => Serviceable ? "Serviceable" : "En Maintenance";
    }
}