using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{
    public class OdvEditVm
    {
        public int Id { get; set; }

        //[Required]
        //public DateTime OdvDate { get; set; }


        [Required]
        public int MissionId { get; set; }

        [Required]
        public int CallSignId { get; set; }

        //[Required]
        //public int SquadronId { get; set; }

        //[Required]
        //public int AcMainGroupId { get; set; }

        [Required]
        public TimeSpan TOFF { get; set; }

        [Required]
        public string Area { get; set; } = "";

        public string? Obs { get; set; }


       

        //public Enums.Zone Zone { get; set; } = Enums.Zone.North;
        //public Enums.MissionType MissionType { get; set; } = Enums.MissionType.Training;
       

        
        
        //// select lists
        //public List<SelectListItem>? Squadrons { get; set; }
        //public List<SelectListItem>? AcMainGroups { get; set; }
        //public List<SelectListItem>? Missions { get; set; }
        //public List<SelectListItem>? CallSigns { get; set; }
        //public List<SelectListItem>? Aircrafts { get; set; }
        //public List<SelectListItem>? CrewMembers { get; set; }
    }
}