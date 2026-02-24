using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FUNewsManagementSystem.Client.Infrastructure
{
  public class AuthHeaderHandler : DelegatingHandler
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var context = _httpContextAccessor.HttpContext;
      if (context != null)
      {
        var token = context.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
      }

      var response = await base.SendAsync(request, cancellationToken);

      // Auto-refresh on 401 Unauthorized
      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && context != null)
      {
        var refreshToken = context.Session.GetString("RefreshToken");
        if (!string.IsNullOrEmpty(refreshToken))
        {
          var newTokens = await TryRefreshTokenAsync(refreshToken, cancellationToken);
          if (newTokens != null)
          {
            // Update session with new tokens
            context.Session.SetString("AccessToken", newTokens.AccessToken);
            context.Session.SetString("RefreshToken", newTokens.RefreshToken);

            // Retry original request with new token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);

            // Clone request for retry (can't reuse same request)
            var retryRequest = await CloneRequestAsync(request);
            response = await base.SendAsync(retryRequest, cancellationToken);
          }
        }
      }

      return response;
    }

    private async Task<TokenResponse?> TryRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
      try
      {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5000");

        var content = new StringContent(
            JsonSerializer.Serialize(new { refreshToken }),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/Auth/RefreshToken", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
          var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
          return JsonSerializer.Deserialize<TokenResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
      }
      catch
      {
        // Silently fail - will redirect to login
      }
      return null;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
      var clone = new HttpRequestMessage(request.Method, request.RequestUri);

      if (request.Content != null)
      {
        var content = await request.Content.ReadAsStringAsync();
        clone.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");
      }

      foreach (var header in request.Headers)
      {
        clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
      }

      return clone;
    }
  }

  public class TokenResponse
  {
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
  }
}
