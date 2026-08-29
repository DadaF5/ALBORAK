using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FRAProject.Areas.HR.Models
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }

        // Foreign Key to Base
        [Required]
        [Display(Name = "Base")]
        public int BaseId { get; set; }

        [ForeignKey("BaseId")]
        public Base Base { get; set; }

        // Navigation to SubDepartments
        public ICollection<SubDepartment> SubDepartments { get; set; } = new HashSet<SubDepartment>();
        [JsonIgnore]
        public ICollection<Wing> Wings { get; set; } = new HashSet<Wing>();
    }
}
