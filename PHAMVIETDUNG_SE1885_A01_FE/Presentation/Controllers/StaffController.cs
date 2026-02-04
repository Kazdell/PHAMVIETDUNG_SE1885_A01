using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A01_FE.Presentation.ViewModels;
using System.Text.Json;
using System.Text;

namespace PHAMVIETDUNG_SE1885_A01_FE.Presentation.Controllers;

public class StaffController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public StaffController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private bool IsStaff()
    {
        var role = HttpContext.Session.GetString("AccountRole");
        return role == "1"; // Assuming 1 is Staff
    }

    // ================= CATEGORIES =================
    public async Task<IActionResult> Categories(int page = 1)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync("/api/Category");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Pagination
            int pageSize = 5;
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
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        
        var client = _httpClientFactory.CreateClient("CoreClient");
        // Re-populate Parent Categories for Dropdown
        var response = await client.GetAsync("/api/Category");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewBag.ParentCategories = allCategories;
        } else {
             ViewBag.ParentCategories = new List<CategoryViewModel>();
        }

        return PartialView("_CreateCategoryPartial");
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryViewModel model)
    {
        if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
        
        var client = _httpClientFactory.CreateClient("CoreClient");
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
            var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewBag.ParentCategories = allCategories;
        } else {
             ViewBag.ParentCategories = new List<CategoryViewModel>();
        }
        return PartialView("_CreateCategoryPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int? id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/Category/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var category = JsonSerializer.Deserialize<CategoryViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Re-populate Parent Categories
             var catResponse = await client.GetAsync("/api/Category");
            if (catResponse.IsSuccessStatusCode)
            {
                var catContent = await catResponse.Content.ReadAsStringAsync();
                var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.ParentCategories = allCategories.Where(c => c.CategoryId != id).ToList(); // Exclude self
            } else {
                 ViewBag.ParentCategories = new List<CategoryViewModel>();
            }

            return PartialView("_EditCategoryPartial", category);
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EditCategory(int id, CategoryViewModel model)
    {
        if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
        if (id != model.CategoryId) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
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
            var allCategories = JsonSerializer.Deserialize<List<CategoryViewModel>>(catContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewBag.ParentCategories = allCategories.Where(c => c.CategoryId != id).ToList();
        } else {
             ViewBag.ParentCategories = new List<CategoryViewModel>();
        }
        return PartialView("_EditCategoryPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/Category/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var category = JsonSerializer.Deserialize<CategoryViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(category);
        }
        return NotFound();
    }

    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.DeleteAsync($"/api/Category/{id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Categories));
        }
        // Handle error
        var errorContent = await response.Content.ReadAsStringAsync();
        TempData["ErrorMessage"] = string.IsNullOrEmpty(errorContent) ? "Failed to delete category." : errorContent;
        if (errorContent.Contains("used by news articles") || errorContent.Contains("REFERENCE constraint"))
        {
             TempData["ErrorMessage"] = "This category has articles; cannot delete.";
        }
        return RedirectToAction(nameof(Categories));
    }

    // ================= NEWS =================
    public async Task<IActionResult> News()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync("/api/NewsArticle");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(news);
        }
        return View(new List<NewsArticleViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> CreateNews()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        await PrepareViewBag();
        return PartialView("_CreateNewsPartial");
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
        model.NewsArticleId = Guid.NewGuid().ToString().Substring(0, 20); // Generate ID

        var client = _httpClientFactory.CreateClient("CoreClient");
        if (ModelState.IsValid)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/NewsArticle", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                 return Json(new { success = true });
            }
            ModelState.AddModelError("", "Failed to create news.");
        }
        await PrepareViewBag();
        return PartialView("_CreateNewsPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditNews(string id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/NewsArticle/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Map NewsTags to SelectedTagIds for Checkboxes
            if (news.NewsTags != null)
            {
                news.Tags = news.NewsTags.Select(nt => nt.Tag).ToList();
                news.SelectedTagIds = news.NewsTags.Select(nt => nt.TagId).ToList();
            }

            await PrepareViewBag();
            return PartialView("_EditNewsPartial", news);
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

        var client = _httpClientFactory.CreateClient("CoreClient");
        if (ModelState.IsValid)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/NewsArticle/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                 return Json(new { success = true });
            }
            ModelState.AddModelError("", "Failed to update news.");
        }
        await PrepareViewBag();
        return PartialView("_EditNewsPartial", model);
    }

        public async Task<IActionResult> History(int page = 1)
        {
            if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
            
            var accountId = HttpContext.Session.GetString("AccountId");
            if (string.IsNullOrEmpty(accountId)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("CoreClient");

            // Filter by CreatedById = accountId
            var url = $"/api/NewsArticle?$filter=CreatedById eq {accountId}&$orderby=CreatedDate desc&$expand=Category,NewsTags($expand=Tag)";
            
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var news = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // Pagination
                int pageSize = 5;
                var pagedNews = news.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)news.Count / pageSize);
                
                return View(pagedNews);
            }
            return View(new List<NewsArticleViewModel>());
        }

        [HttpGet]
    public async Task<IActionResult> DeleteNews(string id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/NewsArticle/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var news = JsonSerializer.Deserialize<NewsArticleViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(news);
        }
        return NotFound();
    }

    [HttpPost, ActionName("DeleteNews")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNewsConfirmed(string id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.DeleteAsync($"/api/NewsArticle/{id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(News));
        }
        return RedirectToAction(nameof(News));
    }

    public async Task<IActionResult> DuplicateNews(string id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        var userId = HttpContext.Session.GetString("AccountId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.PostAsync($"/api/NewsArticle/{id}/Duplicate?userId={userId}", null);
        
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(News));
        }
        return View("Error"); // Or specific error view
    }



    private async Task PrepareViewBag()
    {
        var client = _httpClientFactory.CreateClient("CoreClient");

        // Get Categories
        var catResponse = await client.GetAsync("/api/Category");
        if (catResponse.IsSuccessStatusCode)
        {
            var content = await catResponse.Content.ReadAsStringAsync();
            ViewData["Categories"] = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        
        if (ViewData["Categories"] == null)
        {
            ViewData["Categories"] = new List<CategoryViewModel>();
        }
        
        // Get Tags
        var tagResponse = await client.GetAsync("/api/Tag");
        if (tagResponse.IsSuccessStatusCode)
        {
             var content = await tagResponse.Content.ReadAsStringAsync();
             ViewData["Tags"] = JsonSerializer.Deserialize<List<TagViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        if (ViewData["Tags"] == null)
        {
            ViewData["Tags"] = new List<TagViewModel>();
        }
    }

    // ================= TAGS =================
    public async Task<IActionResult> Tags()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync("/api/Tag");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<TagViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(tags);
        }
        return View(new List<TagViewModel>());
    }

    [HttpGet]
    public IActionResult CreateTag()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        return PartialView("_CreateTagPartial");
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag(TagViewModel model)
    {
        if (!IsStaff()) return Json(new { success = false, message = "Access Denied" });
        if (ModelState.IsValid)
        {
            var client = _httpClientFactory.CreateClient("CoreClient");
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/Tag", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to create tag: {error}");
        }
        return PartialView("_CreateTagPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditTag(int? id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/Tag/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tag = JsonSerializer.Deserialize<TagViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return PartialView("_EditTagPartial", tag);
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
            var client = _httpClientFactory.CreateClient("CoreClient");
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/Tag/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                 return Json(new { success = true });
            }
            ModelState.AddModelError("", "Failed to update tag.");
        }
        return PartialView("_EditTagPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteTag(int? id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        if (id == null) return NotFound();

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/Tag/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tag = JsonSerializer.Deserialize<TagViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(tag);
        }
        return NotFound();
    }

    [HttpPost, ActionName("DeleteTag")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTagConfirmed(int id)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.DeleteAsync($"/api/Tag/{id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Tags));
        }
        return RedirectToAction(nameof(Tags));
    }


    // ================= PROFILE =================
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        
        var id = HttpContext.Session.GetString("AccountId");
        if (string.IsNullOrEmpty(id)) return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/SystemAccount/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<SystemAccountViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(account);
        }
        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");

        var id = HttpContext.Session.GetString("AccountId");
        if (string.IsNullOrEmpty(id)) return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync($"/api/SystemAccount/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<SystemAccountViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(account);
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(SystemAccountViewModel model)
    {
        if (!IsStaff()) return RedirectToAction("AccessDenied", "Account");
        
        var idStr = HttpContext.Session.GetString("AccountId");
        if (string.IsNullOrEmpty(idStr) || model.AccountId.ToString() != idStr) return BadRequest();

        var client = _httpClientFactory.CreateClient("CoreClient");

        // Fetch existing first to verify password
        var existingResponse = await client.GetAsync($"/api/SystemAccount/{idStr}");
        SystemAccountViewModel existing = null;
        if (existingResponse.IsSuccessStatusCode)
        {
             var existingContent = await existingResponse.Content.ReadAsStringAsync();
             existing = JsonSerializer.Deserialize<SystemAccountViewModel>(existingContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                 HttpContext.Session.SetString("AccountName", model.AccountName);
                 return RedirectToAction(nameof(Profile));
             }
             ModelState.AddModelError("", "Failed to update profile via API.");
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> FilterNews(int page = 1, string keyword = "", int? categoryId = null, bool? status = null)
    {
        if (!IsStaff()) return Unauthorized();

        var query = new List<string>();
        var filters = new List<string>();

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

        // Sort
        query.Add("$orderby=CreatedDate desc");

        // Filter
        if (filters.Any())
        {
            query.Add("$filter=" + string.Join(" and ", filters));
        }
        
        // Expand
        query.Add("$expand=Category,NewsTags($expand=Tag)");

        string queryString = string.Join("&", query);
        var url = $"/api/NewsArticle?{queryString}";

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var allNews = JsonSerializer.Deserialize<List<NewsArticleViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Pagination
            int pageSize = 5;
            var pagedNews = allNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)allNews.Count / pageSize);
            
            return PartialView("_NewsTablePartial", pagedNews);
        }
        return PartialView("_NewsTablePartial", new List<NewsArticleViewModel>());
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

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var allCats = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            int pageSize = 5;
            var pagedCats = allCats.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)allCats.Count / pageSize);
            
            return PartialView("_CategoryTablePartial", pagedCats);
        }
        return PartialView("_CategoryTablePartial", new List<CategoryViewModel>());
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

        var client = _httpClientFactory.CreateClient("CoreClient");
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<TagViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return PartialView("_TagTablePartial", tags);
        }
        return PartialView("_TagTablePartial", new List<TagViewModel>());
    }
    [HttpPost]
    public async Task<IActionResult> SuggestTags([FromBody] string content)
    {
        if (!IsStaff()) return Unauthorized();
        
        var client = _httpClientFactory.CreateClient("AiClient");
        var response = await client.PostAsJsonAsync("/api/suggesttags", new { Content = content });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TagResponse>();
            return Json(result?.Tags ?? new List<string>());
        }
        return BadRequest();
    }

    private class TagResponse
    {
        public List<string> Tags { get; set; }
    }
}
