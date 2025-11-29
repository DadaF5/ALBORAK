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

        // --------------------------
        // Foreign Key to AcMainGroup
        // --------------------------
        [Required(ErrorMessage = "Main Group is required")]
        public int AcMainGroupId { get; set; }
        public AcMainGroup AcMainGroup { get; set; }

        // Optional navigation: Aircraft under this type
        public ICollection<Aircraft> Aircrafts { get; set; } = new HashSet<Aircraft>();
    }
}

