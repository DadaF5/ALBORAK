using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class AcType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Max Grossweight is required")]
        public double MaxGrossweight { get; set; }
        public int MaxPassengers { get; set; }

        [Required(ErrorMessage = "Max Engines is required")]
        public int MaxEngines { get; set; } = 1;

        // --------------------------
        // Foreign Key to AcMainGroup
        // --------------------------
        [Required(ErrorMessage = "Main Group is required")]
        public int AcMainGroupId { get; set; }
        public AcMainGroup AcMainGroup { get; set; }

        // Optional navigation: Aircraft under this type
        public ICollection<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        public ICollection<Sortie> Sorties { get; set; } = new List<Sortie>();
    }
}

