using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace FUNewsManagementSystem.AnalyticsAPI.Controllers
{
    [Route("api/analytics/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public DashboardController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? status = null)
        {
            // Base query with filters
            var articlesQuery = _context.NewsArticles.AsQueryable();
            
            if (fromDate.HasValue)
                articlesQuery = articlesQuery.Where(n => n.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                articlesQuery = articlesQuery.Where(n => n.CreatedDate <= toDate.Value.AddDays(1));
            if (status.HasValue)
                articlesQuery = articlesQuery.Where(n => n.NewsStatus == status.Value);
            
            var totalArticles = await articlesQuery.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            
            // Count authors who have articles matching the filter
            var totalAuthors = await articlesQuery
                .Where(n => n.CreatedById != null)
                .Select(n => n.CreatedById)
                .Distinct()
                .CountAsync();

            var topCategories = await articlesQuery
                .Include(n => n.Category)
                .Where(n => n.CategoryId != null)
                .GroupBy(n => n.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Top Authors by article count - Single Query version
            var topAuthors = await articlesQuery
                .Where(n => n.CreatedById != null)
                .GroupBy(n => n.CreatedById)
                .Select(g => new { 
                    Author = _context.SystemAccounts
                        .Where(a => a.AccountId == g.Key)
                        .Select(a => a.AccountName)
                        .FirstOrDefault() ?? "Unknown", 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var stats = new
            {
                TotalArticles = totalArticles,
                TotalCategories = totalCategories,
                TotalAuthors = totalAuthors,
                TopCategories = topCategories,
                TopAuthors = topAuthors
            };

            return Ok(stats);
        }
    }
}
