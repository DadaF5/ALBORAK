using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Support.Repositories
{
    public class BugReportRepository : GenericRepository<BugReport>, IBugReportRepository
    {
        public BugReportRepository(FRAContext context) : base(context) { }

        public async Task<string> GenerateNextReportNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"BUG-{year}-";

            var lastNumber = await _context.BugReports
                .Where(b => b.ReportNumber.StartsWith(prefix))
                .OrderByDescending(b => b.ReportNumber)
                .Select(b => b.ReportNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (lastNumber != null)
            {
                var seqPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(seqPart, out int lastSeq))
                    nextSeq = lastSeq + 1;
            }

            return $"{prefix}{nextSeq:D4}"; // BUG-2026-0001
        }

        public async Task<IEnumerable<BugReport>> GetByStatusAsync(BugStatus status)
        {
            return await _context.BugReports
                .Include(b => b.ReportedBy)
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.ReportedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BugReport>> GetByUserAsync(string userId)
        {
            return await _context.BugReports
                .Where(b => b.ReportedByUserId == userId)
                .OrderByDescending(b => b.ReportedAt)
                .ToListAsync();
        }

        public async Task<BugReport?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.BugReports
                .Include(b => b.ReportedBy)
                .Include(b => b.ResolvedBy)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
