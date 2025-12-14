using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Models;

namespace FRAProject.ViewModels
{
    public class OdvIndexVm
    {
        // Filters
        public int? SelectedSquadronId { get; set; }
        public DateTime SelectedDate { get; set; }
        public int? SelectedAcMainGroupId { get; set; }

        // Create-model (bind the create form to this so posted values are preserved)
        public OdvCreateVm? CreateModel { get; set; }

        // Select lists for the page
        public List<SelectListItem>? Squadrons { get; set; }
        public List<SelectListItem>? AcMainGroups { get; set; }
        public List<SelectListItem>? Missions { get; set; }
        public List<SelectListItem>? CallSigns { get; set; } // Value = Id.ToString()
        public List<SelectListItem>? Aircrafts { get; set; }
        public List<SelectListItem>? CrewMembers { get; set; }

        // in OdvIndexVm
        public List<SelectListItem>? ZoneList { get; set; }
        public List<SelectListItem>? MissionTypeList { get; set; }

        // Data to display
        public List<Odv>? Odvs { get; set; }
    }
}
