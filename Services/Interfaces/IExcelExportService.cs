using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface IExcelExportService
    {
        byte[] ExportStudentsToExcel(IEnumerable<Student> students);
    }
}