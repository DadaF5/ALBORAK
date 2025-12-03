using System.ComponentModel.DataAnnotations;

namespace FRAProject.Enums
{
    public enum MissionType
    {
        [Display(Name = "Training")]
        Training,

        [Display(Name = "Operational")]
        Operational,

        [Display(Name = "Transport")]
        Transport,

        [Display(Name = "Reconnaissance")]
        Reconnaissance,

        [Display(Name = "Other")]
        Other
    }
}
