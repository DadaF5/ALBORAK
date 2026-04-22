using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.HR.Models;
using FRAProject.Areas.SquadronOps.Models; // for Odv
using FRAProject.Models;                  // for Base
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("AcMainGroups", Schema = "dbo")]
    public class AcMainGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Description { get; set; }

        [Required]
        public bool Active { get; set; } = true;

        // FK -> AcCategories(AcCategoryId) with ON DELETE CASCADE in baseline
        [Required]
        public int AcCategoryId { get; set; }
        public AcCategory AcCategory { get; set; } = default!;

        // FK -> Bases(Id) with ON DELETE CASCADE in baseline
        [Required]
        public int BaseId { get; set; }
        public Base Base { get; set; } = default!;

        public ICollection<AcType> AcTypes { get; set; } = new HashSet<AcType>();

        // Baseline has dbo.Odvs with FK Odvs.AcMainGroupId -> AcMainGroups.Id
        public ICollection<Odv> Odvs { get; set; } = new HashSet<Odv>();
    }
}