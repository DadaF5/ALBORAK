using FRAProject.Areas.AircraftMaintenance.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.HR.Models
{
    [Table("Bases")]
    public class Base
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string BaseName { get; set; }

        [StringLength(100)]
        public string? BaseNameLocal { get; set; }

        // Navigation
        public ICollection<Department> Departments { get; set; } = new HashSet<Department>();
        public ICollection<AcMainGroup> AcMainGroups { get; set; } = new HashSet<AcMainGroup>();
      

    }
}
