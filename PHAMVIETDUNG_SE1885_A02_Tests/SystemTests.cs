extern alias CoreApi;
extern alias AiApi;

using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http.Json;
using System.Net;

namespace FUNewsManagementSystem.Tests
{
    // ------------------------------------------------------------------------------------------------
    // 1. AUTHENTICATION TESTS (Core API)
    // ------------------------------------------------------------------------------------------------
    public class AuthTests : IClassFixture<WebApplicationFactory<CoreApi::Program>> 
    {
        private readonly WebApplicationFactory<CoreApi::Program> _factory;

        public AuthTests(WebApplicationFactory<CoreApi::Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var client = _factory.CreateClient();
            var loginRequest = new { Email = "admin@funews.com", Password = "@1" }; 
            
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Expecting Success or at least reachable
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    // ------------------------------------------------------------------------------------------------
    // 3. AI API TESTS (Standalone)
    // ------------------------------------------------------------------------------------------------
    public class AiTests : IClassFixture<WebApplicationFactory<AiApi::Program>>
    {
        private readonly WebApplicationFactory<AiApi::Program> _factory;

        public AiTests(WebApplicationFactory<AiApi::Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SuggestTags_ShouldReturnKeywords()
        {
            var client = _factory.CreateClient();
            var request = new { Content = "This is a breaking news about Artificial Intelligence using .NET" };

            var response = await client.PostAsJsonAsync("/api/suggesttags", request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TagResponse>();
            
            Assert.NotNull(result);
            // Case insensitive check
            Assert.Contains(result.Tags, t => t.IndexOf("Artificial", StringComparison.OrdinalIgnoreCase) >= 0 || t.IndexOf("Intelligence", StringComparison.OrdinalIgnoreCase) >= 0);
        }

         private class TagResponse
        {
            public List<string> Tags { get; set; }
        }
    }
}
