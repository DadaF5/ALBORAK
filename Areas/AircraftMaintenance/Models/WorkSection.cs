using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Responsible sections for Formule 13 completion records (INST, RDR,
    // ARMT, NDI, etc.) — scoped per AcType, since different aircraft types
    // have genuinely different relevant sections (e.g. an Alpha Jet has no
    // ARMT/armament section the way an F5F does).
    [Table("WorkSections")]
    public class WorkSection : LookupBase
    {
        public int AcTypeId { get; set; }
        public AcType? AcType { get; set; }
    }
}