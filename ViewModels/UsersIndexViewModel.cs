using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Helpers;

namespace FRAProject.ViewModels
{
    public class UsersIndexViewModel
    {
        public PaginatedList<UserListViewModel> Users { get; set; } = new PaginatedList<UserListViewModel>(new List<UserListViewModel>(), 0, 1, 20);

        // Filters / sorting / paging state
        public string? Search { get; set; }
        public string? SortOrder { get; set; }
        public string? RoleFilter { get; set; }
        public int? BaseFilter { get; set; }
        public bool? IsActiveFilter { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // lists for filter controls
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BaseList { get; set; } = new List<SelectListItem>();
    }
}
