using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("AircraftVersions", Schema = "dbo")]
    public class AircraftVersion : LookupBase
    {
        // Foreign Key to AcType (parent - existing table)
        [Required]
        public int AcTypeId { get; set; }

        // Navigation property
        [ForeignKey("AcTypeId")]
        public virtual AcType? AcType { get; set; }
    }
}