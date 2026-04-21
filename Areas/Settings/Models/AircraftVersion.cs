using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("AircraftVersions", Schema = "dbo")]
    public class AircraftVersion : LookupBase
    {
    }
}