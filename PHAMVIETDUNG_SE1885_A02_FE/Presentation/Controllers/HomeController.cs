using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;
using Microsoft.AspNetCore.SignalR;
using PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Hubs;
using System.Diagnostics;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A02_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<DashboardHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IHubContext<DashboardHub> hubContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("CoreClient");
            // Base address is already http://localhost:5000. We just append the relative path.
            // Note: ApiSettings in appsettings defines /api suffix, but BaseAddr is root. 
            // We should use /api/... in request or ensure base addr has /api.
            // In Program.cs: BaseAddress = new Uri("http://localhost:5000"); -> No /api
            
            var response = await client.GetAsync("/api/NewsArticle?$filter=NewsStatus eq true&$orderby=CreatedDate desc&$top=12");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(news);
            }
            return View(new List<NewsArticleViewModel>());
        }

        public async Task<IActionResult> Details(string id, string fromNotification = null)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var client = _httpClientFactory.CreateClient("CoreClient");
            var newsResponse = await client.GetAsync($"/api/NewsArticle/{id}");
            
            // Handle deleted/not found articles
            if (!newsResponse.IsSuccessStatusCode)
            {
                ViewBag.ArticleNotFound = true;
                ViewBag.NotificationId = fromNotification;
                ViewBag.ArticleId = id;
                return View("ArticleNotFound");
            }

            var content = await newsResponse.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Trigger SignalR Notification for Live View
            await _hubContext.Clients.All.SendAsync("ReceiveArticleView", id);

            // Fetch Related
            var relatedResponse = await client.GetAsync($"/api/NewsArticle/{id}/Related");
            if (relatedResponse.IsSuccessStatusCode)
            {
                var relatedContent = await relatedResponse.Content.ReadAsStringAsync();
                var related = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(relatedContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewData["RelatedNews"] = related;
            }

            return View(news);
        }

        [HttpPost]
        public async Task<IActionResult> IncrementView(string id)
        {
            var client = _httpClientFactory.CreateClient("CoreClient");
            var response = await client.PostAsync($"/api/NewsArticle/IncrementView/{id}", null);
            if (response.IsSuccessStatusCode) return Ok();
            return BadRequest();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Ping()
        {
            try
            {
                // Bypass Polly for a quick connect check
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.GetAsync("http://localhost:5000/api/NewsArticle?$top=1");
                if (response.IsSuccessStatusCode)
                    return Ok();
            }
            catch
            {
                return StatusCode(503);
            }
            return StatusCode(503);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
