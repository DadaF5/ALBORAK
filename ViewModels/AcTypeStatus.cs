using System.Collections.Generic;
using System.Linq;

namespace FRAProject.ViewModels
{
    public class AcTypeStatus
    {
        public int AcTypeId { get; set; }
        public string TypeName { get; set; }

        // Status name (e.g., "Serviceable") → count
        public Dictionary<string, int> StatusCounts { get; set; } = new();

        // Total aircraft for this type
        public int Total => StatusCounts.Values.Sum();
    }
}