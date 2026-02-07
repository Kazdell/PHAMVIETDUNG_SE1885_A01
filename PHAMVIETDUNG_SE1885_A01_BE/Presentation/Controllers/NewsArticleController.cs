using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticleController : ControllerBase
    {
        private readonly INewsArticleService _service;
        private readonly IWebHostEnvironment _environment; // Injected IWebHostEnvironment

        public NewsArticleController(INewsArticleService service, IWebHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }

        [EnableQuery]
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
        public async Task<IActionResult> Post([FromForm] PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models.NewsArticleModel model)
        {
            try
            {
                string? imagePath = await SaveImage(model.ImageFile);

                var news = new NewsArticle
                {
                    NewsArticleId = model.NewsArticleId, // Or generate new if not provided? Frontend creates ID usually?
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    CreatedDate = model.CreatedDate ?? DateTime.Now,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    NewsImage = imagePath, // Added NewsImage
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
        public async Task<IActionResult> Put(string id, [FromForm] PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models.NewsArticleModel model)
        {
            if (id != model.NewsArticleId) return BadRequest("ID mismatch");
            try
            {
                string? imagePath = await SaveImage(model.ImageFile);

                var news = new NewsArticle
                {
                    NewsArticleId = model.NewsArticleId,
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    // CreatedDate = model.CreatedDate, // Service ignores these for update usually
                    // CreatedById = model.CreatedById,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    CategoryId = model.CategoryId,
                    NewsStatus = model.NewsStatus,
                    UpdatedById = model.UpdatedById
                    // ModifiedDate handled by service
                };

                // Only update image if a new file is uploaded
                if (!string.IsNullOrEmpty(imagePath))
                {
                    news.NewsImage = imagePath;
                }
                // Logic to keep old image if imagePath is null needs to be handled in Service or assumed here. 
                // For now, I'll pass it to Service. If Service replaces whole object, we might lose old image. 
                // Better: Fetch existing news here? Or trust Service handles partial updates?
                // The current Service probably does Entity Framework Update which might overwrite nulls.
                // Recommendation: Pass the image path. If null, the Service should ideally not overwrite it, 
                // OR we fetch the original here.
                
                // Let's check if we can fetch original here to be safe.
                var original = _service.GetNewsById(id);
                if (original != null && string.IsNullOrEmpty(imagePath))
                {
                     news.NewsImage = original.NewsImage;
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
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new Exception("File size exceeds the 5MB limit.");
            }

            // 2. Validate Type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
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
    }
}
