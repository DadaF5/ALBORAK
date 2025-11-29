namespace FRAProject.DTOs
{
    public class SubDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        // For display in Index view
        public string DepartmentName { get; set; } = string.Empty;
        public string BaseName { get; set; } = string.Empty;
    }
}
