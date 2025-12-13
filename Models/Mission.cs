namespace FRAProject.Models
{
    public class Mission
    {
        public int Id { get; set; }

        // mission title
        public string Name { get; set; } = "";

        // optional code/reference (useful to make unique within Phase)
        public string? Code { get; set; }

        // FK to Phase (each Mission belongs to a Phase)
        public int PhaseId { get; set; }
        public Phase? Phase { get; set; }

        // 🔑 VERY IMPORTANT
        // NULL = Global mission
        // NOT NULL = Squadron-specific mission
        public int? SquadronId { get; set; }
        public Squadron? Squadron { get; set; }

        // optional planned date (not required)
        public DateTime? PlannedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // renamed from Notes -> Description
        public string? Description { get; set; }

        // Navigation - if you plan to serialize entities directly, avoid serializing collections
        public ICollection<Odv>? Odvs { get; set; } = new List<Odv>();




    }
}
