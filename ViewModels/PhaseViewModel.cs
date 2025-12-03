using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class PhaseViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Phase Name")]
        public string Name { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
