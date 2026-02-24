using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
  public interface INewsArticleService
  {
    IEnumerable<NewsArticle> GetAllNews();
    IEnumerable<NewsArticle> GetActiveNews();
    NewsArticle GetNewsById(string id);
    Task CreateNewsAsync(NewsArticle news, List<int>? tagIds = null);
    Task UpdateNewsAsync(NewsArticle news, List<int>? tagIds = null);
    Task DeleteNewsAsync(string id);
    IEnumerable<NewsArticle> SearchNews(string keyword);
    IEnumerable<NewsArticle> GetRelatedNews(string newsId);
    ViewModels.ReportResult GetNewsReport(DateTime? startDate, DateTime? endDate, int? categoryId, short? createdById, bool? status, int page, int pageSize);
    void DuplicateNews(string id, short userId);
    Task IncrementViewCount(string id, short? viewerId);
  }
}
