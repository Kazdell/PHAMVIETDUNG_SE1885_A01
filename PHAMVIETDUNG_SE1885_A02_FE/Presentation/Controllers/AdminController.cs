using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.Controllers
{
  public class AdminController(IHttpClientFactory httpClientFactory) : Controller
  {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private bool IsAdmin() => HttpContext.Session.GetString("AccountRole") == "0";

    public async Task<IActionResult> Dashboard()
    {
      if (!IsAdmin()) return View("AccessDenied");

      var client = httpClientFactory.CreateClient("AnalyticsClient");
      try
      {
        var response = await client.GetAsync("/api/analytics/dashboard");
        if (response.IsSuccessStatusCode)
        {
          var responseString = await response.Content.ReadAsStringAsync();
          ViewBag.DashboardStats = responseString;
          return View();
        }
      }
      catch (Exception)
      {
        ViewBag.Error = "Unable to connect to Analytics Service.";
      }

      return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats(DateTime? fromDate = null, DateTime? toDate = null, bool? status = null)
    {
      if (!IsAdmin()) return Unauthorized();

      var client = httpClientFactory.CreateClient("AnalyticsClient");
      try
      {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (status.HasValue) queryParams.Add($"status={status.Value.ToString().ToLower()}");

        var url = "/api/analytics/dashboard" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStringAsync();
          return Content(content, "application/json");
        }
        return StatusCode((int)response.StatusCode);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }

    public IActionResult ExportReports()
    {
      return Redirect("http://localhost:5100/api/analytics/export");
    }

    public async Task<IActionResult> AuditLog(string? userEmail = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
    {
      if (!IsAdmin()) return View("AccessDenied");

      var client = httpClientFactory.CreateClient("CoreClient");

      try
      {
        var queryParams = new List<string> { $"page={page}" };
        if (!string.IsNullOrEmpty(userEmail)) queryParams.Add($"userEmail={Uri.EscapeDataString(userEmail)}");
        if (!string.IsNullOrEmpty(entityType)) queryParams.Add($"entityType={Uri.EscapeDataString(entityType)}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

        var entitiesTask = client.GetAsync("/api/AuditLog/entities");
        var usersTask = client.GetAsync("/api/AuditLog/users");
        var logsTask = client.GetAsync($"/api/AuditLog?{string.Join("&", queryParams)}");

        await Task.WhenAll(entitiesTask, usersTask, logsTask);

        var entitiesResponse = await entitiesTask;
        var usersResponse = await usersTask;
        var response = await logsTask;

        ViewBag.Entities = entitiesResponse.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<List<string>>(await entitiesResponse.Content.ReadAsStringAsync(), JsonOptions) ?? []
            : [];

        ViewBag.Users = usersResponse.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<List<string>>(await usersResponse.Content.ReadAsStringAsync(), JsonOptions) ?? []
            : [];

        ViewBag.SelectedUser = userEmail;
        ViewBag.SelectedEntity = entityType;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStringAsync();
          var result = JsonSerializer.Deserialize<AuditLogPagedResult>(content, JsonOptions);
          return View(result ?? new AuditLogPagedResult());
        }
        else
        {
          ViewBag.Error = $"API returned {response.StatusCode}";
        }
      }
      catch (Exception ex)
      {
        ViewBag.Error = $"Error connecting to API: {ex.Message}";
      }

      return View(new AuditLogPagedResult());
    }

    public async Task<IActionResult> Accounts()
    {
      if (!IsAdmin()) return View("AccessDenied");

      var client = httpClientFactory.CreateClient("CoreClient");
      var response = await client.GetAsync("/api/SystemAccount");
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        var accounts = JsonSerializer.Deserialize<List<SystemAccountViewModel>>(content, JsonOptions) ?? [];

        const int pageSize = 10;
        int page = 1;
        if (Request.Query.ContainsKey("page")) _ = int.TryParse(Request.Query["page"], out page);

        var pagedAccounts = accounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)accounts.Count / pageSize);

        return View(pagedAccounts);
      }
      return View(new List<SystemAccountViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> FilterAccounts(int page = 1, string search = "", int? role = null)
    {
      if (!IsAdmin()) return Unauthorized();
      var client = httpClientFactory.CreateClient("CoreClient");
      var response = await client.GetAsync("/api/SystemAccount");

      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        var accounts = JsonSerializer.Deserialize<List<SystemAccountViewModel>>(content, JsonOptions) ?? [];

        if (!string.IsNullOrEmpty(search))
        {
          accounts = accounts.Where(a =>
            (a.AccountName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.AccountEmail?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        if (role.HasValue)
        {
          accounts = accounts.Where(a => a.AccountRole == role.Value).ToList();
        }

        const int pageSize = 10;
        var pagedAccounts = accounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)accounts.Count / pageSize);

        return PartialView("_AccountTablePartial", pagedAccounts);
      }
      return PartialView("_AccountTablePartial", new List<SystemAccountViewModel>());
    }

    public async Task<IActionResult> Report()
    {
      if (!IsAdmin()) return View("AccessDenied");

      try
      {
        var client = httpClientFactory.CreateClient("AnalyticsClient");
        var response = await client.GetAsync("/api/analytics/dashboard");
        if (response.IsSuccessStatusCode)
        {
          ViewBag.DashboardStats = await response.Content.ReadAsStringAsync();
        }
        else
        {
          ViewBag.Error = "Failed to load report data from Analytics Service.";
        }
      }
      catch (Exception ex)
      {
        ViewBag.Error = $"Analytics Service is unavailable ({ex.Message}). please ensure the API is running on port 5100.";
      }

      return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
      if (!IsAdmin()) return Unauthorized();
      return PartialView("_CreateAccountPartial");
    }

    [HttpPost]
    public async Task<IActionResult> Create(SystemAccountViewModel model)
    {
      if (!IsAdmin()) return Unauthorized();

      model.AccountId = (short)(new Random().Next(100, 30000));

      if (ModelState.IsValid)
      {
        var client = httpClientFactory.CreateClient("CoreClient");
        var content = new StringContent(JsonSerializer.Serialize(model, JsonOptions), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/SystemAccount", content);
        if (response.IsSuccessStatusCode)
        {
          return Json(new { success = true });
        }
        ModelState.AddModelError("", "Failed to create account.");
      }
      return PartialView("_CreateAccountPartial", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(short id)
    {
      if (!IsAdmin()) return Unauthorized();
      var client = httpClientFactory.CreateClient("CoreClient");
      var response = await client.GetAsync($"/api/SystemAccount/{id}");
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<SystemAccountViewModel>(content, JsonOptions);
        return PartialView("_EditAccountPartial", model);
      }
      return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(SystemAccountViewModel model)
    {
      if (!IsAdmin()) return Unauthorized();
      if (ModelState.IsValid)
      {
        var client = httpClientFactory.CreateClient("CoreClient");
        var content = new StringContent(JsonSerializer.Serialize(model, JsonOptions), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"/api/SystemAccount/{model.AccountId}", content);
        if (response.IsSuccessStatusCode)
        {
          return Json(new { success = true });
        }
        ModelState.AddModelError("", "Failed to update account.");
      }
      return PartialView("_EditAccountPartial", model);
    }

    public async Task<IActionResult> Delete(short id)
    {
      if (!IsAdmin()) return View("AccessDenied");
      var client = httpClientFactory.CreateClient("CoreClient");
      var response = await client.DeleteAsync($"/api/SystemAccount/{id}");
      if (response.IsSuccessStatusCode)
      {
        return RedirectToAction(nameof(Accounts));
      }
      TempData["ErrorMessage"] = await ExtractErrorMessage(response);
      return RedirectToAction(nameof(Accounts));
    }

    private static async Task<string> ExtractErrorMessage(HttpResponseMessage response)
    {
      var errorContent = await response.Content.ReadAsStringAsync();
      try
      {
        var json = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
        if (json.TryGetProperty("message", out var msg))
        {
          return msg.GetString() ?? "Operation failed.";
        }
        return string.IsNullOrWhiteSpace(errorContent) ? "Operation failed." : errorContent;
      }
      catch
      {
        return string.IsNullOrWhiteSpace(errorContent) ? "An unexpected error occurred." : errorContent;
      }
    }
  }
}
