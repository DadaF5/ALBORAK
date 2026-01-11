using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.HR.Models
{
    public class SubDepartment
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // FK
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // Navigation
        public ICollection<Person> Persons { get; set; } = new HashSet<Person>();
    }
}
