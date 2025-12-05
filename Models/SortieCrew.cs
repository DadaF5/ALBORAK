namespace FRAProject.Models
{
    // assignment of a Person to a Sortie (1..* crew members per sortie)
    public class SortieCrew
    {
        public int SortieCrewId { get; set; } // surrogate PK

        public int SortieId { get; set; }
        public Sortie? Sortie { get; set; }

        public int PersonId { get; set; } // crew member (Person)
        public Person? Person { get; set; }

        public string? Role { get; set; } // e.g., "PIC", "SIC", "Crewman", "Loadmaster"
        public bool IsPrimary { get; set; } = false;
    }
}