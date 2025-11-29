
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class AcStatusType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Status Name")]
        public string StatusName { get; set; } // e.g., Active, Maintenance, Retired

        [StringLength(100)]
        public string? Description { get; set; }

        public ICollection<Aircraft>? Aircraft { get; set; }
    }

}
