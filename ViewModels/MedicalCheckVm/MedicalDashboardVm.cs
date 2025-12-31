namespace FRAProject.ViewModels.MedicalCheckVm
{
    public class MedicalDashboardVm
    {
        // Top cards
        public int FitCount { get; set; }
        public int ExpiringCount { get; set; }
        public int ExpiredCount { get; set; }

        // Medical flags
        public int ObesityCount { get; set; }
        public int OpticalCorrectionCount { get; set; }
        public int WithBilansCount { get; set; }

        // Main grid
        public List<MedicalCrewRowVm> Crew { get; set; } = new();
    }
}
