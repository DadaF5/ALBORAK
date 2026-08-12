namespace FRAProject.Support.Dtos
{
    public class BugReportDto
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ScreenOrModule { get; set; } = string.Empty;
        public BugSeverity Severity { get; set; }
        public BugStatus Status { get; set; }
        public string ReportedByName { get; set; } = string.Empty; // FullLabel
        public DateTime ReportedAt { get; set; }
    }
}
