namespace FRAProject.DTOs
{
    public class AcTypeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public byte SortOrder { get; set; }
        
        // Technical specs
        public double MaxGrossweight { get; set; }
        public int MaxPassengers { get; set; }
        public int SeatCount { get; set; }
        public int MaxEngines { get; set; }
        
        // Parent/Related
        public int AcMainGroupId { get; set; }
        public string AcMainGroupName { get; set; } = string.Empty;
        
        public int? AircraftManufacturerId { get; set; }
        public string? ManufacturerName { get; set; }
    }
}
