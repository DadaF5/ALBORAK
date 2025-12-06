using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(200)]
        public string? IconClass { get; set; }

        // MVC routing
        [MaxLength(100)]
        public string? Controller { get; set; }

        [MaxLength(100)]
        public string? Action { get; set; }

        [MaxLength(500)]
        public string? Url { get; set; }

        // Hierarchy / ordering
        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;

        // Optional scoping fields (if you use them)
        public int? DepartmentId { get; set; }
        public int? BaseId { get; set; }

        // Optional roles/permission metadata (persisted)
        [MaxLength(200)]
        public string? Roles { get; set; }

        // Runtime-only properties (not persisted)
        [NotMapped]
        public IList<MenuItem> Children { get; set; } = new List<MenuItem>();

        [NotMapped]
        public bool HasChildren => Children != null && Children.Count > 0;

        [NotMapped]
        public object? RouteValues { get; set; }
    }
}