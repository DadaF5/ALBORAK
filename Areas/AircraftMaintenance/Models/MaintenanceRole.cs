using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Lookup table for maintenance roles.
    ///
    /// Predefined roles (seeded):
    ///   Code = "TECH"        Name = "Technician"
    ///   Code = "BASE_SUP"    Name = "Base Supervisor"
    ///   Code = "MASTER_SUP"  Name = "Master Supervisor"
    ///
    /// Role capabilities:
    ///   Technician       – read own work orders, sign off on own task cards.
    ///   Base Supervisor  – read/write all work orders within their Base + AcMainGroup scope.
    ///   Master Supervisor– read/write all work orders across all bases and groups.
    /// </summary>
    [Table("MaintenanceRoles", Schema = "dbo")]
    public class MaintenanceRole : LookupBase
    {
        // Navigation: assignments using this role
        public ICollection<UserMaintenanceAssignment> UserAssignments { get; set; } = new HashSet<UserMaintenanceAssignment>();
    }
}
