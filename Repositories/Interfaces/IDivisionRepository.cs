using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IDivisionRepository : IGenericRepository<Division>
    {
        Task<IEnumerable<Division>> GetAllDivisionsAsync();
    }
}