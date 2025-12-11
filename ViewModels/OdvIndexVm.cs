using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FRAProject.Models;

namespace FRAProject.ViewModels
{
    public class OdvIndexVm
    {
        // Filters
        public DateTime? SelectedDate { get; set; }
        public int? SelectedSquadronId { get; set; }
        public int? SelectedAcMainGroupId { get; set; }

        // Select lists for filters and forms
        public IEnumerable<SelectListItem> Squadrons { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> AcMainGroups { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Missions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> CallSigns { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Aircrafts { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> CrewMembers { get; set; } = Array.Empty<SelectListItem>();

        // Data to render
        public IEnumerable<Odv> Odvs { get; set; } = Array.Empty<Odv>();
    }
}
