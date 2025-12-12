using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class EditUserViewModel
    {
        public string? Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public int? BaseId { get; set; }
        public int? WingId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SquadronId { get; set; }
        public int? AcMainGroupId { get; set; }

        public bool IsActive { get; set; }

        // Roles selected in the form
        public List<string>? SelectedRoles { get; set; }

        // Lists for selects (populated by controller)
        public List<SelectListItem>? AvailableRoles { get; set; }
        public List<SelectListItem>? BaseList { get; set; }
        public List<SelectListItem>? WingList { get; set; }
        public List<SelectListItem>? DepartmentList { get; set; }
        public List<SelectListItem>? SquadronList { get; set; }
        public List<SelectListItem>? AcMainGroupList { get; set; }
    }
}