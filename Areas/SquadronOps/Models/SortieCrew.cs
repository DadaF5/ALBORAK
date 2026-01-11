using FRAProject.Enums;

namespace FRAProject.Areas.SquadronOps.Models
{
    // assignment of a Person to a Sortie (1..* crew members per sortie)
    public class SortieCrew
    {
        public int Id { get; set; }

        public int SortieId { get; set; }
        public Sortie? Sortie { get; set; }

        // Reference to existing CrewMember entity (adjust type if you store Person instead)
        public int CrewMemberId { get; set; }
        public CrewMember? CrewMember { get; set; }

        public CrewSeat Seat { get; set; }

        // Role e.g. "Pilot", "Copilot", "Observer"
        public string? Role { get; set; }

        // Mark primary performer for this role if required
        public bool IsPrimary { get; set; } = false;

        public string? Remarks { get; set; }
        public AircraftRole AircraftRole { get; internal set; }
    }
}