using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FRAProject.Areas.Settings.Models; // ASSUMPTION: AcType lives here — matches AcTypesController/AircraftVersion convention. Adjust using if your AcType moved.

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    /// <summary>
    /// A named installable slot on an aircraft type (e.g. "Engine #1", "APU",
    /// "Left Main Landing Gear"). AcType-scoped like WorkSection was BEFORE its
    /// AcMainGroup fix — deliberately kept AcType-scoped here (not AcMainGroup)
    /// because real position layouts differ per variant, same reasoning that
    /// keeps JobCard/InspectionType AcType-scoped after the WorkSection lesson.
    /// </summary>
    [Table("ComponentPositions", Schema = "dbo")]
    public class ComponentPosition
    {
        public int Id { get; set; }

        [Required]
        public int AcTypeId { get; set; }
        [ForeignKey(nameof(AcTypeId))]
        public virtual AcType? AcType { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        // ASSUMPTION: Ata/AtaCategory live in this same Area+namespace (introduced
        // in the WorkOrder/Formule1213 session for JobCard.AtaId). Adjust if not.
        public int? AtaId { get; set; }
        [ForeignKey(nameof(AtaId))]
        public virtual Ata? Ata { get; set; }

        public bool IsActive { get; set; } = true;
        public byte SortOrder { get; set; }

        public virtual ICollection<ComponentTypePosition> ComponentTypePositions { get; set; } = new HashSet<ComponentTypePosition>();
        public virtual ICollection<Component> Components { get; set; } = new HashSet<Component>();
        public virtual ICollection<ComponentEvent> ComponentEvents { get; set; } = new HashSet<ComponentEvent>();
    }
}
