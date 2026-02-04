using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A01_FE.Presentation.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A01_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("CoreClient");
            // Base address is already http://localhost:5000. We just append the relative path.
            // Note: ApiSettings in appsettings defines /api suffix, but BaseAddr is root. 
            // We should use /api/... in request or ensure base addr has /api.
            // In Program.cs: BaseAddress = new Uri("http://localhost:5000"); -> No /api
            
            var response = await client.GetAsync("/api/NewsArticle?$filter=NewsStatus eq true&$orderby=CreatedDate desc");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(news);
            }
            return View(new List<NewsArticleViewModel>());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var client = _httpClientFactory.CreateClient("CoreClient");
            var newsResponse = await client.GetAsync($"/api/NewsArticle/{id}");
            if (!newsResponse.IsSuccessStatusCode) return NotFound();

            var content = await newsResponse.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
