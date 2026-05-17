using FRAProject.Areas.Settings.Interfaces;
using FRAProject.Areas.Settings.Models;
using FRAProject.Data;
using FRAProject.Infrastructure.Repositories;

namespace FRAProject.Areas.Settings.Repositories
{
    public class BaseRepository: GenericRepository<Base>, IBaseRepository
    {
        public BaseRepository(FRAContext context) : base(context)
        {
        }       
        
    }
}
