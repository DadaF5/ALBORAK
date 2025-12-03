using System;
using FRAProject.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Models
{
    public class Odv
    {
        public int OdvID { get; set; }

        public int SquadronID { get; set; }
        public Squadron? Squadron { get; set; }

        public int MissionId { get; set; }
        public Mission? Mission { get; set; }

        // date only
        public DateTime OdvDate { get; set; }

        // enum-backed fields
        // We'll map these to strings in DB via EF value converters (see FRAContext snippet).
        public Zone ZoneID { get; set; } = Zone.North;
        public MissionType MissionTypeID { get; set; } = MissionType.Other;
        public string Area { get; set; } = "";
        public OdvStatus? OdvStatus { get; set; } = Enums.OdvStatus.Planned;

        public TimeSpan? TOFF { get; set; }
        public string? Obs { get; set; }

        public int AcMainGroupID { get; set; }
        public AcMainGroup? AcMainGroup { get; set; }

        public string? CallSignId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}