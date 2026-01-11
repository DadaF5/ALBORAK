using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.HR.Models
{
    public class Rank
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; }

        [Required, StringLength(150)]
        public string FullRank { get; set; }

        [Required]
        public int Sequence { get; set; }

        // FK -> RankType
        [Required]
        public int RankTypeId { get; set; }
        public RankType RankType { get; set; }

        // Navigation
        public ICollection<Person> Persons { get; set; } = new HashSet<Person>();
    }
}
