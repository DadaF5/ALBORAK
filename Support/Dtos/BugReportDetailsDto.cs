namespace FRAProject.Support.Dtos
{
    public class BugReportDetailsDto : BugReportCreateDto
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; } = string.Empty;
        public BugStatus Status { get; set; }
        public string ReportedByName { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string? ScreenshotPath { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedByName { get; set; }
    }
}
