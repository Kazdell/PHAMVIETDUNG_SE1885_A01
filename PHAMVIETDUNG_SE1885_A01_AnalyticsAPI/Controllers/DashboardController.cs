using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace FUNewsManagementSystem.AnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public DashboardController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalArticles = await _context.NewsArticles.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalAuthors = await _context.SystemAccounts.CountAsync();
            
            var statusBreakdown = await _context.NewsArticles
                .GroupBy(n => n.NewsStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Status == true ? "Active" : "Inactive", v => v.Count);

            var topCategories = await _context.NewsArticles
                .Include(n => n.Category)
                .Where(n => n.CategoryId != null)
                .GroupBy(n => n.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var stats = new
            {
                TotalArticles = totalArticles,
                TotalCategories = totalCategories,
                TotalAuthors = totalAuthors,
                StatusBreakdown = statusBreakdown,
                TopCategories = topCategories
            };

            return Ok(stats);
        }
    }
}
