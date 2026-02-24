using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AccessToken") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpClientFactory.CreateClient("CoreClient");
            var loginData = new { Email = email, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;
                
                var accessToken = root.GetProperty("accessToken").GetString();
                var refreshToken = root.GetProperty("refreshToken").GetString();
                // Assumes API returns Role. If not, we might need to decode JWT here.
                // For now, let's assume successful login allows access.

                HttpContext.Session.SetString("AccessToken", accessToken ?? "");
                if (refreshToken != null) HttpContext.Session.SetString("RefreshToken", refreshToken);

                if (root.TryGetProperty("user", out var userElement) || root.TryGetProperty("User", out userElement))
                {
                    if (userElement.TryGetProperty("accountId", out var idProp) || userElement.TryGetProperty("AccountId", out idProp))
                        HttpContext.Session.SetString("AccountId", idProp.ToString());

                    if (userElement.TryGetProperty("accountName", out var nameProp) || userElement.TryGetProperty("AccountName", out nameProp))
                         HttpContext.Session.SetString("AccountName", nameProp.ToString());

                    if (userElement.TryGetProperty("accountRole", out var roleProp) || userElement.TryGetProperty("AccountRole", out roleProp))
                         HttpContext.Session.SetString("AccountRole", roleProp.ToString());
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
