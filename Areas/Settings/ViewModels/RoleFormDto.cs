// Areas/Settings/ViewModels/RoleFormDto.cs (or wherever your other Form DTOs live)
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class RoleFormDto
    {
        public string? Id { get; set; } // null = Create, set = Edit

        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [StringLength(50)]
        [Display(Name = "Nom du rôle")]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleListVm
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public bool IsProtected { get; set; } // "Admin" — can't be deleted
    }
}