using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;
using System.Net.Http.Json;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly INewsArticleRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;

        public RecommendationService(INewsArticleRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
        }

        public IEnumerable<NewsArticle> GetTrendingNews(int top = 5)
        {
            try
            {
                // Try calling AnalyticsAPI first
                var client = _httpClientFactory.CreateClient("AnalyticsClient");
                var response = client.GetAsync("/api/Trending").Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var trendingData = response.Content.ReadFromJsonAsync<List<TrendingItem>>().Result;
                    if (trendingData != null && trendingData.Any())
                    {
                        // Map IDs back to NewsArticle entities from local repository
                        var trendingIds = trendingData.Select(t => t.Id).ToList();
                        var articles = _repository.GetAll()
                            .Where(n => trendingIds.Contains(n.NewsArticleId))
                            .ToList();
                        
                        // Preserve order from AnalyticsAPI
                        return trendingIds
                            .Select(id => articles.FirstOrDefault(a => a.NewsArticleId == id))
                            .Where(a => a != null)
                            .Take(top)
                            .ToList()!;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecommendationService] AnalyticsAPI call failed, falling back to local logic: {ex.Message}");
            }

            // Fallback: Local logic
            return _repository.GetActiveNews()
                .OrderByDescending(n => n.ViewCount)
                .Take(top)
                .ToList();
        }

        public IEnumerable<NewsArticle> GetRecommendedNews(short? userId, int top = 5)
        {
            // Placeholder: Implement advanced logic later (e.g. based on user's past interaction)
            // For now, return Trending news as default recommendation
            return GetTrendingNews(top);
        }

        private class TrendingItem
        {
            public string Id { get; set; } = string.Empty;
            public string? Title { get; set; }
            public string? Category { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}
