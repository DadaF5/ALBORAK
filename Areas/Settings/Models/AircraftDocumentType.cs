
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("AircraftDocumentTypes", Schema = "dbo")]
    public class AircraftDocumentType
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; } = string.Empty; // CDN, CEN, PEA, LME...

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<AircraftDocument> AircraftDocuments { get; set; } = new HashSet<AircraftDocument>();
    }
}