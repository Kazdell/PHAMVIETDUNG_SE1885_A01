using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Hubs;
using System.Net.Http.Json;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _repository;
        private readonly IGenericRepository<NewsTag> _newsTagRepo;
        private readonly IAuditService _auditService;
        private readonly IHubContext<BusinessLogic.Hubs.NotificationHub> _hubContext;
        private readonly IHubContext<BusinessLogic.Hubs.AdminDashboardHub> _adminHub;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITagRepository _tagRepository;

        public NewsArticleService(
            INewsArticleRepository repository, 
            IGenericRepository<NewsTag> newsTagRepo,
            IAuditService auditService,
            IHubContext<BusinessLogic.Hubs.NotificationHub> hubContext,
            IHubContext<BusinessLogic.Hubs.AdminDashboardHub> adminHub,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IHttpClientFactory httpClientFactory,
            ITagRepository tagRepository)
        {
            _repository = repository;
            _newsTagRepo = newsTagRepo;
            _auditService = auditService;
            _hubContext = hubContext;
            _adminHub = adminHub;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _httpClientFactory = httpClientFactory;
            _tagRepository = tagRepository;
        }

        private string GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system@local";
        }

        public IEnumerable<NewsArticle> GetAllNews()
        {
            return _repository.GetAll();
        }

        public IEnumerable<NewsArticle> GetActiveNews()
        {
             return _repository.GetActiveNews();
        }

        public NewsArticle GetNewsById(string id)
        {
            var news = _repository.GetById(id);
            if (news != null)
            {
                news.ViewCount++;
                _repository.Update(news);
                _adminHub.Clients.All.SendAsync("ReceiveArticleView", new { ArticleId = news.NewsArticleId, AuthorId = news.CreatedById });
            }
            return news;
        }

        public async Task CreateNewsAsync(NewsArticle news, List<int>? tagIds = null)
        {
            news.CreatedDate = DateTime.Now;
            news.ModifiedDate = DateTime.Now;
            _repository.Insert(news);

            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _newsTagRepo.Insert(new NewsTag { NewsArticleId = news.NewsArticleId, TagId = tagId });
                }
            }

            // Audit (awaited)
            await _auditService.LogActionAsync(GetCurrentUserEmail(), "Create", "NewsArticle", news.NewsArticleId, null, news);
            
            // Notification
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New Article Published: {news.NewsTitle}");
            await _adminHub.Clients.All.SendAsync("ReceiveNewArticle", new { Title = news.NewsTitle, AuthorId = news.CreatedById, ArticleId = news.NewsArticleId });

            // Persist Notification for Author (Confirmation)
            await _notificationService.CreateNotificationAsync(news.CreatedById ?? 0, "Article Published", $"Your article '{news.NewsTitle}' has been published successfully.", news.NewsArticleId);

            // Broadcast Notification to ALL users (except author) - saved to DB for offline users
            await _notificationService.BroadcastToAllAsync("New Article", $"'{news.NewsTitle}' has been published.", news.CreatedById, news.NewsArticleId);

            // Call AiAPI to learn from article content and tags
            if (tagIds != null && tagIds.Any() && !string.IsNullOrEmpty(news.NewsContent))
            {
                try
                {
                    var tagNames = _tagRepository.GetAll().Where(t => tagIds.Contains(t.TagId)).Select(t => t.TagName).ToList();
                    var learnPayload = new { Content = news.NewsContent, Tags = tagNames };
                    var aiClient = _httpClientFactory.CreateClient("AiClient");
                    await aiClient.PostAsJsonAsync("/api/suggesttags/learn", learnPayload);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the article creation
                    Console.WriteLine($"[NewsArticleService] AiAPI Learn call failed: {ex.Message}");
                }
            }
        }

        public async Task UpdateNewsAsync(NewsArticle news, List<int>? tagIds = null)
        {
            var existing = _repository.GetById(news.NewsArticleId);
            if (existing != null)
            {
                // Capture old values before update
                var oldValues = new { existing.NewsTitle, existing.Headline, existing.NewsContent, existing.CategoryId, existing.NewsStatus };
                
                existing.NewsTitle = news.NewsTitle;
                existing.Headline = news.Headline;
                existing.NewsContent = news.NewsContent;
                existing.NewsSource = news.NewsSource;
                existing.NewsImage = news.NewsImage; // Update image path
                existing.CategoryId = news.CategoryId;
                existing.NewsStatus = news.NewsStatus;
                existing.UpdatedById = news.UpdatedById;
                existing.ModifiedDate = DateTime.Now;
                
                _repository.Update(existing);

                // Audit (awaited)
                await _auditService.LogActionAsync(GetCurrentUserEmail(), "Update", "NewsArticle", news.NewsArticleId, oldValues, news);

                // Update Tags
                if (tagIds != null)
                {
                    // Remove existing tags
                    var currentTags = _newsTagRepo.GetAll().Where(nt => nt.NewsArticleId == news.NewsArticleId).ToList();
                    foreach (var tag in currentTags)
                    {
                         _newsTagRepo.Delete(tag);
                    }

                    // Add new tags
                    foreach (var tagId in tagIds)
                    {
                         _newsTagRepo.Insert(new NewsTag { NewsArticleId = news.NewsArticleId, TagId = tagId });
                    }
                }
                
                // Notification
                await _hubContext.Clients.All.SendAsync("ReceiveArticleUpdate", news.NewsTitle);
            }
        }

        public async Task DeleteNewsAsync(string id)
        {
            var news = _repository.GetById(id);
            if (news == null) throw new KeyNotFoundException("Article not found.");
            
            if (news.NewsStatus == true)
            {
                throw new InvalidOperationException(Common.SystemMessages.GetMessage(Common.SystemMessages.ActiveArticleDeleteError));
            }

            // Manual Cascade Delete for NewsTags
            var tags = _newsTagRepo.GetAll().Where(nt => nt.NewsArticleId == id).ToList();
            foreach (var tag in tags)
            {
                _newsTagRepo.Delete(tag);
            }

            _repository.Delete(id);

            // Audit (awaited)
            await _auditService.LogActionAsync(GetCurrentUserEmail(), "Delete", "NewsArticle", id, null, null);
        }

        public IEnumerable<NewsArticle> SearchNews(string keyword)
        {
            var all = _repository.GetAll();
             if (!string.IsNullOrEmpty(keyword))
             {
                 all = all.Where(n => (n.NewsTitle != null && n.NewsTitle.Contains(keyword)) || (n.Headline != null && n.Headline.Contains(keyword)));
             }
             return all;
        }
        public IEnumerable<NewsArticle> GetRelatedNews(string newsId)
        {
            var currentNews = _repository.GetById(newsId);
            if (currentNews == null) return new List<NewsArticle>();

            // Ensure we have current tags. Repository might lazy load or we query
            var currentTagIds = _newsTagRepo.GetAll().Where(nt => nt.NewsArticleId == newsId).Select(nt => nt.TagId).ToList();

            // Logic: Same Category OR Common Tag
            // Note: EF Core evaluation might be complex if not careful.
            // Using in-memory for simpler logic if dataset small, or composed query.
            // Since repo is Generic and returns IEnumerable (or IQueryable depending on implementation, here IEnumerable from GetAll), 
            // we might be doing client-side evaluation. 
            // Better to use IQueryable if possible, but Repository returns IEnumerable.
            // We'll proceed with Client-side for now as per architecture.
            
            var allActive = _repository.GetAll().Where(n => n.NewsStatus == true && n.NewsArticleId != newsId);
            
            return allActive.Where(n => 
                (currentNews.CategoryId != null && n.CategoryId == currentNews.CategoryId) ||
                (n.NewsTags != null && n.NewsTags.Any(nt => currentTagIds.Contains(nt.TagId)))
            )
            .OrderByDescending(n => n.CreatedDate)
            .Take(5);
        }

        public void DuplicateNews(string id, short userId)
        {
            var original = _repository.GetById(id);
            if (original == null) throw new Exception("Original article not found.");

            var newId = Guid.NewGuid().ToString().Substring(0, 20); // Limit to 20 chars
            
            var copy = new NewsArticle
            {
                NewsArticleId = newId,
                NewsTitle = "Copy of " + original.NewsTitle,
                Headline = original.Headline,
                CreatedDate = DateTime.Now,
                NewsContent = original.NewsContent,
                NewsSource = original.NewsSource,
                CategoryId = original.CategoryId,
                NewsStatus = false, // Default to inactive for safety
                CreatedById = userId,
                UpdatedById = null,
                ModifiedDate = null
            };

            _repository.Insert(copy);

            // Copy Tags
            var originalTags = _newsTagRepo.GetAll().Where(nt => nt.NewsArticleId == id).ToList();
            foreach (var Tag in originalTags)
            {
                _newsTagRepo.Insert(new NewsTag { NewsArticleId = newId, TagId = Tag.TagId });
            }
        }

        public ViewModels.ReportResult GetNewsReport(DateTime? startDate, DateTime? endDate, int? categoryId, short? createdById, bool? status, int page, int pageSize)
        {
            var query = _repository.GetAll().AsQueryable();

            // Filters
            if (startDate.HasValue)
                query = query.Where(n => n.CreatedDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedDate <= endDate.Value);

             if (categoryId.HasValue)
                query = query.Where(n => n.CategoryId == categoryId.Value);

            if (createdById.HasValue)
                query = query.Where(n => n.CreatedById == createdById.Value);

            if (status.HasValue)
                query = query.Where(n => n.NewsStatus == status.Value);

            // Statistics
            var totalRecords = query.Count();
            var activeCount = query.Count(n => n.NewsStatus == true);
            var inactiveCount = query.Count(n => n.NewsStatus == false);
            
            // Should be done before Paging? Pagination applies to the list of articles. Stats apply to the filtered set.
            
            // Pagination & Sorting
            var articleEntities = query.OrderByDescending(n => n.CreatedDate)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var articles = articleEntities.Select(n => new ViewModels.NewsArticleReportViewModel
            {
                NewsArticleId = n.NewsArticleId,
                NewsTitle = n.NewsTitle,
                Headline = n.Headline,
                CreatedDate = n.CreatedDate,
                NewsContent = n.NewsContent,
                NewsSource = n.NewsSource,
                CategoryId = n.CategoryId,
                CategoryName = n.Category?.CategoryName, // Reliance on Lazy Loading or custom loading. If GenericRepo includes it.
                NewsStatus = n.NewsStatus,
                CreatedById = n.CreatedById,
                CreatedByName = n.CreatedBy?.AccountName,
                UpdatedById = n.UpdatedById,
                ModifiedDate = n.ModifiedDate
            }).ToList();
            
            return new ViewModels.ReportResult
            {
                Articles = articles,
                TotalRecords = totalRecords,
                ActiveCount = activeCount,
                InactiveCount = inactiveCount
            };
        }
    }
}
