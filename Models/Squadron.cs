using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class Squadron
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        [Display(Name = "Call-Sign (BORAK)")]
        public string? CallSign { get; set; }

        [StringLength(100)]
        [Display(Name = "Logo Path")]
        public string? LogoPath { get; set; }

        [NotMapped]
        [Display(Name = "Squadron Logo")]
        public IFormFile? LogoFile { get; set; }

        [StringLength(40)]
        [Display(Name = "Nom de l'Escadron")]
        public string? FrenchName { get; set; }

        [StringLength(10)]
        [Display(Name = "Short Call-Sign (BRK)")]
        public string? CallSignShort { get; set; }

        // FK to Wing
        [Required]
        public int WingId { get; set; }
        public Wing Wing { get; set; }

        public bool Active { get; set; } = true;

        // Computed for display
        [NotMapped]
        public string FullName => $"{Name} ({Wing?.Name})";
    }
}