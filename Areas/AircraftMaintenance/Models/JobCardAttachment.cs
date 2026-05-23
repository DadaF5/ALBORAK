using FRAProject.Models;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    public class JobCardAttachment
    {
        public int Id { get; set; }

        public int JobCardId { get; set; }
        public JobCard? JobCard { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

        public string? UploadedByUserId { get; set; }
        public ApplicationUser? UploadedByUser { get; set; }
    }
}