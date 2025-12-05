using FRAProject.Enums;

namespace FRAProject.ViewModels
{
    public class OdvCreateVm
    {
        // ODV header fields
        public int SquadronID { get; set; }
        public int MissionId { get; set; }
        public DateTime OdvDate { get; set; }
        public Zone ZoneID { get; set; } = Zone.North;
        public MissionType MissionTypeID { get; set; } = MissionType.Other;
        public string Area { get; set; } = "";
        public OdvStatus? OdvStatus { get; set; } = Enums.OdvStatus.Planned;
        public TimeSpan? TOFF { get; set; }         // optional ODV-level TOFF

        public int AcMainGroupID { get; set; }
        public string? CallSignId { get; set; }
        public string? Obs { get; set; }

        // Nested sorties
        public List<SortieVm> Sorties { get; set; } = new List<SortieVm>();
    }
}
