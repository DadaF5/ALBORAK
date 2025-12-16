using FRAProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class SortieCrewCreateVm
    {
        public int SortieId { get; set; }

        [Required]
        public int CrewMemberId { get; set; }

        [Required]
        public CrewSeat Seat { get; set; }

        [Required]
        public AircraftRole AircraftRole { get; set; }

        public bool IsPrimary { get; set; }
    }
}
