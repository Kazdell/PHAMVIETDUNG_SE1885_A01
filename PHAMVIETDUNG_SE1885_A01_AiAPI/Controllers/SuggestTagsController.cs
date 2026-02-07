using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FUNewsManagementSystem.AiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestTagsController : ControllerBase
    {
        private readonly LearningCache _cache;

        public SuggestTagsController(LearningCache cache)
        {
            _cache = cache;
        }

        [HttpPost]
        public IActionResult SuggestTags([FromBody] TagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required.");
            }

            // Expanded stop words
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "the", "is", "at", "which", "on", "and", "a", "an", "in", "to", "for", "of", "with", "by", "from", "that", "this", "it", "are", "was", "were", "been", "being", "have", "has", "had", "do", "does", "did", "but", "if", "or", "because", "as", "until", "while", "about", "against", "between", "into", "through", "during", "before", "after", "above", "below", "up", "down", "out", "off", "over", "under", "again", "further", "then", "once", "here", "there", "when", "where", "why", "how", "all", "any", "both", "each", "few", "more", "most", "other", "some", "such", "no", "nor", "not", "only", "own", "same", "so", "than", "too", "very", "can", "will", "just", "should", "now", "also", "many", "more", "well" };

            // 1. Extract Named Entities
            var entities = Regex.Matches(request.Content, @"\b[A-Z][a-z0-9]{2,19}\b")
                .Select(m => m.Value)
                .Where(w => !stopWords.Contains(w))
                .ToList();

            // 2. Extract Keywords
            var keywords = Regex.Matches(request.Content, @"\b[a-zA-Z][a-zA-Z0-9]{2,19}\b") 
                .Select(m => m.Value.ToLower())
                .Where(w => !stopWords.Contains(w))
                .ToList();

            var stemmedKeywords = keywords
                .Select(w => {
                    if (w.EndsWith("ies") && w.Length > 5) return w.Substring(0, w.Length - 3) + "y";
                    if (w.EndsWith("es") && w.Length > 4) return w.Substring(0, w.Length - 2);
                    if (w.EndsWith("s") && !w.EndsWith("ss") && w.Length > 3) return w.Substring(0, w.Length - 1);
                    if (w.EndsWith("ing") && w.Length > 6) return w.Substring(0, w.Length - 3);
                    return w;
                })
                .Distinct()
                .ToList();

            // 3. Learning Cache (Top Tags based on keywords)
            var learnedTags = _cache.GetTopTags(stemmedKeywords);

            // 4. Semantic Topic Mapping
            var topicMapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Scholarship", new List<string> { "Education", "Resources" } },
                { "Student", new List<string> { "Campus Life", "Education" } },
                { "Wednesday", new List<string> { "Events" } },
                { "Session", new List<string> { "Events", "Information" } },
                { "Program", new List<string> { "Education" } },
                { "Exchange", new List<string> { "Campus Life", "Global" } },
                { "London", new List<string> { "Global" } },
                { "Application", new List<string> { "Resources", "Education" } },
                { "Housing", new List<string> { "Resources", "Campus Life" } },
                { "Course", new List<string> { "Education" } },
                { "Engagement", new List<string> { "Campus Life" } },
                { "Culture", new List<string> { "Campus Life", "Global" } },
                { "Credit", new List<string> { "Education" } }
            };

            var extraTags = new List<string>();
            foreach (var kw in stemmedKeywords)
            {
                if (topicMapping.TryGetValue(kw, out var mapped)) extraTags.AddRange(mapped);
            }

            var words = entities
                .Concat(learnedTags)
                .Concat(extraTags)
                .Concat(stemmedKeywords.Select(k => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(k)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(20)
                .ToList();

            if (words.Count < 5)
            {
                var standard = new List<string> { "News", "Breaking", "General", "Update", "Information", "Education", "Events" };
                words = words.Concat(standard).Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToList();
            }

            return Ok(new { Tags = words });
        }

        [HttpPost("learn")]
        public IActionResult Learn([FromBody] LearnRequest request)
        {
            if (request.Tags == null || !request.Tags.Any() || string.IsNullOrEmpty(request.Content))
                return BadRequest();

            // Extract keywords from content to associate with tags
            var keywords = Regex.Matches(request.Content, @"\b[a-zA-Z]{3,20}\b")
                .Select(m => m.Value.ToLower())
                .Distinct();

            foreach (var tag in request.Tags)
            {
                _cache.Learn(tag, keywords);
            }

            return Ok();
        }
    }

    public class LearnRequest
    {
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class TagRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
