namespace FRAProject.ViewModels.Planning
{
    public class OdvIndexItemVM
    {
        public int Id { get; set; }
        public DateTime OdvDate { get; set; }
        public string MissionName { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? Area { get; set; }
        public int SortieCount { get; set; }
        public bool IsPreflightApproved { get; set; }
    }
}
