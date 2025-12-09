namespace FRAProject.Services
{
    public interface ICurrentUserService
    {
        string? UserName { get; }
        string? UserId { get; }
        int? SquadronId { get; }
        int? WingId { get; }
        int? AcMainGroupId { get; }
        bool IsAdmin { get; }
    }

}
