using FRAProject.Areas.Settings.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Responsible sections for Formule 13 completion records (INST, RDR,
    // ARMT, NDI, etc.) — scoped per AcMainGroup (real aircraft family: F16,
    // F5, C130, AJET), not per individual AcType variant. F16C/F16D share
    // sections, as do F5E/F5F — a section only earns its own row when the
    // FAMILY itself genuinely differs (e.g. no ARMT section makes sense
    // for the Alpha Jet family), not when two variants of the same family
    // happen to have separate AcType rows.
    //
    // NOTE: this was originally AcTypeId-scoped, back when AcMainGroup's
    // seeded data had drifted to mission-scoped categories (CHASSE-2B
    // lumping F16+F5+AJET together) and wasn't safe to use for real family
    // grouping — that was a deliberate, flagged tradeoff at the time. The
    // RBAC session fixed AcMainGroup's real seeded data (F16-2B/F5-2B/
    // AJET-2B), and this migrates WorkSection onto it, merging what were
    // duplicate per-AcType rows (see the accompanying data migration).
    [Table("WorkSections")]
    public class WorkSection : LookupBase
    {
        public int AcMainGroupId { get; set; }
        public AcMainGroup? AcMainGroup { get; set; }
    }
}
