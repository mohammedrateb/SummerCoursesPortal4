using WebApplication1.Models;

namespace WebApplication1.Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student?> GetByNationalIdAsync(string nationalId);
        Task<bool> NationalIdExistsAsync(string nationalId);
        Task<IEnumerable<Student>> GetByLevelAsync(int levelId);
        Task<IEnumerable<Student>> GetByDivisionAsync(int divisionId);
        Task<IEnumerable<Student>> GetByLevelAndDivisionAsync(int levelId, int divisionId);
        Task<IEnumerable<Student>> GetAllWithDetailsAsync();
        Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedWithDetailsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            int? levelId = null,
            int? divisionId = null);
    }
}