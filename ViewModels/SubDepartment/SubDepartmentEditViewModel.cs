using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels.SubDepartment
{
    public class SubDepartmentEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int BaseId { get; set; }
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem> Bases { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
