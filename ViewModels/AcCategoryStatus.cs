using System.Collections.Generic;

namespace FRAProject.ViewModels
{
    public class AcCategoryStatus
    {
        public int AcCategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<AcMainGroupStatus> MainGroups { get; set; } = new();
    }
}