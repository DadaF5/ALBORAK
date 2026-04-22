namespace FRAProject.DTOs
{
    public class AircraftVersionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;         // From LookupBase
        public string Name { get; set; } = string.Empty;         // From LookupBase
        public string? Description { get; set; }                 // From LookupBase
        public bool IsActive { get; set; }                       // From LookupBase
        public byte SortOrder { get; set; }                      // From LookupBase
        
        // Additional fields
        public int AcTypeId { get; set; }
        public string AcTypeName { get; set; } = string.Empty;   // For display
    }
}
