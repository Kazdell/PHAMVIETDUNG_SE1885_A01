using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;
using System.Text.Json;
using System.Text;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.Controllers;

public class StaffController(IHttpClientFactory httpClientFactory) : Controller
{
  private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

  private bool IsStaff()
  {
    var role = HttpContext.Session.GetString("AccountRole");
    return role == "1"; // Assuming 1 is Staff
  }

  // ================= CATEGORIES =================
  public async Task<IActionResult> Dashboard()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var accountId = HttpContext.Session.GetString("AccountId");

    var viewModel = new DashboardViewModel();

    // 1. Get Global Stats from Analytics API
    var analyticsClient = httpClientFactory.CreateClient("AnalyticsClient");
    var analyticsResponse = await analyticsClient.GetAsync("/api/analytics/Dashboard");
    if (analyticsResponse.IsSuccessStatusCode)
    {
      var content = await analyticsResponse.Content.ReadAsStringAsync();
      var globalStats = JsonSerializer.Deserialize<DashboardViewModel>(content, JsonOptions);
      if (globalStats != null)
      {
        viewModel.TotalArticles = globalStats.TotalArticles;
        viewModel.TotalCategories = globalStats.TotalCategories;
        viewModel.TotalAuthors = globalStats.TotalAuthors;
      }
    }

    // 2. Get Personal Stats from Core API using Report Endpoint
    // We use the Report endpoint which supports filtering by CreatedById
    var coreClient = httpClientFactory.CreateClient("CoreClient");
    // /api/NewsArticle/Report?createdById={accountId}&pageSize=1 (we only need the count)
    var myReportResponse = await coreClient.GetAsync($"/api/NewsArticle/Report?createdById={accountId}&pageSize=1");
    if (myReportResponse.IsSuccessStatusCode)
    {
      var content = await myReportResponse.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<ReportResult>(content, JsonOptions);
      viewModel.MyTotalArticles = result?.TotalRecords ?? 0;
    }

    return View(viewModel);
  }

  public class DashboardViewModel
  {
    public int TotalArticles { get; set; }
    public int TotalCategories { get; set; }
    public int TotalAuthors { get; set; }
    public int MyTotalArticles { get; set; }
  }

  public class ReportResult
  {
    public int TotalRecords { get; set; }
    // We don't need other fields for dashboard count
  }

  [HttpGet]
  public async Task<IActionResult> Export(DateTime? startDate, DateTime? endDate)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var client = httpClientFactory.CreateClient("AnalyticsClient");
    var url = $"/api/analytics/Export?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsByteArrayAsync();
      return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NewsReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }
    return BadRequest("Failed to export report.");
  }

  [HttpGet]
  public async Task<IActionResult> GetTrendingNews()
  {
    if (!IsStaff()) return Unauthorized();
    var client = httpClientFactory.CreateClient("AnalyticsClient");
    var response = await client.GetAsync("/api/analytics/Trending");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      return Content(content, "application/json");
    }
    return StatusCode((int)response.StatusCode);
  }

  public async Task<IActionResult> Categories(int page = 1)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync("/api/Category");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, JsonOptions) ?? [];

      // Pagination
      const int pageSize = 10;
      int totalRecords = allCategories.Count;
      int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

      var pagedCategories = allCategories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = totalPages;

      return View(pagedCategories);
    }
    return View(new List<CategoryViewModel>());
  }

  [HttpGet]
  public async Task<IActionResult> CreateCategory()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    // Re-populate Parent Categories for Dropdown
    var response = await client.GetAsync("/api/Category");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, JsonOptions);
      ViewBag.ParentCategories = allCategories;
    }
    else
    {
      ViewBag.ParentCategories = new List<CategoryViewModel>();
    }

    return PartialView("~/Presentation/Views/Staff/_CreateCategoryPartial.cshtml");
  }

  [HttpPost]
  public async Task<IActionResult> CreateCategory(CategoryViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });

    var client = httpClientFactory.CreateClient("CoreClient");
    if (ModelState.IsValid)
    {
      var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
      var response = await client.PostAsync("/api/Category", jsonContent);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }
      var error = await response.Content.ReadAsStringAsync();
      ModelState.AddModelError("", $"Failed to create category: {error}");
    }
    // Re-populate Parent Categories for Dropdown if failure
    var catResponse = await client.GetAsync("/api/Category");
    if (catResponse.IsSuccessStatusCode)
    {
      var content = await catResponse.Content.ReadAsStringAsync();
      var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, JsonOptions);
      ViewBag.ParentCategories = allCategories;
    }
    else
    {
      ViewBag.ParentCategories = new List<CategoryViewModel>();
    }
    return PartialView("~/Presentation/Views/Staff/_CreateCategoryPartial.cshtml", model);
  }

  [HttpGet]
  public async Task<IActionResult> EditCategory(int? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/Category/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var category = JsonSerializer.Deserialize<CategoryViewModel>(content, JsonOptions);

      // Re-populate Parent Categories
      var catResponse = await client.GetAsync("/api/Category");
      if (catResponse.IsSuccessStatusCode)
      {
        var catContent = await catResponse.Content.ReadAsStringAsync();
        var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, JsonOptions) ?? [];
        ViewBag.ParentCategories = allCategories.Where(c => c.CategoryId != id).ToList(); // Exclude self
      }
      else
      {
        ViewBag.ParentCategories = new List<CategoryViewModel>();
      }

      return PartialView("~/Presentation/Views/Staff/_EditCategoryPartial.cshtml", category);
    }
    return NotFound();
  }

  [HttpPost]
  public async Task<IActionResult> EditCategory(int id, CategoryViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
    if (id != model.CategoryId) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    if (ModelState.IsValid)
    {
      var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
      var response = await client.PutAsync($"/api/Category/{id}", jsonContent);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }
      ModelState.AddModelError("", "Failed to update category.");
    }
    // Re-populate Parent Categories
    var catResponse = await client.GetAsync("/api/Category");
    if (catResponse.IsSuccessStatusCode)
    {
      var catContent = await catResponse.Content.ReadAsStringAsync();
      var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, JsonOptions) ?? [];
      ViewBag.ParentCategories = allCategories.Where(c => c.CategoryId != id).ToList();
    }
    else
    {
      ViewBag.ParentCategories = new List<CategoryViewModel>();
    }
    return PartialView("~/Presentation/Views/Staff/_EditCategoryPartial.cshtml", model);
  }

  [HttpGet]
  public async Task<IActionResult> DeleteCategory(int? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/Category/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var category = JsonSerializer.Deserialize<CategoryViewModel>(content, JsonOptions);
      return View(category);
    }
    return NotFound();
  }

  [HttpPost, ActionName("DeleteCategory")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteCategoryConfirmed(int id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.DeleteAsync($"/api/Category/{id}");
    if (response.IsSuccessStatusCode)
    {
      return RedirectToAction(nameof(Categories));
    }
    // Handle error
    TempData["ErrorMessage"] = await ExtractErrorMessage(response);
    return RedirectToAction(nameof(Categories));
  }

  // ================= NEWS =================
  public async Task<IActionResult> News(int page = 1, int? tagId = null)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    await PrepareViewBag();

    var accountId = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(accountId)) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    // Filter: Created by me OR Updated by me
    var filter = $"(CreatedById eq {accountId} or UpdatedById eq {accountId})";
    if (tagId.HasValue)
    {
      filter += $" and NewsTags/any(nt: nt/TagId eq {tagId.Value})";
    }

    var url = $"/api/NewsArticle?$filter={filter}&$orderby=CreatedDate desc&$expand=Category,NewsTags($expand=Tag)";

    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, JsonOptions) ?? [];

      // Client-side pagination (since we fetched filtered list)
      const int pageSize = 5;
      var pagedNews = news.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = (int)Math.Ceiling((double)news.Count / pageSize);

      return View(pagedNews);
    }
    return View(new List<NewsArticleViewModel>());
  }



  [HttpGet]
  public async Task<IActionResult> CreateNews()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    await PrepareViewBag();
    return PartialView("~/Presentation/Views/Staff/_CreateNewsPartial.cshtml");
  }

  [HttpPost]
  public async Task<IActionResult> CreateNews(NewsArticleViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });

    // Manual Validation or Logic before sending
    model.CreatedDate = DateTime.Now;
    model.ModifiedDate = DateTime.Now;
    model.NewsStatus = true;
    model.CreatedById = short.Parse(HttpContext.Session.GetString("AccountId") ?? "0");
    model.NewsArticleId = Guid.NewGuid().ToString()[..20]; // Generate ID

    var client = httpClientFactory.CreateClient("CoreClient");
    if (ModelState.IsValid)
    {
      var multipartContent = CreateMultipartContent(model);
      var response = await client.PostAsync("/api/NewsArticle", multipartContent);

      if (response.IsSuccessStatusCode)
      {
        // Trigger SignalR Notification
        // REMOVED: SignalR Notification is handled by Backend NewsArticleService
        return Json(new { success = true });
      }
      var error = await response.Content.ReadAsStringAsync();
      ModelState.AddModelError("", $"Failed to create news: {error}"); // Capture backend error
    }
    await PrepareViewBag();
    return PartialView("~/Presentation/Views/Staff/_CreateNewsPartial.cshtml", model);
  }

  [HttpGet]
  public async Task<IActionResult> EditNews(string? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/NewsArticle/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, JsonOptions);
      if (news == null) return NotFound();

      // Map NewsTags to SelectedTagIds for Checkboxes
      if (news.NewsTags.Count > 0)
      {
        news.Tags = [.. news.NewsTags.Select(nt => nt.Tag)];
        news.SelectedTagIds = [.. news.NewsTags.Select(nt => nt.TagId)];
      }

      await PrepareViewBag();
      return PartialView("~/Presentation/Views/Staff/_EditNewsPartial.cshtml", news);
    }
    return NotFound();
  }

  [HttpPost]
  public async Task<IActionResult> EditNews(string id, NewsArticleViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
    if (id != model.NewsArticleId) return NotFound();

    model.ModifiedDate = DateTime.Now;
    model.UpdatedById = short.Parse(HttpContext.Session.GetString("AccountId") ?? "0");

    var client = httpClientFactory.CreateClient("CoreClient");
    if (ModelState.IsValid)
    {
      var multipartContent = CreateMultipartContent(model);
      var response = await client.PutAsync($"/api/NewsArticle/{id}", multipartContent);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }
      var error = await response.Content.ReadAsStringAsync();
      ModelState.AddModelError("", $"Failed to update news: {error}");
    }
    await PrepareViewBag();
    return PartialView("~/Presentation/Views/Staff/_EditNewsPartial.cshtml", model);
  }

  private static MultipartFormDataContent CreateMultipartContent(NewsArticleViewModel model)
  {
    var content = new MultipartFormDataContent();

    if (model.NewsArticleId != null) content.Add(new StringContent(model.NewsArticleId), nameof(model.NewsArticleId));
    if (model.NewsTitle != null) content.Add(new StringContent(model.NewsTitle), nameof(model.NewsTitle));
    if (model.Headline != null) content.Add(new StringContent(model.Headline), nameof(model.Headline));
    if (model.NewsContent != null) content.Add(new StringContent(model.NewsContent), nameof(model.NewsContent));
    if (model.NewsSource != null) content.Add(new StringContent(model.NewsSource), nameof(model.NewsSource));
    if (model.NewsImage != null) content.Add(new StringContent(model.NewsImage), nameof(model.NewsImage));

    if (model.CategoryId.HasValue) content.Add(new StringContent(model.CategoryId.Value.ToString()), nameof(model.CategoryId));
    content.Add(new StringContent(model.NewsStatus.ToString()), nameof(model.NewsStatus));
    if (model.CreatedById.HasValue) content.Add(new StringContent(model.CreatedById.Value.ToString()), nameof(model.CreatedById));
    if (model.UpdatedById.HasValue) content.Add(new StringContent(model.UpdatedById.Value.ToString()), nameof(model.UpdatedById));

    // Tags
    if (model.SelectedTagIds.Count > 0)
    {
      foreach (var tagId in model.SelectedTagIds)
      {
        content.Add(new StringContent(tagId.ToString()), "SelectedTagIds");
      }
    }

    // Image File
    if (model.ImageFile != null)
    {
      var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
      content.Add(streamContent, nameof(model.ImageFile), model.ImageFile.FileName);
    }

    return content;
  }

  public async Task<IActionResult> History(int page = 1)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var accountId = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(accountId)) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");

    // Filter by CreatedById = accountId
    var url = $"/api/NewsArticle?$filter=CreatedById eq {accountId}&$orderby=CreatedDate desc&$expand=Category,NewsTags($expand=Tag)";

    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, JsonOptions) ?? [];

      // Pagination
      const int pageSize = 5;
      var pagedNews = news.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = (int)Math.Ceiling((double)news.Count / pageSize);

      return View(pagedNews);
    }
    return View(new List<NewsArticleViewModel>());
  }

  [HttpGet]
  public async Task<IActionResult> DeleteNews(string? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/NewsArticle/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, JsonOptions);
      return View(news);
    }
    return NotFound();
  }

  [HttpPost, ActionName("DeleteNews")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteNewsConfirmed(string id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.DeleteAsync($"/api/NewsArticle/{id}");
    if (response.IsSuccessStatusCode)
    {
      return RedirectToAction(nameof(News));
    }

    TempData["ErrorMessage"] = await ExtractErrorMessage(response);
    return RedirectToAction(nameof(News));
  }

  public async Task<IActionResult> DuplicateNews(string? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var userId = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.PostAsync($"/api/NewsArticle/{id}/Duplicate?userId={userId}", null);

    if (response.IsSuccessStatusCode)
    {
      return RedirectToAction(nameof(News));
    }
    return View("Error"); // Or specific error view
  }



  private async Task PrepareViewBag()
  {
    var client = httpClientFactory.CreateClient("CoreClient");

    // Get Categories
    var catResponse = await client.GetAsync("/api/Category");
    if (catResponse.IsSuccessStatusCode)
    {
      var content = await catResponse.Content.ReadAsStringAsync();
      ViewData["Categories"] = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, JsonOptions);
    }

    ViewData["Categories"] ??= new List<CategoryViewModel>();

    // Get Tags
    var tagResponse = await client.GetAsync("/api/Tag");
    if (tagResponse.IsSuccessStatusCode)
    {
      var content = await tagResponse.Content.ReadAsStringAsync();
      ViewData["Tags"] = JsonSerializer.Deserialize<List<TagViewModel>>(content, JsonOptions);
    }

    ViewData["Tags"] ??= new List<TagViewModel>();
  }

  // ================= TAGS =================
  public async Task<IActionResult> Tags()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync("/api/Tag");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var tags = JsonSerializer.Deserialize<List<TagViewModel>>(content, JsonOptions);
      return View(tags);
    }
    return View(new List<TagViewModel>());
  }

  [HttpGet]
  public IActionResult CreateTag()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    return PartialView("~/Presentation/Views/Staff/_CreateTagPartial.cshtml");
  }

  [HttpPost]
  public async Task<IActionResult> CreateTag(TagViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
    if (ModelState.IsValid)
    {
      var client = httpClientFactory.CreateClient("CoreClient");
      var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
      var response = await client.PostAsync("/api/Tag", jsonContent);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }
      var error = await response.Content.ReadAsStringAsync();
      ModelState.AddModelError("", $"Failed to create tag: {error}");
    }
    return PartialView("~/Presentation/Views/Staff/_CreateTagPartial.cshtml", model);
  }

  [HttpGet]
  public async Task<IActionResult> EditTag(int? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/Tag/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var tag = JsonSerializer.Deserialize<TagViewModel>(content, JsonOptions);
      return PartialView("~/Presentation/Views/Staff/_EditTagPartial.cshtml", tag);
    }
    return NotFound();
  }

  [HttpPost]
  public async Task<IActionResult> EditTag(int id, TagViewModel model)
  {
    if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
    if (id != model.TagId) return NotFound();

    if (ModelState.IsValid)
    {
      var client = httpClientFactory.CreateClient("CoreClient");
      var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
      var response = await client.PutAsync($"/api/Tag/{id}", jsonContent);

      if (response.IsSuccessStatusCode)
      {
        return Json(new { success = true });
      }
      ModelState.AddModelError("", "Failed to update tag.");
    }
    return PartialView("~/Presentation/Views/Staff/_EditTagPartial.cshtml", model);
  }

  [HttpGet]
  public async Task<IActionResult> DeleteTag(int? id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    if (id == null) return NotFound();

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/Tag/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var tag = JsonSerializer.Deserialize<TagViewModel>(content, JsonOptions);
      return View(tag);
    }
    return NotFound();
  }

  [HttpPost, ActionName("DeleteTag")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteTagConfirmed(int id)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");
    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.DeleteAsync($"/api/Tag/{id}");
    if (response.IsSuccessStatusCode)
    {
      return RedirectToAction(nameof(Tags));
    }
    TempData["ErrorMessage"] = await ExtractErrorMessage(response);
    return RedirectToAction(nameof(Tags));
  }


  // ================= PROFILE =================
  [HttpGet]
  public async Task<IActionResult> Profile()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var id = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(id)) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/SystemAccount/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var account = JsonSerializer.Deserialize<SystemAccountViewModel>(content, JsonOptions);

      // Fetch article statistics for Quick Stats
      try
      {
        // Get total articles by this user
        var totalResponse = await client.GetAsync($"/api/NewsArticle/Report?createdById={id}&pageSize=1");
        if (totalResponse.IsSuccessStatusCode)
        {
          var totalContent = await totalResponse.Content.ReadAsStringAsync();
          var totalResult = JsonSerializer.Deserialize<ReportResult>(totalContent, JsonOptions);
          ViewBag.TotalArticles = totalResult?.TotalRecords ?? 0;
        }
        else
        {
          ViewBag.TotalArticles = 0;
        }

        // Get published articles count (NewsStatus = true)
        var publishedResponse = await client.GetAsync($"/api/NewsArticle/Report?createdById={id}&newsStatus=true&pageSize=1");
        if (publishedResponse.IsSuccessStatusCode)
        {
          var publishedContent = await publishedResponse.Content.ReadAsStringAsync();
          var publishedResult = JsonSerializer.Deserialize<ReportResult>(publishedContent, JsonOptions);
          ViewBag.PublishedArticles = publishedResult?.TotalRecords ?? 0;
        }
        else
        {
          ViewBag.PublishedArticles = 0;
        }

        // Get draft articles count (NewsStatus = false)
        var draftResponse = await client.GetAsync($"/api/NewsArticle/Report?createdById={id}&newsStatus=false&pageSize=1");
        if (draftResponse.IsSuccessStatusCode)
        {
          var draftContent = await draftResponse.Content.ReadAsStringAsync();
          var draftResult = JsonSerializer.Deserialize<ReportResult>(draftContent, JsonOptions);
          ViewBag.DraftArticles = draftResult?.TotalRecords ?? 0;
        }
        else
        {
          ViewBag.DraftArticles = 0;
        }
      }
      catch
      {
        ViewBag.TotalArticles = 0;
        ViewBag.PublishedArticles = 0;
        ViewBag.DraftArticles = 0;
      }

      return View(account);
    }
    return NotFound();
  }

  [HttpGet]
  public async Task<IActionResult> EditProfile()
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var id = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(id)) return RedirectToAction("Login", "Account");

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync($"/api/SystemAccount/{id}");
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var account = JsonSerializer.Deserialize<SystemAccountViewModel>(content, JsonOptions);
      return View(account);
    }
    return NotFound();
  }

  [HttpPost]
  public async Task<IActionResult> EditProfile(SystemAccountViewModel model)
  {
    if (!IsStaff()) return RedirectToAction("Login", "Account");

    var idStr = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(idStr) || model.AccountId.ToString() != idStr) return BadRequest();

    var client = httpClientFactory.CreateClient("CoreClient");

    // Fetch existing first to verify password
    var existingResponse = await client.GetAsync($"/api/SystemAccount/{idStr}");
    SystemAccountViewModel? existing = null;
    if (existingResponse.IsSuccessStatusCode)
    {
      var existingContent = await existingResponse.Content.ReadAsStringAsync();
      existing = JsonSerializer.Deserialize<SystemAccountViewModel>(existingContent, JsonOptions);
    }

    if (existing == null) return NotFound();

    // Password Verification Logic
    if (!string.IsNullOrEmpty(model.NewPassword))
    {
      if (string.IsNullOrEmpty(model.OldPassword))
      {
        ModelState.AddModelError("OldPassword", "Old Password is required to set a new password.");
      }
      else if (model.OldPassword != existing.AccountPassword)
      {
        ModelState.AddModelError("OldPassword", "The Old Password is incorrect.");
      }
      else
      {
        model.AccountPassword = model.NewPassword;
      }
    }
    else
    {
      // Keep old password
      model.AccountPassword = existing.AccountPassword;
    }

    if (ModelState.IsValid)
    {
      model.AccountRole = existing.AccountRole; // Prevent role change

      var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
      // The API SystemAccountService.UpdateAccount takes SystemAccount.
      // SystemAccount has AccountPassword. If we set it to NewPassword, it updates.

      var response = await client.PutAsync($"/api/SystemAccount/{model.AccountId}", jsonContent);

      if (response.IsSuccessStatusCode)
      {
        // Update Session Name if changed
        HttpContext.Session.SetString("AccountName", model.AccountName ?? "");
        return RedirectToAction(nameof(Profile));
      }
      ModelState.AddModelError("", "Failed to update profile via API.");
    }
    return View(model);
  }

  [HttpGet]
  public async Task<IActionResult> FilterNews(int page = 1, string keyword = "", int? categoryId = null, bool? status = null, int? tagId = null)
  {
    if (!IsStaff()) return Unauthorized();

    var accountId = HttpContext.Session.GetString("AccountId");
    if (string.IsNullOrEmpty(accountId)) return Unauthorized();

    var query = new List<string>();
    var filters = new List<string>
        {
            $"(CreatedById eq {accountId} or UpdatedById eq {accountId})"
        };

    if (!string.IsNullOrEmpty(keyword))
    {
      string k = keyword.Replace("'", "''");
      var orConditions = new List<string>
            {
                $"contains(NewsTitle, '{k}')",
                $"contains(Headline, '{k}')",
            };
      filters.Add($"({string.Join(" or ", orConditions)})");
    }

    if (categoryId.HasValue)
    {
      filters.Add($"CategoryId eq {categoryId.Value}");
    }

    if (status.HasValue)
    {
      filters.Add($"NewsStatus eq {status.Value.ToString().ToLower()}");
    }

    if (tagId.HasValue)
    {
      filters.Add($"NewsTags/any(nt: nt/TagId eq {tagId.Value})");
    }

    // Sort
    query.Add("$orderby=CreatedDate desc");

    // Filter
    if (filters.Count > 0)
    {
      query.Add("$filter=" + string.Join(" and ", filters));
    }

    // Expand
    query.Add("$expand=Category,NewsTags($expand=Tag)");

    string queryString = string.Join("&", query);
    var url = $"/api/NewsArticle?{queryString}";

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var allNews = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, JsonOptions) ?? [];

      // Pagination
      const int pageSize = 5;
      var pagedNews = allNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = (int)Math.Ceiling((double)allNews.Count / pageSize);

      return PartialView("~/Presentation/Views/Staff/_NewsTablePartial.cshtml", pagedNews);
    }
    return PartialView("~/Presentation/Views/Staff/_NewsTablePartial.cshtml", new List<NewsArticleViewModel>());
  }

  [HttpGet]
  public async Task<IActionResult> FilterCategories(int page = 1, string keyword = "")
  {
    if (!IsStaff()) return Unauthorized();

    var query = new List<string>();
    if (!string.IsNullOrEmpty(keyword))
    {
      string k = keyword.Replace("'", "''");
      query.Add($"$filter=contains(CategoryName, '{k}') or contains(CategoryDesciption, '{k}')");
    }

    string queryString = string.Join("&", query);
    var url = $"/api/Category?{queryString}";

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var allCats = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, JsonOptions) ?? [];

      const int pageSize = 10;
      var pagedCats = allCats.Skip((page - 1) * pageSize).Take(pageSize).ToList();

      ViewBag.CurrentPage = page;
      ViewBag.TotalPages = (int)Math.Ceiling((double)allCats.Count / pageSize);

      return PartialView("~/Presentation/Views/Staff/_CategoryTablePartial.cshtml", pagedCats);
    }
    return PartialView("~/Presentation/Views/Staff/_CategoryTablePartial.cshtml", new List<CategoryViewModel>());
  }

  [HttpGet]
  public async Task<IActionResult> FilterTags(string keyword = "")
  {
    if (!IsStaff()) return Unauthorized();

    var query = new List<string>();
    if (!string.IsNullOrEmpty(keyword))
    {
      string k = keyword.Replace("'", "''");
      // TagName search
      query.Add($"$filter=contains(TagName, '{k}')");
    }

    string queryString = string.Join("&", query);
    var url = $"/api/Tag?{queryString}";

    var client = httpClientFactory.CreateClient("CoreClient");
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var content = await response.Content.ReadAsStringAsync();
      var tags = JsonSerializer.Deserialize<List<TagViewModel>>(content, JsonOptions);
      return PartialView("~/Presentation/Views/Staff/_TagTablePartial.cshtml", tags);
    }
    return PartialView("~/Presentation/Views/Staff/_TagTablePartial.cshtml", new List<TagViewModel>());
  }
  [HttpPost]
  public async Task<IActionResult> SuggestTags([FromBody] SuggestTagsRequest request)
  {
    if (!IsStaff()) return Unauthorized();
    string content = request.Content ?? "";

    var client = httpClientFactory.CreateClient("AiClient");
    var response = await client.PostAsJsonAsync("/api/suggesttags", new { Content = content });

    if (response.IsSuccessStatusCode)
    {
      var result = await response.Content.ReadFromJsonAsync<TagResponse>();
      return Json(result?.Tags ?? []);
    }
    return BadRequest();
  }

  [HttpPost]
  public async Task<IActionResult> LearnTags([FromBody] LearnTagsRequest request)
  {
    if (!IsStaff()) return Unauthorized();
    var client = httpClientFactory.CreateClient("AiClient");
    await client.PostAsJsonAsync("/api/suggesttags/learn", request);
    return Ok();
  }

  private class TagResponse
  {
    public List<string> Tags { get; init; } = [];
  }

  public class SuggestTagsRequest
  {
    public string? Content { get; init; }
  }

  public class LearnTagsRequest
  {
    public string? Content { get; init; }
  }
  private static async Task<string> ExtractErrorMessage(HttpResponseMessage response)
  {
    var errorContent = await response.Content.ReadAsStringAsync();
    try
    {
      // Try to parse JSON "message" property
      var json = JsonSerializer.Deserialize<JsonElement>(errorContent);
      if (json.TryGetProperty("message", out var msg))
      {
        return msg.GetString() ?? "Operation failed.";
      }
      // Fallback to "detail" or other standard fields if necessary, or just raw content
      return string.IsNullOrWhiteSpace(errorContent) ? "Operation failed." : errorContent;
    }
    catch
    {
      return string.IsNullOrWhiteSpace(errorContent) ? "An unexpected error occurred." : errorContent;
    }
  }
}
