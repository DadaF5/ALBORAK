namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class PopulateJobCardsViewModel
    {
        public int WorkOrderId { get; set; }
        public string WONumber { get; set; } = string.Empty;
        public string AircraftLabel { get; set; } = string.Empty;

        // Resolved automatically from the WorkOrder's own InspectionTypes
        // (via InspectionTypeProgram -> ProgramJobCard) — the user no
        // longer picks programs manually, just reviews/adjusts the
        // resulting job card list.
        public List<JobCardSelectItemViewModel> AvailableJobCards { get; set; } = [];
    }

    public class JobCardSelectItemViewModel
    {
        public int JobCardId { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ProgramLabel { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }
}