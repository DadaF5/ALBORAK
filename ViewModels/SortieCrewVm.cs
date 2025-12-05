namespace FRAProject.ViewModels
{
    public class SortieCrewVm
    {
        public int PersonId { get; set; }           // selected crew member
        public string? Role { get; set; }
        public bool IsPrimary { get; set; }
    }
}
