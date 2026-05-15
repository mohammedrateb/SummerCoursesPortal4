using WebApplication1.ViewModels;
using WebApplication1.ViewModels.Student;

namespace WebApplication1.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDetailsViewModel?> GetStudentByNationalIdAsync(string nationalId);
        Task<WebApplication1.Models.Student?> GetStudentByIdAsync(int id);
        Task<ServiceResult> RegisterStudentAsync(StudentRegisterViewModel model);

        Task<StudentEditViewModel?> GetStudentForEditAsync(int id, bool isAdmin = false);
        Task<ServiceResult> UpdateStudentAsync(StudentEditViewModel model, bool isAdmin = false);
        Task<ServiceResult> DeleteStudentAsync(int id, bool isAdmin = false);

        Task<bool> IsNationalIdRegisteredAsync(string nationalId);
        Task<IEnumerable<WebApplication1.Models.Student>> GetAllStudentsAsync();
        Task<IEnumerable<WebApplication1.Models.Student>> GetFilteredStudentsAsync(int? levelId, int? divisionId);
        Task<ServiceResult> UpdatePaymentStatusAsync(int studentId, bool status);

        // كود الوصول: التحقق منه (للطالب فقط)
        Task<bool> VerifyAccessCodeAsync(int studentId, string accessCode);

        // Pagination جديد
        Task<(IEnumerable<WebApplication1.Models.Student> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            int? levelId = null,
            int? divisionId = null);
    }
}
