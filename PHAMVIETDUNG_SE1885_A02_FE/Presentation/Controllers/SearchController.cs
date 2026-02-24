using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.Controllers;

public class SearchController : Controller
{
  private readonly IHttpClientFactory _httpClientFactory;

  public SearchController(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  public async Task<IActionResult> Index()
  {
    var client = _httpClientFactory.CreateClient("CoreClient");

    // 1. Fetch Categories
    var catResponse = await client.GetAsync("/api/Category");
    if (catResponse.IsSuccessStatusCode)
    {
      var content = await catResponse.Content.ReadAsStringAsync();
      ViewData["Categories"] = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // 2. Fetch Tags
    var tagResponse = await client.GetAsync("/api/Tag");
    if (tagResponse.IsSuccessStatusCode)
    {
      var content = await tagResponse.Content.ReadAsStringAsync();
      ViewData["Tags"] = JsonSerializer.Deserialize<List<TagViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // 3. Fetch Authors
    var accResponse = await client.GetAsync("/api/SystemAccount");
    if (accResponse.IsSuccessStatusCode)
    {
      var content = await accResponse.Content.ReadAsStringAsync();
      ViewData["Authors"] = JsonSerializer.Deserialize<List<SystemAccountViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    return View();
  }

  [HttpGet]
  public async Task<IActionResult> Results(string keyword, int? categoryId, int? tagId, short? createdById, DateTime? minDate, DateTime? maxDate, string sort = "date_desc")
  {
    var client = _httpClientFactory.CreateClient("CoreClient");
    var query = new List<string>();
    var filters = new List<string>();

    // Always filter active news for public search? User didn't specify access control for Global Search.
    // Assuming public search -> Active only.
    filters.Add("NewsStatus eq true");

    if (!string.IsNullOrEmpty(keyword))
    {
      string k = keyword.Replace("'", "''");
      var orConditions = new List<string>
            {
                $"contains(NewsTitle, '{k}')",
                $"contains(Headline, '{k}')",
                $"contains(NewsContent, '{k}')"
            };
      filters.Add($"({string.Join(" or ", orConditions)})");
    }

    if (categoryId.HasValue)
    {
      filters.Add($"CategoryId eq {categoryId.Value}");
    }

    if (tagId.HasValue)
    {
      // OData 4 Any operator on Collection
      filters.Add($"NewsTags/any(nt: nt/TagId eq {tagId.Value})");
    }

    if (createdById.HasValue)
    {
      filters.Add($"CreatedById eq {createdById.Value}");
    }

    if (minDate.HasValue)
    {
      filters.Add($"CreatedDate ge {minDate.Value:yyyy-MM-dd}T00:00:00Z");
    }

    if (maxDate.HasValue)
    {
      filters.Add($"CreatedDate le {maxDate.Value:yyyy-MM-dd}T23:59:59Z");
    }

    if (filters.Any())
    {
      query.Add("$filter=" + string.Join(" and ", filters));
    }

    // Sort
    string orderBy = "CreatedDate desc"; // Default
    switch (sort)
    {
      case "date_asc": orderBy = "CreatedDate asc"; break;
      case "title_asc": orderBy = "NewsTitle asc"; break;
      case "title_desc": orderBy = "NewsTitle desc"; break;
    }
    query.Add($"$orderby={orderBy}");

    // Expand
    query.Add("$expand=Category,NewsTags($expand=Tag),CreatedBy");

    string queryString = string.Join("&", query);
    // Using OData endpoint
    var url = $"/api/NewsArticle?{queryString}";

    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var results = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      return PartialView("_SearchResultsPartial", results);
    }

    return PartialView("_SearchResultsPartial", new List<NewsArticleViewModel>());
  }
}
