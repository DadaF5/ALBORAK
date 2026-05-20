

using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{

    public enum AircraftStatus
    {
        Available = 0,
        Assigned = 10,      // assigned to a sortie but still on ground
        Airborne = 20,      // airborne (TWR should release back to Available)
        Unserviceable = 30  // aircraft declared unserviceable / maintenance
    }
    public class AircraftItemVm
    {
        public int Id { get; set; }
        public string? Registration { get; set; }
        public string? AcType { get; set; }

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;
        public AircraftStatus Status { get; set; }
    }

    public class AircraftSelectVm
    {
        public int SortieId { get; set; }       
        public List<AircraftItemVm> Aircrafts { get; set; } = new List<AircraftItemVm>();
    }
}
