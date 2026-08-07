namespace FRAProject.ViewModels.Settings
{
    public class AtaListItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}