using System.Reflection;

namespace FRAProject.Areas.SquadronOps.Models
{
    public class Phase
    {
        public int Id { get; set; }

        // You use BaseName style for other entities; Phase uses Name here for clarity
        public string Name { get; set; } = "";

        // optional localized or descriptive name
        public string? Description { get; set; }

        // Navigation
        public ICollection<Mission> Missions { get; set; } = new HashSet<Mission>();
    }
}
