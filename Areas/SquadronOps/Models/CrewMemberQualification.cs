using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.Models
{
    public class CrewMemberQualification
    {
        [Key]
        public int Id { get; set; }

        // FK to CrewMember
        [Required]
        [Display(Name = "Crew Member")]
        public int CrewMemberId { get; set; }
        public CrewMember? CrewMember { get; set; }

        // FK to Qualification
        [Required]
        [Display(Name = "Qualification")]
        public int QualificationId { get; set; }
        public Qualification? Qualification { get; set; }

        [Display(Name = "Valid From")]
        public DateTime? ValidFrom { get; set; }

        [Display(Name = "Valid Until")]
        public DateTime? ValidUntil { get; set; }

        [StringLength(100)]
        [Display(Name = "Issued By")]
        public string? IssuedBy { get; set; }

        [StringLength(255)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(20)]
        [Display(Name = "Status")]
        public string? Status { get; set; }  // Active, Expired, Suspended, etc.      

        // convenience helper
        public bool IsCurrent => !ValidUntil.HasValue || ValidUntil.Value.Date >= DateTime.UtcNow.Date;
    }
}