using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recService;
        private readonly IAiCacheService _cacheService;

        public RecommendationController(IRecommendationService recService, IAiCacheService cacheService)
        {
            _recService = recService;
            _cacheService = cacheService;
        }

        [HttpGet("Trending")]
        public async Task<IActionResult> GetTrending()
        {
            // Use Cached version
            var trending = await _cacheService.GetCachedTrendingNewsAsync();
            return Ok(trending);
        }

        [HttpGet("Recommend")]
        public async Task<IActionResult> GetRecommended([FromQuery] short? userId)
        {
            // For now, Recommend falls back to Trending, so use Cache too
            // Once personalized logic is added, use _recService.GetRecommendedNews
             var trending = await _cacheService.GetCachedTrendingNewsAsync();
             return Ok(trending);
        }
    }
}
