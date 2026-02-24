using System.Collections.Concurrent;
using System.Text.Json;

namespace FUNewsManagementSystem.AiAPI
{
  public class LearningCache
  {
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _cache = new();
    private static readonly string _cacheFilePath = Path.Combine(AppContext.BaseDirectory, "learning_cache.json");
    private static readonly object _fileLock = new();

    static LearningCache()
    {
      LoadFromFile();
    }

    private static void LoadFromFile()
    {
      try
      {
        if (File.Exists(_cacheFilePath))
        {
          var json = File.ReadAllText(_cacheFilePath);
          var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
          if (data != null)
          {
            foreach (var kvp in data)
            {
              _cache[kvp.Key] = new ConcurrentDictionary<string, int>(kvp.Value);
            }
          }
          Console.WriteLine($"[LearningCache] Loaded {_cache.Count} keywords from cache file.");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[LearningCache] Failed to load cache: {ex.Message}");
      }
    }

    private static void SaveToFile()
    {
      try
      {
        lock (_fileLock)
        {
          var data = _cache.ToDictionary(
              kvp => kvp.Key,
              kvp => kvp.Value.ToDictionary(x => x.Key, x => x.Value)
          );
          var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
          File.WriteAllText(_cacheFilePath, json);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[LearningCache] Failed to save cache: {ex.Message}");
      }
    }

    public void Learn(string tag, IEnumerable<string> keywords)
    {
      foreach (var keyword in keywords)
      {
        var tagCounts = _cache.GetOrAdd(keyword.ToLower(), _ => new ConcurrentDictionary<string, int>());
        tagCounts.AddOrUpdate(tag, 1, (_, count) => count + 1);
      }
      SaveToFile(); // Persist after learning
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
