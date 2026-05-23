using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// Represents a periodic inspection type for a specific aircraft type.
    /// Examples: "100h Check", "Phase 1", "Annual Inspection".
    ///
    /// Key design notes:
    /// - Scoped per AcType: a Code must be unique within one AcType.
    /// - NextInspectionTypeId: self-reference chain for inspection progression.
    /// - Inherits Code, Name, Description, IsActive, SortOrder from LookupBase.
    /// </summary>
    [Table("InspectionTypes", Schema = "dbo")]
    public class InspectionType : LookupBase
    {
        // =========================================
        // AcType scope: inspection is per aircraft type
        // =========================================
        [Required]
        public int AcTypeId { get; set; }
        public AcType AcType { get; set; } = default!;

        // =========================================
        // Self-reference: optional chain to next inspection type
        // e.g. Phase1 -> Phase2 -> Phase3
        // =========================================
        public int? NextInspectionTypeId { get; set; }
        public InspectionType? NextInspectionType { get; set; }

        // Reverse: inspection types whose NextInspectionType points to this one
        public ICollection<InspectionType> PrecedingInspectionTypes { get; set; } = new HashSet<InspectionType>();
    }
}
