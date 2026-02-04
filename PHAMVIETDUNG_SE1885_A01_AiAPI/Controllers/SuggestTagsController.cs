using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FUNewsManagementSystem.AiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestTagsController : ControllerBase
    {
        [HttpPost]
        public IActionResult SuggestTags([FromBody] TagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required.");
            }

            // slightly smarter mock logic
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "is", "at", "which", "on", "and", "a", "an", "in", "to", "for", "of", "with", "by", "from", "that", "this", "it"
            };

            var words = Regex.Matches(request.Content, @"\b[\w]{4,}\b") // Words >= 4 chars
                .Select(m => m.Value)
                .Where(w => !stopWords.Contains(w))
                .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Select(g => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.Key.ToLower()))
                .Take(5)
                .ToList();

            if (!words.Any())
            {
                words = new List<string> { "General", "News" };
            }

            return Ok(new { Tags = words });
        }
    }

    public class TagRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
