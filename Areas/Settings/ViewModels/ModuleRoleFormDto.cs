// Areas/Settings/ViewModels/ModuleRoleFormDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    public class ModuleRoleFormDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le module est obligatoire.")]
        [Display(Name = "Module")]
        public string ModuleCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code du rôle est obligatoire.")]
        [StringLength(30)]
        [Display(Name = "Code Rôle")]
        public string RoleCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom du Rôle")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Peut écrire (sinon lecture seule)")]
        public bool CanWrite { get; set; } = true;

        [StringLength(20)]
        [Display(Name = "Niveau de Signature (WorkOrder)")]
        public string? SignOffLevel { get; set; }

        [Display(Name = "Filtrer par Base")]
        public bool ShowBaseScope { get; set; } = true;

        [Display(Name = "Filtrer par Groupe d'Aéronefs")]
        public bool ShowGroupScope { get; set; } = true;

        [Display(Name = "Filtrer par Escadre")]
        public bool ShowWingScope { get; set; } = false;

        [Display(Name = "Actif")]
        public bool IsActive { get; set; } = true;

        [Range(0, 255)]
        [Display(Name = "Ordre d'affichage")]
        public byte SortOrder { get; set; } = 99;

        public IEnumerable<SelectListItem> ModuleOptions { get; set; } = [];
    }
}