using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    [Table("AcStatusTypes", Schema = "dbo")]
    public class AcStatusType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? StatusCode { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        public byte SortOrder { get; set; } = 99;
        public bool IsActive { get; set; } = true;  

        public ICollection<Aircraft> Aircrafts { get; set; } = new HashSet<Aircraft>();
    }
}