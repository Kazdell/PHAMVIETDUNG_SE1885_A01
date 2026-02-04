using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A01_FE.Presentation.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("AnalyticsClient");
            try 
            {
                var response = await client.GetAsync("/api/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Pass JSON string directly to view for Chart.js parsing
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

        public IActionResult ExportReports()
        {
             // Redirect user browser to download directly (or proxy if needed, but direct is easier if Auth allowed)
             // Since Auth is Header-based, we might need a proxy action here if we want to attach token.
             // For simplicity in this phase, let's assume we proxy it or just show link.
             return Redirect("http://localhost:5100/api/export"); 
        }
    }
}
