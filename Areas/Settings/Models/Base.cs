using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("Bases")]
    public class Base
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string BaseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BaseName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Geo Coordinates
        [Column(TypeName = "decimal(10, 7)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(10, 7)")]
        public decimal? Longitude { get; set; }

        // Navigation
        public ICollection<Department> Departments { get; set; } = new HashSet<Department>();
        public ICollection<AcMainGroup> AcMainGroups { get; set; } = new HashSet<AcMainGroup>();
        public ICollection<Aircraft> Aircraft { get; set; } = new HashSet<Aircraft>();


    }
}
