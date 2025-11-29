using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class Wing
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [Required, StringLength(60)]
        [Display(Name = "Wing Long Name")]
        public string WingLong { get; set; }

        // FK to Department
        [Required]
        public int DepartmentId { get; set; }
        [Display(Name = "Department")]
        public Department Department { get; set; }

        public bool Active { get; set; } = true;

        // AcMainGroup is OPTIONAL
        [Display(Name = "Main Group")]
        public int? AcMainGroupId { get; set; }  // Nullable for optional relationship        
        // Navigation property
        public AcMainGroup? AcMainGroup { get; set; }


        // Navigation property: Wing has many Squadrons
        public ICollection<Squadron>? Squadrons { get; set; }

        [NotMapped]
        public string FullName => $"{Name} ({Department?.Name})";
    }
}
