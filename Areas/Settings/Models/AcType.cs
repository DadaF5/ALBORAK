using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.Settings.Models;
using FRAProject.Areas.SquadronOps.Controllers;
using FRAProject.Areas.SquadronOps.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    /// <summary>
    /// Aircraft Type - specific variant of an aircraft (e.g., F-16C, F-16D)
    /// Child of AcMainGroup (e.g., F-16 Fighting Falcon)
    /// Parent of AircraftVersion (e.g., Block 50, Block 52+)
    /// </summary>
    [Table("AcTypes", Schema = "dbo")]
    public class AcType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Code { get; set; }

        // Technical specifications
        [Required]
        public double MaxGrossweight { get; set; }

        [Required]
        public int MaxPassengers { get; set; }

        [Required]
        public int SeatCount { get; set; }

        [Required]
        public int MaxEngines { get; set; }

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; } = 99;

        // Foreign Keys
        [Required]
        public int AcMainGroupId { get; set; }
        
        public int? AircraftManufacturerId { get; set; }

        // Navigation properties
        [ForeignKey("AcMainGroupId")]
        public virtual AcMainGroup? AcMainGroup { get; set; }

        [ForeignKey("AircraftManufacturerId")]
        public virtual AircraftManufacturer? AircraftManufacturer { get; set; }

        // Collections
        public virtual ICollection<AircraftVersion> AircraftVersions { get; set; } = new HashSet<AircraftVersion>();
        public virtual ICollection<Aircraft> Aircrafts { get; set; } = new HashSet<Aircraft>();
        public virtual ICollection<Sortie> Sorties { get; set; } = new HashSet<Sortie>();
    }
}
