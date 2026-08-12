namespace FRAProject.Support.Dtos
{
    public class BugReportTriageDto
    {
        public int Id { get; set; }
        public BugStatus Status { get; set; }
        public string? AdminNotes { get; set; }
    }
}
