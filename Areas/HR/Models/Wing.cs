using FRAProject.Areas.AircraftMaintenance.Models;
using FRAProject.Areas.SquadronOps.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FRAProject.Areas.HR.Models
{
    public class Wing
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string WingLong { get; set; } = "";

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int? AcMainGroupId { get; set; }
        public AcMainGroup? AcMainGroup { get; set; }

        public int? BaseId { get; set; }
        public Base? Base { get; set; }

        public bool Active { get; set; } = true;

        // Navigation - if you plan to serialize entities directly, avoid serializing collections
        [JsonIgnore] // prevents cycles if you serialize Wing
        public ICollection<Squadron>? Squadrons { get; set; }
    }
}