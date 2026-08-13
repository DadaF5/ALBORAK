using FRAProject.Areas.AircraftMaintenance.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("ATA")]
    public class Ata : LookupBase
    {
        public int? AtaCategoryId { get; set; }
        public AtaCategory? AtaCategory { get; set; }

        // Aircraft snags and malfunctions
        // Ata.cs — add (single FK, same reasoning)
        public ICollection<Snag> Snags { get; set; } = new HashSet<Snag>();
    }
}