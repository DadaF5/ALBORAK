using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels.SubDepartment
{
    public class SubDepartmentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        // Dropdown for Departments
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }
}
