using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Student?> GetByNationalIdAsync(string nationalId) =>
            await _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .FirstOrDefaultAsync(s => s.NationalId == nationalId);

        public async Task<bool> NationalIdExistsAsync(string nationalId) =>
            await _dbSet.AnyAsync(s => s.NationalId == nationalId);

        public async Task<IEnumerable<Student>> GetByLevelAsync(int levelId) =>
            await _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .Where(s => s.LevelId == levelId)
                .ToListAsync();

        public async Task<IEnumerable<Student>> GetByDivisionAsync(int divisionId) =>
            await _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .Where(s => s.DivisionId == divisionId)
                .ToListAsync();

        public async Task<IEnumerable<Student>> GetByLevelAndDivisionAsync(int levelId, int divisionId) =>
            await _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .Where(s => s.LevelId == levelId && s.DivisionId == divisionId)
                .ToListAsync();

        public async Task<IEnumerable<Student>> GetAllWithDetailsAsync() =>
            await _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .OrderBy(s => s.FullName)
                .ToListAsync();

        public async Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedWithDetailsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            int? levelId = null,
            int? divisionId = null)
        {
            var query = _dbSet
                .Include(s => s.Level)
                .Include(s => s.Division)
                .AsQueryable();

            if (levelId.HasValue)
                query = query.Where(s => s.LevelId == levelId.Value);

            if (divisionId.HasValue)
                query = query.Where(s => s.DivisionId == divisionId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s =>
                    s.FullName.Contains(term) ||
                    s.NationalId.Contains(term));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }
    }
}