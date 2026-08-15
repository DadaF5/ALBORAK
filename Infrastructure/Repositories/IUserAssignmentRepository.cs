// Repositories/IUserAssignmentRepository.cs
using FRAProject.Models;

namespace FRAProject.Infrastructure.Repositories
{
    public interface IUserAssignmentRepository
    {
        Task<UserAssignment?> GetByIdAsync(int id);
        Task<IEnumerable<UserAssignment>> GetActiveByUserIdAsync(string userId);
        Task<IEnumerable<UserAssignment>> GetAllActiveWithDetailsAsync();
        Task<UserAssignment> AddAsync(UserAssignment assignment);
        void Update(UserAssignment assignment);
    }
}