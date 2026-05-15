using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class LevelRepository : GenericRepository<Level>, ILevelRepository
    {
        public LevelRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Level>> GetAllLevelsAsync() =>
            await _dbSet.OrderBy(l => l.Id).ToListAsync();
    }
}