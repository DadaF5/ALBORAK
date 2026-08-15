// Services/UserAssignmentDtos.cs
namespace FRAProject.Services
{
    public class UserAssignmentGrantDto
    {
        public string UserId { get; set; } = null!;
        public int? ModuleRoleId { get; set; }
        public int BaseId { get; set; }
        public bool IsBaseAdmin { get; set; }
        public int? AcMainGroupId { get; set; }
        public int? WingId { get; set; }
    }
}