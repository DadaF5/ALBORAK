namespace FRAProject.ViewModels.RankType
{
    public class RankTypeViewModel
    {
        public int Id { get; set; }
        // Corrected property name to match the model
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
