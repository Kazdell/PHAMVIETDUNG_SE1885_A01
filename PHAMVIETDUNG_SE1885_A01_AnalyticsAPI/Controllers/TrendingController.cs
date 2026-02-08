using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace FUNewsManagementSystem.AnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrendingController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public TrendingController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendingArticles()
        {
            // Logic: For now, return the latest 5 active articles as "Trending"
            // In a real system, we would track "Views" and order by that.
            var trending = await _context.NewsArticles
                .Where(n => n.NewsStatus == true)
                .OrderByDescending(n => n.ViewCount) // Real trending logic
                .Take(5)
                .Select(n => new 
                { 
                    Id = n.NewsArticleId, 
                    Title = n.NewsTitle, 
                    Category = n.Category.CategoryName,
                    Date = n.CreatedDate,
                    ViewCount = n.ViewCount // Include ViewCount
                })
                .ToListAsync();

            return Ok(trending);
        }
    }
}
