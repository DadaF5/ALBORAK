using FRAProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    public class SortieCrewCreateVm
    {
        public int Id { get; set; }  //for Edit operations
        public int SortieId { get; set; }

        [Required]
        [Display(Name = "Crew Member")]
        public int CrewMemberId { get; set; }

        [Required]
        [Display(Name = "Seat")]
        public CrewSeat Seat { get; set; }

        [Required]
        [Display(Name = "Aircraft Role")]
        public AircraftRole AircraftRole { get; set; }

        [Display(Name = "Role (Optional)")]
        [StringLength(50)]
        public string? Role { get; set; }

        [Display(Name = "Is Primary")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(500)]
        public string? Remarks { get; set; }

        // For display purposes
        public string? CrewMemberName { get; set; }
        public string? SortieCode { get; set; }
    }
}
