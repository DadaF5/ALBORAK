namespace FRAProject.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Base info (for display in Index view)
        public int BaseId { get; set; }
        public string BaseName { get; set; } = string.Empty;
    }
}
