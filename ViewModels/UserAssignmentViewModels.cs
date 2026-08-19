// ViewModels/UserAssignmentViewModels.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    // Renamed from UserAssignmentGrantDto — collided with the distinct
    // Services.UserAssignmentGrantDto (the minimal DTO GrantAsync() takes),
    // causing "ambiguous reference" since UserAssignmentsController has
    // both namespaces in scope. Matches this project's established
    // FormDto naming convention (AircraftFormDto, RoleFormDto,
    // ModuleRoleFormDto) — this is the form-binding version with the
    // extra UI-only properties (UserLabel, dropdown option lists) the
    // service-layer DTO deliberately doesn't need.
    public class UserAssignmentFormDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserLabel { get; set; } = string.Empty; // display only

        public bool IsBaseAdmin { get; set; }
        public int? ModuleRoleId { get; set; }
        public int BaseId { get; set; }
        public int? AcMainGroupId { get; set; }
        public int? WingId { get; set; }

        public IEnumerable<SelectListItem> ModuleRoleOptions { get; set; } = [];
        public IEnumerable<SelectListItem> BaseOptions { get; set; } = [];
        public IEnumerable<SelectListItem> WingOptions { get; set; } = [];
        // AcMainGroupOptions populated via AJAX once Base is chosen —
        // AcMainGroup.BaseId means the list depends on which Base is
        // selected, same cascading pattern as Aircraft's Create form.
    }

    public class UserAssignmentListItemVm
    {
        public int Id { get; set; }
        public string RoleLabel { get; set; } = string.Empty; // "Base Admin" or "MAINTENANCE / TECHNICIAN"
        public string BaseName { get; set; } = string.Empty;
        public string? AcMainGroupLabel { get; set; }
        public string? WingName { get; set; }
        public DateTime GrantedAtUtc { get; set; }
        public string? GrantedByLabel { get; set; }
    }
}
