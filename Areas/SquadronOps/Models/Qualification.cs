using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.Models
{
    public class Qualification
    {
        [Key]
        public int Id { get; set; }   // matches FK fields

        [Required(ErrorMessage = "Qualification name is required.")]
        [StringLength(100, ErrorMessage = "Qualification name cannot exceed 100 characters.")]
        [Display(Name = "Qualification Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Qualification type is required.")]
        [StringLength(20, ErrorMessage = "Qualification type cannot exceed 20 characters.")]
        [Display(Name = "Qualification Type")]
        public string QualificationType { get; set; } = "Other";   // Military / Civilian / Other

        [Display(Name = "Active")]
        public bool Active { get; set; } = true;

        // navigation
        public ICollection<CrewMemberQualification> CrewMemberQualifications { get; set; } = new List<CrewMemberQualification>();
    }
}