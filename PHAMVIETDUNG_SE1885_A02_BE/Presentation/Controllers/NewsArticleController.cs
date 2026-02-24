using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticleController : ControllerBase
    {
        private readonly INewsArticleService _service;
        private readonly IWebHostEnvironment _environment; // Injected IWebHostEnvironment

        #region Constants
        private const int MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024; // 5MB
        private static readonly string[] ALLOWED_IMAGE_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif" };
        #endregion

        public NewsArticleController(INewsArticleService service, IWebHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }

        [EnableQuery(MaxTop = 100, PageSize = 20)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_service.GetAllNews().AsQueryable());
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var news = _service.GetNewsById(key);
            if (news == null) return NotFound();
            return Ok(news);
        }

        [HttpGet("{key}/Related")]
        public IActionResult GetRelated(string key)
        {
            return Ok(_service.GetRelatedNews(key));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] PHAMVIETDUNG_SE1885_A02_BE.Presentation.Models.NewsArticleModel model)
        {
            try
            {
                // Priority: File upload > URL
                string? imagePath = await SaveImage(model.ImageFile);
                if (string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(model.NewsImage))
                {
                    imagePath = model.NewsImage; // Use URL if no file uploaded
                }

                var news = new NewsArticle
                {
                    NewsArticleId = string.IsNullOrEmpty(model.NewsArticleId) 
                        ? DateTime.Now.Ticks.ToString() 
                        : model.NewsArticleId,
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    CreatedDate = model.CreatedDate ?? DateTime.Now,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    NewsImage = imagePath, // File or URL
                    CategoryId = model.CategoryId,
                    NewsStatus = model.NewsStatus,
                    CreatedById = model.CreatedById,
                    UpdatedById = model.UpdatedById,
                    ModifiedDate = DateTime.Now
                };

                await _service.CreateNewsAsync(news, model.SelectedTagIds);
                return CreatedAtAction(nameof(Get), new { key = news.NewsArticleId }, news);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromForm] PHAMVIETDUNG_SE1885_A02_BE.Presentation.Models.NewsArticleModel model)
        {
            if (id != model.NewsArticleId) return BadRequest("ID mismatch");
            try
            {
                // Priority: File upload > URL > Original image
                string? imagePath = await SaveImage(model.ImageFile);
                if (string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(model.NewsImage))
                {
                    imagePath = model.NewsImage; // Use URL if no file uploaded
                }

                var news = new NewsArticle
                {
                    NewsArticleId = model.NewsArticleId,
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    CategoryId = model.CategoryId,
                    NewsStatus = model.NewsStatus,
                    UpdatedById = model.UpdatedById
                };

                // Set image: use new value if provided, else keep original
                if (!string.IsNullOrEmpty(imagePath))
                {
                    news.NewsImage = imagePath;
                }
                else
                {
                    // Keep original image if no new image provided
                    var original = _service.GetNewsById(id);
                    if (original != null)
                    {
                        news.NewsImage = original.NewsImage;
                    }
                }

                await _service.UpdateNewsAsync(news, model.SelectedTagIds);
                return Ok(news);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<string?> SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Validate Size (Max 5MB)
            if (file.Length > MAX_FILE_SIZE_BYTES)
            {
                throw new Exception("File size exceeds the 5MB limit.");
            }

            // 2. Validate Type
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ALLOWED_IMAGE_EXTENSIONS.Contains(extension))
            {
                throw new Exception("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            // 3. Determine uploads folder - fallback to ContentRootPath if WebRootPath is null
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(rootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteNewsAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/Duplicate")]
        public IActionResult Duplicate(string id, [FromQuery] short userId)
        {
            try
            {
                _service.DuplicateNews(id, userId);
                return Ok("Article duplicated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("Report")]
        public IActionResult GetReport(DateTime? startDate, DateTime? endDate, int? categoryId, short? createdById, bool? status, int page = 1, int pageSize = 12)
        {
            return Ok(_service.GetNewsReport(startDate, endDate, categoryId, createdById, status, page, pageSize));
        }

        [HttpPost("IncrementView/{id}")]
        public async Task<IActionResult> IncrementView(string id)
        {
            // Try to get userId from token if available
            short? userId = null;
            // Assuming we might have a claim or something, or passed via query?
            // The requirement says "dù có đăng nhập tài khoản hay xem bằng kiểu guest".
            // If logged in, we should record it.
            // Check headers or User Object?
            // User is populated by JWT Middleware if token present.
            var accountIdClaim = User.FindFirst("AccountId");
            if (accountIdClaim != null && short.TryParse(accountIdClaim.Value, out short accId))
            {
                userId = accId;
            }

            await _service.IncrementViewCount(id, userId);
            return Ok();
        }
    }
}
