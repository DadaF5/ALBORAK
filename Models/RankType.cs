using System.ComponentModel.DataAnnotations;

namespace FRAProject.Models
{
    public class RankType
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Rank> Ranks { get; set; } = new HashSet<Rank>();
    }
}
