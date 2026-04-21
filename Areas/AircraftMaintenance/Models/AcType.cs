
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Models; // Sortie
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

        [StringLength(30)]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; } = 99;


        // optional FKs into Settings lookups:
        public int? AircraftManufacturerId { get; set; }
        public AircraftManufacturer? AircraftManufacturer { get; set; }

        public int? AircraftVersionId { get; set; }
        public AircraftVersion? AircraftVersion { get; set; }



        public ICollection<Aircraft> Aircrafts { get; set; } = new HashSet<Aircraft>();

        // Baseline has dbo.Sorties with NOT NULL AcTypeId
        public ICollection<Sortie> Sorties { get; set; } = new HashSet<Sortie>();
    }
}