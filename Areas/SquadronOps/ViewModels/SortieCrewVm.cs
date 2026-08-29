namespace FRAProject.Areas.SquadronOps.ViewModels
{
    public class SortieCrewVm
    {
        // Selected crew member id from dropdown (can be CrewMember.Id or Person.Id depending on your UI)
        public int CrewMemberId { get; set; }

        // Role in the sortie e.g. "Pilot", "Copilot", "Observer"
        public string? Role { get; set; }

        // Mark this crew member as primary for their role
        public bool IsPrimary { get; set; }

        // Optional free text remarks for this assignment
        public string? Remarks { get; set; }
    }
}
