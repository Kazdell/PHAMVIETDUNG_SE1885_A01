using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace FUNewsManagementSystem.AnalyticsAPI.Controllers
{
    [Route("api/analytics/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public ExportController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedDate <= endDate.Value);

            var articles = await query.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("News Articles");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Title";
                worksheet.Cell(currentRow, 3).Value = "Headline";
                worksheet.Cell(currentRow, 4).Value = "Category";
                worksheet.Cell(currentRow, 5).Value = "Author";
                worksheet.Cell(currentRow, 6).Value = "Date";
                worksheet.Cell(currentRow, 7).Value = "Status";

                foreach (var article in articles)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = article.NewsArticleId;
                    worksheet.Cell(currentRow, 2).Value = article.NewsTitle;
                    worksheet.Cell(currentRow, 3).Value = article.Headline;
                    worksheet.Cell(currentRow, 4).Value = article.Category?.CategoryName;
                    worksheet.Cell(currentRow, 5).Value = article.CreatedBy?.AccountName;
                    worksheet.Cell(currentRow, 6).Value = article.CreatedDate?.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 7).Value = article.NewsStatus == true ? "Active" : "Inactive";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NewsReport.xlsx");
                }
            }
        }
    }
}
