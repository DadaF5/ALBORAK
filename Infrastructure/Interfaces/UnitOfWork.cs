
using FRAProject.Areas.AircraftMaintenance.Repositories;
using FRAProject.Data;
using FRAProject.Infrastructure.Interfaces;

namespace FRAProject.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FRAContext _context;   
        public UnitOfWork(FRAContext context)
        {
            _context = context;
            AcMainGroups = new AcMainGroupRepository(_context);
        }
        public IAcMainGroupRepository AcMainGroups { get; private set; }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
