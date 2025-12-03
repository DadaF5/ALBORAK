using System.ComponentModel.DataAnnotations;

namespace FRAProject.Enums
{
    public enum Zone
    {
        [Display(Name = "North")]
        North,

        [Display(Name = "South")]
        South,

        [Display(Name = "East")]
        East,

        [Display(Name = "West")]
        West,

        [Display(Name = "Central")]
        Central,

        [Display(Name = "Foreign")]
        Foreign
    }
}
