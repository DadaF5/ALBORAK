using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class AcCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AcCategoryId")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(20)]       
        public string Name { get; set; } // e.g., "Fighter", "Transport", "Training"

        [StringLength(100)]
        public string Description { get; set; }

        // Navigation property
        public ICollection<AcMainGroup> AcMainGroups { get; set; } = new HashSet<AcMainGroup>();
    }
}
