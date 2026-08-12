namespace FRAProject.Areas.AircraftMaintenance.Models
{
    // Final approval chain from the bottom of a real Formule 13 — 4 fixed
    // levels (not user-configurable): Chef AT/SEP -> Chef SCQ -> Chef ST ->
    // Chef SGMA1. Real forms show a name/initials plus a circular unit
    // stamp (e.g. "SCA-36", squadron number "26"/"44") per level — no
    // signature image capture, same electronic-attestation approach used
    // for WorkOrderSectionPart/Task.
    public class WorkOrderSectionSignOff
    {
        public int Id { get; set; }

        public int WorkOrderSectionId { get; set; }
        public WorkOrderSection? WorkOrderSection { get; set; }

        public string Level { get; set; } = string.Empty; // CHEF_AT_SEP | CHEF_SCQ | CHEF_ST | CHEF_SGMA1
        public int SortOrder { get; set; } = 100;

        public string? SignedByName { get; set; }
        public string? StampReference { get; set; } // e.g. "SCA-36", "44"
        public DateTime? SignedAtUtc { get; set; }
        public string? Remarks { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public static readonly (string Level, string Label, int SortOrder)[] CanonicalLevels =
        {
            ("CHEF_AT_SEP", "Chef AT/SEP", 1),
            ("CHEF_SCQ",    "Chef SCQ",    2),
            ("CHEF_ST",     "Chef ST",     3),
            ("CHEF_SGMA1",  "Chef SGMA1",  4),
        };
    }
}