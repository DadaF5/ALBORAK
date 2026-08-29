using System.ComponentModel.DataAnnotations;

namespace FRAProject.Areas.SquadronOps.ViewModels
{
    public class SortieCreateVm


    {
        public int Id { get; set; }

        [Required]
        public int OdvId { get; set; }

        [Required]
        [StringLength(10)]
        public string SortieCode { get; set; } = "";

        [Required]
        public int AcTypeId { get; set; }   // F-16C, F-16D, etc.

        [Required]
        [StringLength(100)]
        public string Configuration { get; set; } = "";

        public decimal? FuelQuantity { get; set; }


        public int Sequence { get; set; } = 1;

        public decimal? PlannedFuel { get; set; }

        public string? Notes { get; set; }
    }

}
