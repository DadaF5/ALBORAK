using FRAProject.Enums;

namespace FRAProject.ViewModels.MedicalCheckVm
{
    public class MedicalCrewRowVm
    {
        public int CrewMemberId { get; set; }
        public string Name { get; set; } = "";
        public string Squadron { get; set; } = "";

        public DateTime? LastCheckDate { get; set; }
        public MedicalCheckType? CheckType { get; set; }
        public string? Decision { get; set; }

        public int RemainingDays { get; set; }
        public MedicalFitnessStatus FitnessStatus { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Durations
        public int DurationYears { get; set; }
        public int DurationMonths { get; set; }
        public int DurationDays { get; set; }


        public bool HasObesity { get; set; }
        public bool HasOpticalCorrection { get; set; }
        public bool HasOpenBilans { get; set; }
    }

}
