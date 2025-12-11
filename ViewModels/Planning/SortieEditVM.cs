namespace FRAProject.ViewModels.Planning
{
    public class SortieEditVM
    {
        public int Id { get; set; }
        public int OdvId { get; set; }
        public string? Callsign { get; set; }
        public TimeSpan? PlannedTOFF { get; set; }
        public string? Configuration { get; set; }
        public decimal? FuelQuantity { get; set; }
    }
}
