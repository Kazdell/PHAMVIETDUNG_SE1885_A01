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

        public NewsArticleController(INewsArticleService service)
        {
            _service = service;
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
        public IActionResult Post([FromBody] PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models.NewsArticleModel model)
        {
            try
            {
                var news = new NewsArticle
                {
                    NewsArticleId = model.NewsArticleId, // Or generate new if not provided? Frontend creates ID usually?
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    CreatedDate = model.CreatedDate ?? DateTime.Now,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    CategoryId = model.CategoryId,
                    NewsStatus = model.NewsStatus,
                    CreatedById = model.CreatedById,
                    UpdatedById = model.UpdatedById,
                    ModifiedDate = DateTime.Now
                };

                _service.CreateNews(news, model.SelectedTagIds);
                return CreatedAtAction(nameof(Get), new { key = news.NewsArticleId }, news);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] PHAMVIETDUNG_SE1885_A01_BE.Presentation.Models.NewsArticleModel model)
        {
            if (id != model.NewsArticleId) return BadRequest("ID mismatch");
            try
            {
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

                _service.UpdateNews(news, model.SelectedTagIds);
                return Ok(news);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _service.DeleteNews(id);
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
