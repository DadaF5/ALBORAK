using FRAProject.Enums;

namespace FRAProject.ViewModels
{
    public class OdvCreateVm
    {
        // ODV header fields (note naming matches your controller usage: SquadronID, AcMainGroupID etc.)
        public int SquadronID { get; set; }
        public int MissionId { get; set; }

        // date only
        public DateTime OdvDate { get; set; } = DateTime.UtcNow.Date;

        // Enums (use your Zone / MissionType / OdvStatus enums)
        public Zone ZoneID { get; set; } = Zone.North;
        public MissionType MissionTypeID { get; set; } = MissionType.Other;

        public string Area { get; set; } = string.Empty;
        public OdvStatus? OdvStatus { get; set; } = Enums.OdvStatus.Planned;

        // optional ODV-level TOFF
        public TimeSpan? TOFF { get; set; }

        public int AcMainGroupID { get; set; }
        public string? CallSignId { get; set; }
        public string? Obs { get; set; }

        // Nested sorties to create/edit with this ODV
        public List<SortieVm> Sorties { get; set; } = new List<SortieVm>();
    }
}
