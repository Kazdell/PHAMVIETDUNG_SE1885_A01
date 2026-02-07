using System.Collections.Concurrent;

namespace FUNewsManagementSystem.AiAPI
{
    public class LearningCache
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _cache = new();

        public void Learn(string tag, IEnumerable<string> keywords)
        {
            foreach (var keyword in keywords)
            {
                var tagCounts = _cache.GetOrAdd(keyword.ToLower(), _ => new ConcurrentDictionary<string, int>());
                tagCounts.AddOrUpdate(tag, 1, (_, count) => count + 1);
            }
        }

        public List<string> GetTopTags(IEnumerable<string> keywords, int limit = 5)
        {
            var aggregatedCounts = new Dictionary<string, int>();

            foreach (var keyword in keywords)
            {
                if (_cache.TryGetValue(keyword.ToLower(), out var tagCounts))
                {
                    foreach (var kvp in tagCounts)
                    {
                        if (aggregatedCounts.ContainsKey(kvp.Key))
                            aggregatedCounts[kvp.Key] += kvp.Value;
                        else
                            aggregatedCounts[kvp.Key] = kvp.Value;
                    }
                }
            }

            return aggregatedCounts
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(limit)
                .ToList();
        }
    }
}
