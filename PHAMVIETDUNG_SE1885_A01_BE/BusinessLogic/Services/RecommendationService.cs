using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly INewsArticleRepository _repository;

        public RecommendationService(INewsArticleRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<NewsArticle> GetTrendingNews(int top = 5)
        {
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
    }
}
