using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.Settings.Models
{
    [Table("AircraftManufacturers", Schema = "dbo")]
    public class AircraftManufacturer : LookupBase
    {
    }
}