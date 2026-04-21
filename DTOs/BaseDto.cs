namespace FRAProject.DTOs
{
    public class BaseDto
    {
        public int Id { get; set; }
        public string BaseName { get; set; } = string.Empty;
        public string? BaseNameLocal { get; set; }       
        public decimal? Latitude { get; set; } 
        public decimal? Longitude { get; set; }

    }
}
