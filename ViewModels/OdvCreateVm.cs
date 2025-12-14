using FRAProject.Enums;
using FRAProject.Models; // adjust if your enums live elsewhere
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    // ViewModel for creating or editing an ODV with nested sorties & crew
    public class OdvCreateVm
    {
        public int? Id { get; set; }

        // Squadron selection - required for create
        [Required(ErrorMessage = "Please select a squadron")]
        [Display(Name = "Squadron")]
        public int SquadronId { get; set; }

        // Mission selection - required
        [Required(ErrorMessage = "Please select a mission")]
        [Display(Name = "Mission")]
        public int MissionId { get; set; }

        // Date only (use <input type="date" /> in view)
        [Required]
        [DataType(DataType.Date)]
        public DateTime OdvDate { get; set; } = DateTime.UtcNow.Date;

        // Use your Zone enum; default provided. If you want user to choose, render select with enum values.
        [Display(Name = "Zone")]
        [Required]
        public Enums.Zone Zone { get; set; } = Enums.Zone.North;

        // Mission type enum
        [Display(Name = "Mission Type")]
        [Required]
        public Enums.MissionType MissionType { get; set; } = Enums.MissionType.Training;

        [Display(Name = "Area")]
        [Required]
        public string? Area { get; set; }

        // Use the same enum type for ODV status as in your Models namespace
        [Display(Name = "ODV Status")]
        public OdvStatus? OdvStatus { get; set; } = FRAProject.Enums.OdvStatus.Planned;

        // Optional planned TOFF: bind using <input type="time" /> or accept a string and parse on server
        [Display(Name = "Planned TOFF")]
        [Required]
        public TimeSpan? TOFF { get; set; }

        [Required]
        [Display(Name = "Aircraft Main Group")]
        public int AcMainGroupId { get; set; }

        // in OdvCreateVm
        [Required]
        [Display(Name = "Call Sign")]
       
        public int CallSignId { get; set; }    // allow none

        [Display(Name = "Observations")]
        public string? Obs { get; set; }

        // Nested sorties to create/edit with this ODV
        public List<SortieVm> Sorties { get; set; } = new List<SortieVm>();
    }
}