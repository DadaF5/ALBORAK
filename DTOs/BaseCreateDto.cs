using System.ComponentModel.DataAnnotations;

namespace FRAProject.DTOs
{
    public class BaseCreateDto
    {
        public int Id { get; set; }   // <-- REQUIRED FOR EDIT

        [Required(ErrorMessage = "Base name is required")]
        [StringLength(100)]
        public string BaseName { get; set; }

        [StringLength(100)]
        public string? BaseNameLocal { get; set; }
    }
}