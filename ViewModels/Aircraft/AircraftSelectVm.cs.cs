using FRAProject.Models;

namespace FRAProject.ViewModels
{
    public class AircraftItemVm
    {
        public int Id { get; set; }
        public string? Registration { get; set; }
        public string? AcType { get; set; }
        public AircraftStatus Status { get; set; }
    }

    public class AircraftSelectVm
    {
        public int SortieId { get; set; }
        public List<AircraftItemVm> Aircrafts { get; set; } = new List<AircraftItemVm>();
    }
}
