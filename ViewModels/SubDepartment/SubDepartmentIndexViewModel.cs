using FRAProject.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels.SubDepartment
{
    public class SubDepartmentIndexViewModel
    {
        public IEnumerable<SubDepartmentDto> SubDepartments { get; set; } = new List<SubDepartmentDto>();

        // Filters
        public int? BaseId { get; set; }
        public int? DepartmentId { get; set; }
        public string? SearchTerm { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Bases { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
