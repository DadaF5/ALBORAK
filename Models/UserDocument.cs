namespace FRAProject.Models
{
    // Metadata for user documents (licenses, medical, certificates...)
    // Store files in secure blob storage and keep only reference / URL / storage key here.
    public class UserDocument
    {
        public int Id { get; set; }
        public string UserId { get; set; } = ""; // ApplicationUser.Id
        public string DocumentType { get; set; } = ""; // e.g. "Medical", "License", "TrainingRecord"
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSizeBytes { get; set; }
        public string StorageKey { get; set; } = ""; // e.g. S3 key or blob path
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAtUtc { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? Notes { get; set; }
    }
}
