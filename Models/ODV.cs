using System;
using System.Collections.Generic;
using FRAProject.Enums;

namespace FRAProject.Models
{
    /// <summary>
    /// ODV (Air Activity) entity following EF naming conventions.
    /// - PK is Id
    /// - FK properties end with "Id"
    /// - Navigation properties are nullable reference types
    /// - Collections are initialized to avoid NREs
    /// </summary>
    public class Odv
    {
        // Primary key (EF convention)
        public int Id { get; set; }

        // Header / relationships
        public int SquadronId { get; set; }
        public Squadron? Squadron { get; set; }

        public int MissionId { get; set; }
        public Mission? Mission { get; set; }

        // date only (configure column type to "date" in DbContext)
        public DateTime OdvDate { get; set; }

        // enum-backed fields (we'll map enums to string columns via value converters in DbContext)
        public Zone Zone { get; set; } = Zone.North;
        public MissionType MissionType { get; set; } = MissionType.Other;

        public string Area { get; set; } = string.Empty;
        public OdvStatus? OdvStatus { get; set; } = Enums.OdvStatus.Planned;

        // Time-of-takeoff (configure column type "time" in DbContext)
        public TimeSpan? TOFF { get; set; }

        public string? Obs { get; set; }

        public int AcMainGroupId { get; set; }
        public AcMainGroup? AcMainGroup { get; set; }

        // call sign / identifier
        public string? CallSign { get; set; }

        // audit fields (use UTC)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // navigation: sorties
        public ICollection<Sortie> Sorties { get; set; } = new List<Sortie>();
    }
}