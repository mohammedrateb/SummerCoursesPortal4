using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels;
using WebApplication1.ViewModels.Student;
using StudentModel = WebApplication1.Models.Student;

namespace WebApplication1.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ILevelRepository _levelRepo;
        private readonly IDivisionRepository _divisionRepo;

        private const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 8;

        public StudentService(
            IStudentRepository studentRepo,
            ILevelRepository levelRepo,
            IDivisionRepository divisionRepo)
        {
            _studentRepo = studentRepo;
            _levelRepo = levelRepo;
            _divisionRepo = divisionRepo;
        }

        public async Task<StudentDetailsViewModel?> GetStudentByNationalIdAsync(string nationalId)
        {
            var student = await _studentRepo.GetByNationalIdAsync(nationalId);
            if (student == null) return null;

            return new StudentDetailsViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                NationalId = student.NationalId,
                PhoneNumber = student.PhoneNumber,
                LevelName = student.Level?.Name ?? " ",
                DivisionName = student.Division?.Name ?? " ",
                SubjectCount = student.SubjectCount,
                Subject1 = student.Subject1,
                Subject2 = student.Subject2,
                Subject3 = student.Subject3,
                Subject4 = student.Subject4,
                Subject5 = student.Subject5,
                PaymentStatus = student.PaymentStatus,
                CreatedAt = student.CreatedAt
            };
        }

        public async Task<StudentModel?> GetStudentByIdAsync(int id) =>
            await _studentRepo.GetByIdAsync(id);

        public async Task<ServiceResult> RegisterStudentAsync(StudentRegisterViewModel model)
        {
            if (await _studentRepo.NationalIdExistsAsync(model.NationalId))
                return ServiceResult.Fail("هذا الرقم القومي مسجل من قبل");

            if (model.SubjectCount >= 2 && string.IsNullOrWhiteSpace(model.Subject2))
                return ServiceResult.Fail("المادة الثانية مطلوبة");
            if (model.SubjectCount >= 3 && string.IsNullOrWhiteSpace(model.Subject3))
                return ServiceResult.Fail("المادة الثالثة مطلوبة");
            if (model.SubjectCount >= 4 && string.IsNullOrWhiteSpace(model.Subject4))
                return ServiceResult.Fail("المادة الرابعة مطلوبة");
            if (model.SubjectCount >= 5 && string.IsNullOrWhiteSpace(model.Subject5))
                return ServiceResult.Fail("المادة الخامسة مطلوبة");

            var accessCode = GenerateAccessCode();

            var student = new StudentModel
            {
                FullName = model.FullName,
                NationalId = model.NationalId,
                PhoneNumber = model.PhoneNumber,
                LevelId = model.LevelId,
                DivisionId = model.DivisionId,
                SubjectCount = model.SubjectCount,
                Subject1 = model.Subject1,
                Subject2 = model.SubjectCount >= 2 ? model.Subject2 : null,
                Subject3 = model.SubjectCount >= 3 ? model.Subject3 : null,
                Subject4 = model.SubjectCount >= 4 ? model.Subject4 : null,
                Subject5 = model.SubjectCount >= 5 ? model.Subject5 : null,
                PaymentStatus = false,
                CreatedAt = DateTime.UtcNow,
                AccessCode = accessCode
            };

            await _studentRepo.AddAsync(student);
            return ServiceResult.Ok(accessCode, student.Id);
        }

        public async Task<StudentEditViewModel?> GetStudentForEditAsync(int id, bool isAdmin = false)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return null;

            if (!isAdmin && student.PaymentStatus)
                return null;

            return new StudentEditViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                LevelId = student.LevelId,
                DivisionId = student.DivisionId,
                SubjectCount = student.SubjectCount,
                Subject1 = student.Subject1,
                Subject2 = student.Subject2,
                Subject3 = student.Subject3,
                Subject4 = student.Subject4,
                Subject5 = student.Subject5,
                Levels = await _levelRepo.GetAllLevelsAsync(),
                Divisions = await _divisionRepo.GetAllDivisionsAsync()
            };
        }

        public async Task<ServiceResult> UpdateStudentAsync(StudentEditViewModel model, bool isAdmin = false)
        {
            var student = await _studentRepo.GetByIdAsync(model.Id);
            if (student == null)
                return ServiceResult.Fail("الطالب غير موجود");

            if (!isAdmin && student.PaymentStatus)
                return ServiceResult.Fail("⛔ لا يمكن تعديل بيانات طالب تم سداد رسومه.");

            if (model.SubjectCount >= 2 && string.IsNullOrWhiteSpace(model.Subject2))
                return ServiceResult.Fail("المادة الثانية مطلوبة");
            if (model.SubjectCount >= 3 && string.IsNullOrWhiteSpace(model.Subject3))
                return ServiceResult.Fail("المادة الثالثة مطلوبة");
            if (model.SubjectCount >= 4 && string.IsNullOrWhiteSpace(model.Subject4))
                return ServiceResult.Fail("المادة الرابعة مطلوبة");
            if (model.SubjectCount >= 5 && string.IsNullOrWhiteSpace(model.Subject5))
                return ServiceResult.Fail("المادة الخامسة مطلوبة");

            student.FullName = model.FullName;
            student.LevelId = model.LevelId;
            student.DivisionId = model.DivisionId;
            student.SubjectCount = model.SubjectCount;
            student.Subject1 = model.Subject1;
            student.Subject2 = model.SubjectCount >= 2 ? model.Subject2 : null;
            student.Subject3 = model.SubjectCount >= 3 ? model.Subject3 : null;
            student.Subject4 = model.SubjectCount >= 4 ? model.Subject4 : null;
            student.Subject5 = model.SubjectCount >= 5 ? model.Subject5 : null;

            await _studentRepo.UpdateAsync(student);
            return ServiceResult.Ok("تم التعديل بنجاح");
        }

        public async Task<ServiceResult> DeleteStudentAsync(int id, bool isAdmin = false)
        {
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null)
                return ServiceResult.Fail("الطالب غير موجود");

            if (!isAdmin && student.PaymentStatus)
                return ServiceResult.Fail("⛔ لا يمكن حذف طالب تم سداد رسومه.");

            await _studentRepo.DeleteAsync(id);
            return ServiceResult.Ok("تم الحذف بنجاح");
        }

        public async Task<bool> IsNationalIdRegisteredAsync(string nationalId) =>
            await _studentRepo.NationalIdExistsAsync(nationalId);

        public async Task<IEnumerable<StudentModel>> GetAllStudentsAsync() =>
            await _studentRepo.GetAllWithDetailsAsync();

        public async Task<IEnumerable<StudentModel>> GetFilteredStudentsAsync(int? levelId, int? divisionId)
        {
            if (levelId.HasValue && divisionId.HasValue)
                return await _studentRepo.GetByLevelAndDivisionAsync(levelId.Value, divisionId.Value);

            if (levelId.HasValue)
                return await _studentRepo.GetByLevelAsync(levelId.Value);

            if (divisionId.HasValue)
                return await _studentRepo.GetByDivisionAsync(divisionId.Value);

            return await _studentRepo.GetAllWithDetailsAsync();
        }

        public async Task<ServiceResult> UpdatePaymentStatusAsync(int studentId, bool status)
        {
            var student = await _studentRepo.GetByIdAsync(studentId);
            if (student == null)
                return ServiceResult.Fail("الطالب غير موجود");

            student.PaymentStatus = status;
            await _studentRepo.UpdateAsync(student);
            return ServiceResult.Ok("تم تحديث حالة الدفع");
        }

        public async Task<bool> VerifyAccessCodeAsync(int studentId, string accessCode)
        {
            if (string.IsNullOrWhiteSpace(accessCode)) return false;

            var student = await _studentRepo.GetByIdAsync(studentId);
            if (student == null || string.IsNullOrEmpty(student.AccessCode)) return false;

            return CryptographicEqualsIgnoreCase(student.AccessCode, accessCode.Trim().ToUpperInvariant());
        }

        // ════════════════════════════════════════
        // Pagination
        // ════════════════════════════════════════
        public async Task<(IEnumerable<StudentModel> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            int? levelId = null,
            int? divisionId = null)
        {
            // 1) جلب البيانات مفلترة حسب المستوى والشعبة
            var students = (await GetFilteredStudentsAsync(levelId, divisionId)).AsEnumerable();

            // 2) تطبيق البحث النصي إن وُجد
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                students = students.Where(s =>
                    s.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.NationalId.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            // 3) حساب العدد الكلي قبل التقطيع — ضروري لحساب عدد الصفحات في الـ UI
            var totalCount = students.Count();

            // 4) تطبيق الـ Pagination
            var paged = students
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return (paged, totalCount);
        }

        // ════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════
        private static string GenerateAccessCode()
        {
            var chars = new char[CodeLength];
            var bytes = new byte[CodeLength];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            for (int i = 0; i < CodeLength; i++)
                chars[i] = CodeAlphabet[bytes[i] % CodeAlphabet.Length];
            return new string(chars);
        }

        private static bool CryptographicEqualsIgnoreCase(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= char.ToUpperInvariant(a[i]) ^ char.ToUpperInvariant(b[i]);
            return diff == 0;
        }
    }
}