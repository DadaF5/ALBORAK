using FRAProject.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    public class DepartmentEditViewModel
    {
        // For Edit
        public DepartmentEditDto Edit { get; set; } = new DepartmentEditDto();

        // Dropdown for Base selection
        public IEnumerable<SelectListItem> Bases { get; set; } = new List<SelectListItem>();
       
        // Optional: Search/filter properties for Index
        public int? FilterBaseId { get; set; }
        public string? SearchTerm { get; set; }

        // Optional: Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / PageSize);
    }
}
