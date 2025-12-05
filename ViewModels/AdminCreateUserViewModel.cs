using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    // Admin creates users and sets scoping, roles, qualifications, etc.
    public class AdminCreateUserViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public string? JobTitle { get; set; }
        public string? EmployeeNumber { get; set; }

        // Organization scoping (admin chooses)
        public int? BaseId { get; set; }
        public int? WingId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SquadronId { get; set; }
        public int? AcMainGroupId { get; set; }

        // Roles to assign
        public List<string> Roles { get; set; } = new List<string>();

        // Initial qualifications to attach
        public List<int> QualificationAcTypeIds { get; set; } = new List<int>();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
