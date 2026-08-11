namespace FRAProject.ViewModels.AircraftMaintenance
{
    public class InspectionTypeManageProgramsViewModel
    {
        public int InspectionTypeId { get; set; }
        public string InspectionTypeCode { get; set; } = string.Empty;
        public string InspectionTypeName { get; set; } = string.Empty;

        public List<LinkedProgramItemViewModel> LinkedPrograms { get; set; } = [];
        public List<MaintenanceProgramLookupViewModel> AvailablePrograms { get; set; } = [];
    }

    public class LinkedProgramItemViewModel
    {
        public int LinkId { get; set; } // InspectionTypeProgram.Id
        public int MaintenanceProgramId { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
    }
}