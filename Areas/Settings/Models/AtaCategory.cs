namespace FRAProject.Areas.Settings.Models
{
    // The 4 top-level ATA section groupings (Aircraft General, Airframe
    // Systems, Structure, Power Plant), per ATA iSpec 2200. Standard
    // LookupBase — default table name "AtaCategories" via EF convention,
    // no schema override.
    public class AtaCategory : LookupBase
    {
        public ICollection<Ata> AtaChapters { get; set; } = [];
    }
}
