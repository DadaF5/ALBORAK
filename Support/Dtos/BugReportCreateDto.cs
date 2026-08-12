using System.ComponentModel.DataAnnotations;

namespace FRAProject.Support.Dtos
{
    public class BugReportCreateDto
    {
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire")]
        public string Description { get; set; } = string.Empty;

        public string? ExpectedBehavior { get; set; }
        public string? ActualBehavior { get; set; }

        [Required(ErrorMessage = "L'écran/module est obligatoire")]
        [StringLength(100)]
        public string ScreenOrModule { get; set; } = string.Empty;

        [Required]
        public BugSeverity Severity { get; set; } = BugSeverity.MEDIUM;

        public IFormFile? Screenshot { get; set; }

        // Hidden — auto-captured, shown read-only in the form for transparency
        public string? PageUrl { get; set; }
        public string? UserIpAddress { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? UserAgent { get; set; }
        public string? HttpMethod { get; set; }
    }
}
