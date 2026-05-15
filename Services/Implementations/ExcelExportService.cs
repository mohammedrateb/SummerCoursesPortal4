using ClosedXML.Excel;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] ExportStudentsToExcel(IEnumerable<Student> students)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("بيانات الطلاب");

            // ✅ العناوين
            worksheet.Cell(1, 1).Value = "م";
            worksheet.Cell(1, 2).Value = "الاسم";
            worksheet.Cell(1, 3).Value = "الرقم القومي";
            worksheet.Cell(1, 4).Value = "رقم الموبايل";
            worksheet.Cell(1, 5).Value = "المستوى";
            worksheet.Cell(1, 6).Value = "الشعبة";
            worksheet.Cell(1, 7).Value = "عدد المواد";
            worksheet.Cell(1, 8).Value = "المادة الأولى";
            worksheet.Cell(1, 9).Value = "المادة الثانية";
            worksheet.Cell(1, 10).Value = "المادة الثالثة";
            worksheet.Cell(1, 11).Value = "المادة الرابعة";
            worksheet.Cell(1, 12).Value = "المادة الخامسة";
            worksheet.Cell(1, 13).Value = "حالة الدفع";
            worksheet.Cell(1, 14).Value = "تاريخ التسجيل";

            // ✅ تنسيق العناوين
            var headerRange = worksheet.Range(1, 1, 1, 12);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ✅ البيانات
            var studentList = students.ToList();
            for (int i = 0; i < studentList.Count; i++)
            {
                var s = studentList[i];
                int row = i + 2;

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = s.FullName;
                worksheet.Cell(row, 3).Value = s.NationalId;
                worksheet.Cell(row, 4).Value = s.PhoneNumber;
                worksheet.Cell(row, 5).Value = s.Level?.Name ?? "";
                worksheet.Cell(row, 6).Value = s.Division?.Name ?? "";
                worksheet.Cell(row, 7).Value = s.SubjectCount;
                worksheet.Cell(row, 8).Value = s.Subject1;
                worksheet.Cell(row, 9).Value = s.Subject2 ?? "-";
                worksheet.Cell(row, 10).Value = s.Subject3 ?? "-";
                worksheet.Cell(row, 11).Value = s.Subject4 ?? "-";
                worksheet.Cell(row, 12).Value = s.Subject5 ?? "-";
                worksheet.Cell(row, 13).Value = s.PaymentStatus ? "تم الدفع" : "لم يتم الدفع";
                worksheet.Cell(row, 14).Value = s.CreatedAt.ToString("yyyy-MM-dd");

                // تلوين صفوف متناوبة
                if (i % 2 == 0)
                    worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            }

            // ✅ ضبط عرض الأعمدة تلقائياً
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}