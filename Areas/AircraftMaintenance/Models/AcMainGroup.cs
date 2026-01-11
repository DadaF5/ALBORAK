// Models/AcMainGroup.cs
using FRAProject.Areas.HR.Models;
using FRAProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FRAProject.Areas.AircraftMaintenance.Models;
public class AcMainGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(50)]
    [Required(ErrorMessage = "Please enter a group name.")]
    public string Name { get; set; }

    [StringLength(50)]
    public string? Description { get; set; } = string.Empty;  

    public bool Active { get; set; } = true;

    // Foreign Key to AcCategory
    [Required(ErrorMessage = "Please select a category.")]
    public int AcCategoryId { get; set; }
    public virtual AcCategory AcCategory { get; set; }


    [Required(ErrorMessage = "Please select a Base.")]
    public int BaseId { get; set; }
    public virtual Base Base { get; set; }

    // Navigation to AcTypes
    public ICollection<AcType> AcTypes { get; set; } = new HashSet<AcType>();
    public ICollection<Odv> Odvs { get; set; } = new HashSet<Odv>();



}