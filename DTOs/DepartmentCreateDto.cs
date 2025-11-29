using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class DepartmentCreateDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100)]
        public string Name { get; set; }        

        [Required(ErrorMessage = "Base is required")]
        public int BaseId { get; set; }

        [StringLength(150)]
        public string? Description { get; set; }
    }
}
