using Microsoft.Extensions.Caching.Memory;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public interface IAiCacheService
    {
        Task<IEnumerable<NewsArticle>> GetCachedTrendingNewsAsync();
        // Future: Task<IEnumerable<string>> GetCachedTagsAsync();
        void RefreshTrendingCache();
    }

    public class AiCacheService : IAiCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private const string TrendingCacheKey = "TrendingNews";

        public AiCacheService(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        public async Task<IEnumerable<NewsArticle>> GetCachedTrendingNewsAsync()
        {
            if (!_cache.TryGetValue(TrendingCacheKey, out IEnumerable<NewsArticle> trendingNews))
            {
                // If miss, fetch immediately? Or wait for worker?
                // Better to fetch immediately on miss + set cache.
                trendingNews = await FetchTrendingNewsFromDb();
                _cache.Set(TrendingCacheKey, trendingNews, TimeSpan.FromMinutes(30)); // 30 min default
            }
            return trendingNews;
        }

        public void RefreshTrendingCache()
        {
            // Called by Background Worker
            var trendingNews = FetchTrendingNewsFromDb().Result;
            _cache.Set(TrendingCacheKey, trendingNews, TimeSpan.FromMinutes(30)); 
        }

        private async Task<IEnumerable<NewsArticle>> FetchTrendingNewsFromDb()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
                // We use synchronous GetTrendingNews but wrap in Task
                return await Task.Run(() => recommendationService.GetTrendingNews(5));
            }
        }
    }
}
