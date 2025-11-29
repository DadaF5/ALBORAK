using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
    {
        public class PersonViewModel
{
    public int Id { get; set; }

    // Rank
    [Required]
    public int RankId { get; set; }
    public string RankName { get; set; } = string.Empty;

    // Identity
    [Required, StringLength(20)]
    public string Matricule { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

        [StringLength(10)]
        public string Gender { get; set; } = "Male"; // Default value

        // SubDepartment
        [Required]
    public int SubDepartmentId { get; set; }
    public string SubDepartmentName { get; set; } = string.Empty;

    // Department
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    // Base
    public int BaseId { get; set; }
    public string BaseName { get; set; } = string.Empty;

    // Other fields
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

    // Photo
    public byte[]? Photo { get; set; }
      
    // Property used for uploading the file
    public IFormFile? PhotoFile { get; set; }

    // Patrimonial status
    [StringLength(50)]
    public string? PatrimonialStatus { get; set; } = "Single";

    // Computed
    public string FullName => $"{FirstName} {LastName}";
    public string RankFullName => $"{RankName} {LastName} {FirstName}";
}

    }