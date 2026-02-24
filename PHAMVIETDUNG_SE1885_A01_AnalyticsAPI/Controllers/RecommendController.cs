using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace FUNewsManagementSystem.AnalyticsAPI.Controllers
{
    [Route("api/analytics/[controller]")]
    [ApiController]
    public class RecommendController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public RecommendController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecommended(string id)
        {
            var sourceArticle = await _context.NewsArticles
                .Include(n => n.NewsTags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);

            if (sourceArticle == null) return NotFound("Article not found");

            // Suggest related articles based on category or tags
            var tagIds = sourceArticle.NewsTags.Select(nt => nt.TagId).ToList();

            var relatedArticles = await _context.NewsArticles
                .Include(n => n.Category)
                .Where(n => n.NewsStatus == true && n.NewsArticleId != id)
                .Where(n => n.CategoryId == sourceArticle.CategoryId || n.NewsTags.Any(nt => tagIds.Contains(nt.TagId)))
                .OrderByDescending(n => n.CreatedDate)
                .Take(3)
                .Select(n => new 
                { 
                    Id = n.NewsArticleId, 
                    Title = n.NewsTitle, 
                    Category = n.Category.CategoryName,
                    Date = n.CreatedDate,
                    ViewCount = n.ViewCount
                })
                .ToListAsync();

            return Ok(relatedArticles);
        }
    }
}
