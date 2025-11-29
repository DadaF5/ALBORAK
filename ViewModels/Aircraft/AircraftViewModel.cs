using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FRAProject.ViewModels
{

    public class AircraftViewModel
    {
        public int Id { get; set; }

        // --------------------------
        // Basic Fields
        // --------------------------

        [Required]
        [Display(Name = "Tail Number")]
        public int TailNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Registration { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Manufacture Date")]
        public DateTime? ManufactureDate { get; set; }

        [StringLength(10)]
        [Display(Name = "Internal Code")]
        public string? IntCode { get; set; }

        [Display(Name = "Observations")]
        public string? Obs { get; set; }

        // --------------------------
        // Status
        // --------------------------

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Serviceable")]
        public bool IsServiceable { get; set; } = true;

        // --------------------------
        // Foreign Keys
        // --------------------------

        [Required(ErrorMessage = "AcType is required")]
        [Display(Name = "Aircraft Type")]
        public int AcTypeId { get; set; }

        [Required(ErrorMessage = "Status Type is required")]
        [Display(Name = "Status Type")]
        public int AcStatusTypeId { get; set; }

        // --------------------------
        // Dropdown Lists (nullable!)
        // --------------------------

        public IEnumerable<SelectListItem>? AcMainGroups { get; set; }
        public IEnumerable<SelectListItem>? AcTypes { get; set; }
        public IEnumerable<SelectListItem>? AcStatusTypes { get; set; }

        // --------------------------
        // Default serviceable Status Id
        public int DefaultServiceableStatusId { get; set; }

    }
}

