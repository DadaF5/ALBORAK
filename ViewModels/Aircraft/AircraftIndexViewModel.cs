
using FRAProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels
{
    public class AircraftIndexViewModel
    {
        public int? FilterBaseId { get; set; }
        public int? FilterAcTypeId { get; set; }
        public int? FilterStatusTypeId { get; set; }

        public IEnumerable<SelectListItem>? Bases { get; set; }
        public IEnumerable<SelectListItem>? AcTypes { get; set; }
        public IEnumerable<SelectListItem>? StatusTypes { get; set; }

        public List<Aircraft>? Aircrafts { get; set; }
    }
}

