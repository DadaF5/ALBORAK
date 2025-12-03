using System.ComponentModel.DataAnnotations;

namespace FRAProject.Enums
{
    public enum OdvStatus
    {
        [Display(Name = "Planned")]
        Planned,

        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Cancelled")]
        Cancelled,

        [Display(Name = "Postponed")]
        Postponed
    }
}
