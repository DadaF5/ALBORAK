using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Support.Repositories
{
    public interface IBugReportRepository : IGenericRepository<BugReport>
    {
        Task<string> GenerateNextReportNumberAsync();
        Task<IEnumerable<BugReport>> GetByStatusAsync(BugStatus status);
        Task<IEnumerable<BugReport>> GetByUserAsync(string userId);
        Task<BugReport?> GetByIdWithDetailsAsync(int id); // includes ReportedBy, ResolvedBy
    }
}
