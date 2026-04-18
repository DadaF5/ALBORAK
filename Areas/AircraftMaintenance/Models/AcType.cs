using FRAProject.Areas.SquadronOps.Models; // Sortie
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    [Table("AcTypes", Schema = "dbo")]
    public class AcType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Baseline: NOT NULL
        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public double MaxGrossweight { get; set; }

        [Required]
        public int MaxPassengers { get; set; }

        [Required]
        public int SeatCount { get; set; }

        [Required]
        public int MaxEngines { get; set; }

        [Required]
        public int AcMainGroupId { get; set; }
        public AcMainGroup AcMainGroup { get; set; } = default!;

        public ICollection<Aircraft> Aircrafts { get; set; } = new HashSet<Aircraft>();

        // Baseline has dbo.Sorties with NOT NULL AcTypeId
        public ICollection<Sortie> Sorties { get; set; } = new HashSet<Sortie>();
    }
}