namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class JobCardAttachmentItemViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public DateTime UploadedAtUtc { get; set; }
        public string? UploadedByUserName { get; set; }
    }
}