namespace FRAProject.ViewModels
{
    public class AcMainGroupStatus
    {
        public int AcMainGroupId { get; set; }
        public string MainGroupName { get; set; }
        public List<AcTypeStatus> Types { get; set; } = new();
    }
}