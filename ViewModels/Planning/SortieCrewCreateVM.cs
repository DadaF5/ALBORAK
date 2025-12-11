using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels.Planning
{
    public class SortieCrewCreateVM
    {
        public int SortieId { get; set; }
        public int SelectedCrewMemberId { get; set; }
        public string Role { get; set; } = "Pilot";

        // For view
        public IEnumerable<SelectListItem>? CrewOptions { get; set; }
    }
}
