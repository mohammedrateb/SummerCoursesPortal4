using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class DivisionRepository : GenericRepository<Division>, IDivisionRepository
    {
        public DivisionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Division>> GetAllDivisionsAsync() =>
            await _dbSet.OrderBy(d => d.Id).ToListAsync();
    }
}