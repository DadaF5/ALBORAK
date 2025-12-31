using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    // CrewMember is an aircrew profile connected 1:1 to Person.
    public class CrewMember
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Sequence No")]
        public int? SequenceNo { get; set; }

        // Captain (Full Name used operationally)
        [Required(ErrorMessage = "Captain name is required.")]
        [StringLength(30, ErrorMessage = "Captain name cannot exceed 30 characters.")]
        [Display(Name = "Captain")]
        public string Captain { get; set; } = string.Empty;

        // Radio or cockpit nickname
        [Required(ErrorMessage = "Nickname is required.")]
        [StringLength(10, ErrorMessage = "Nickname cannot exceed 10 characters.")]
        [Display(Name = "Nickname")]
        public string NickName { get; set; } = string.Empty;

        // Role as a crew member
        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        [Display(Name = "Role")]
        public string? Role { get; set; }

        // Relative path to photo file
        [StringLength(255, ErrorMessage = "Photo path cannot exceed 255 characters.")]
        [Display(Name = "Photo")]
        public string? Photo { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; } = true;

        [Display(Name = "Mobile (Operational)")]
        public string? Mobile { get; set; } // changed to string to store international numbers

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Ready";

        [Display(Name = "Allowed To Sign")]
        public bool AllowedToSign { get; set; } = false;

        [StringLength(20, ErrorMessage = "Crew Member Type cannot exceed 20 characters.")]
        [Display(Name = "Crew Member Type")]
        public string CrewMemberType { get; set; } = "Pilot";

        // ----------------------
        // Foreign Keys & Navigation
        // ----------------------      

        [Required(ErrorMessage = "Squadron selection is required.")]
        [Display(Name = "Squadron")]
        public int SquadronId { get; set; }
        public Squadron? Squadron { get; set; }


        [Required(ErrorMessage = "Person information is required.")]
        [Display(Name = "Person")]
        public int PersonId { get; set; }
        public Person? Person { get; set; }

        // Optional primary qualification FK (if you want to store a single 'main' qualification)
        [Display(Name = "Primary Qualification")]
        public int? PrimaryQualificationId { get; set; }
        [ForeignKey(nameof(PrimaryQualificationId))]
        public Qualification? PrimaryQualification { get; set; }

        // All qualifications for the crew member (history/current)
        public ICollection<CrewMemberQualification> CrewMemberQualifications { get; set; } = new List<CrewMemberQualification>();
        public ICollection<MedicalCheck> MedicalChecks { get; set; } = new List<MedicalCheck>();
        // =============================================
        // Relationships
        // =============================================
        // Helper Properties for easier access to related data
        [NotMapped]
        public MedicalCheck? LatestMedicalCheck => MedicalChecks
            .OrderByDescending(mc => mc.CheckDate)
            .FirstOrDefault();

        [NotMapped]
        public DateTime? MedicalExpiry => LatestMedicalCheck?.NextDueDate;

        [NotMapped]
        public int? DaysToMedicalExpiry => DaysToMedicalExpiry.HasValue ?
            (int?)(MedicalExpiry.Value-DateTime.Today).Days : null;

        [NotMapped]
        public List<MedicalBilan> PendingBilans => MedicalChecks
            .SelectMany(mc => mc.Bilans)
            .Where(bilan => !bilan.IsCompleted)
            .ToList();

        //Audit
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}