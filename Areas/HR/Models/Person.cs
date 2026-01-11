using FRAProject.Areas.SquadronOps.Models;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.HR.Models
{
    /// <summary>
    /// DOMAIN: HR (Human Resources)
    /// Represents an employee/person in the organization.
    /// Educational Purpose: Core HR entity that links to organizational structure (Department/SubDepartment)
    /// and can optionally link to CrewMember for flight operations personnel.
    /// 
    /// Key Relationships:
    /// - Person → Rank (Many-to-One): Each person has a military/organizational rank
    /// - Person → SubDepartment → Department → Base (Many-to-One chain): Organizational hierarchy
    /// - Person → CrewMember (One-to-One, optional): If person is flight crew, links to operational records
    /// 
    /// This demonstrates how HR data integrates with operational domains (Squadron Ops, Medical Care).
    /// </summary>
    public class Person
    {
        [Key]
        public int Id { get; set; }

        // ========== Rank ==========
        [Required]
        public int RankId { get; set; }
        public Rank Rank { get; set; }

        // ========== Identity ==========
        [Required, StringLength(20)]
        public string Matricule { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        // ========== SubDepartment ==========
        [Required]
        public int SubDepartmentId { get; set; }
        public SubDepartment SubDepartment { get; set; }

        // ========== Other fields ==========
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? NationalId { get; set; }

        [StringLength(100)]
        public string? Speciality { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; } = "Morocco";

        public bool Active { get; set; } = true;

        // Patrimonial status
        [StringLength(50)]
        public string? PatrimonialStatus { get; set; } = "Single";

        // Photo
        public byte[]? Photo { get; set; }

        // Computed
        public string FullName => $"{FirstName} {LastName}";

        // Navigation: optional 1:1 link to CrewMember
        public CrewMember? CrewMember { get; set; }
    }
}
