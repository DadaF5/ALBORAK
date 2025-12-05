using System;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class CompleteSortieVm
    {
        public int SortieId { get; set; }
        public int OdvId { get; set; }

        [Required]
        [Display(Name = "Take-off (UTC)")]
        public DateTime TakeOffUtc { get; set; }

        [Required]
        [Display(Name = "Landing (UTC)")]
        public DateTime LandingUtc { get; set; }

        [Display(Name = "Hobbs Start")]
        public decimal? HobbsStart { get; set; }

        [Display(Name = "Hobbs End")]
        public decimal? HobbsEnd { get; set; }

        [Display(Name = "Tach Start")]
        public decimal? TachStart { get; set; }

        [Display(Name = "Tach End")]
        public decimal? TachEnd { get; set; }

        [Display(Name = "Fuel Used (kg)")]
        public decimal? FuelUsedKg { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string CompletedBy { get; set; } = "";
    }
}
