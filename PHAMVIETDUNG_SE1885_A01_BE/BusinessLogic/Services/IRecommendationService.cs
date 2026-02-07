using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public interface IRecommendationService
    {
        IEnumerable<NewsArticle> GetTrendingNews(int top = 5);
        IEnumerable<NewsArticle> GetRecommendedNews(short? userId, int top = 5);
    }
}
