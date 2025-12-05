using Microsoft.AspNetCore.Mvc.Rendering;

namespace FRAProject.ViewModels.Rank
{
    public class RankViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string FullRank { get; set; } = string.Empty;

        public int Sequence { get; set; }

        public int RankTypeId { get; set; }   
       
        public IEnumerable<SelectListItem> RankTypes { get; set; } = new List<SelectListItem>();
    }
}
