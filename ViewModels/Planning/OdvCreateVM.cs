using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels.Planning
{
    public class OdvCreateVM
    {
        public int? Id { get; set; } // null when creating
        [Required] public DateTime OdvDate { get; set; } = DateTime.Today;
        [Required] public int SquadronId { get; set; }
        public int? BaseId { get; set; }
        public int MissionId { get; set; }
        public string Area { get; set; } = string.Empty;
        public string? CallSign { get; set; }
        public string? Obs { get; set; }
    }
}
