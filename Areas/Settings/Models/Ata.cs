using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("ATA")]
    public class Ata : LookupBase
    {
        public int? AtaCategoryId { get; set; }
        public AtaCategory? AtaCategory { get; set; }
    }
}