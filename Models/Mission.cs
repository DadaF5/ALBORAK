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

        // optional planned date (not required)
        public DateTime? PlannedDate { get; set; }

        // renamed from Notes -> Description
        public string? Description { get; set; }

        // Navigation
        public ICollection<Odv>? Odvs { get; set; }


    }
}
