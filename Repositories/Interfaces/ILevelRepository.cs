using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces
{
    public interface ILevelRepository : IGenericRepository<Level>
    {
        Task<IEnumerable<Level>> GetAllLevelsAsync();
    }
}