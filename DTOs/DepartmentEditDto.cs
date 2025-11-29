using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class DepartmentEditDto
    {
        [Required]
        public int Id { get; set; } // Required to identify which Department to edit

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(150, ErrorMessage = "Description cannot exceed 150 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Base is required")]
        [Display(Name = "Base")]
        public int BaseId { get; set; }
    }
}
