using System.ComponentModel.DataAnnotations;

namespace FRAProject.Models
{
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
    }
}
