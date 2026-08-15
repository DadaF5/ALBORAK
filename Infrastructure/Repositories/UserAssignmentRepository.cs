// Repositories/UserAssignmentRepository.cs
using FRAProject.Data;
using FRAProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FRAProject.Infrastructure.Repositories
{
    public class UserAssignmentRepository : IUserAssignmentRepository
    {
        private readonly FRAContext _context;
        public UserAssignmentRepository(FRAContext context) => _context = context;

        public async Task<UserAssignment?> GetByIdAsync(int id) =>
            await _context.UserAssignments.FindAsync(id);

        public async Task<IEnumerable<UserAssignment>> GetActiveByUserIdAsync(string userId) =>
            await _context.UserAssignments
                .Where(a => a.UserId == userId && a.IsActive)
                .Include(a => a.ModuleRole)
                .Include(a => a.Base)
                .Include(a => a.AcMainGroup)
                .Include(a => a.Wing)
                .ToListAsync();

        public async Task<IEnumerable<UserAssignment>> GetAllActiveWithDetailsAsync() =>
            await _context.UserAssignments
                .Where(a => a.IsActive)
                .Include(a => a.User)
                .Include(a => a.ModuleRole)
                .Include(a => a.Base)
                .Include(a => a.AcMainGroup)
                .ToListAsync();

        public async Task<UserAssignment> AddAsync(UserAssignment assignment)
        {
            await _context.UserAssignments.AddAsync(assignment);
            return assignment;
        }

        public void Update(UserAssignment assignment) => _context.UserAssignments.Update(assignment);
    }
}