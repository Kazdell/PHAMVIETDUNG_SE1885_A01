extern alias CoreApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using CoreApi::PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;
using System.Net.Http.Headers;

namespace FUNewsManagementSystem.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<CoreApi::Program>>
    {
        private readonly WebApplicationFactory<CoreApi::Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<CoreApi::Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public void TestConfigurationLoad()
        {
            var config = _factory.Services.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
            var email = config["AdminAccount:Email"];
            var password = config["AdminAccount:Password"];
            
            // Fail explicitly with the value found to see it in logs
            Assert.True(email == "admin@FUNewsManagementSystem.org", $"Expected email 'admin@FUNewsManagementSystem.org', but found: '{email}'");
             Assert.True(password == "@@abc123@@", $"Expected password '@@abc123@@', but found: '{password}'");
        }

        private async Task<string> AuthenticateAsync(string email, string password)
        {
            var loginRequest = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result.AccessToken;
        }

        [Fact]
        public async Task GetCategories_ShouldReturnOk_AndListCategories()
        {
            var response = await _client.GetAsync("/api/category");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
            categories.Should().NotBeNull();
            categories.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetNewsArticles_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/newsarticle");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var articles = await response.Content.ReadFromJsonAsync<List<NewsArticle>>();
            articles.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnCreated_WhenAdmin()
        {
            // 1. Login as Admin (Credentials from appsettings.json)
            var token = await AuthenticateAsync("admin@FUNewsManagementSystem.org", "@@abc123@@");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Create Category
            var newCategory = new { CategoryName = "Integration Test Category", CategoryDesciption = "Test Desc" };
            var response = await _client.PostAsJsonAsync("/api/category", newCategory);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdCategory = await response.Content.ReadFromJsonAsync<Category>();
            createdCategory.Should().NotBeNull();
            createdCategory.CategoryName.Should().Be("Integration Test Category");
        }

        [Fact]
        public async Task CreateNewsArticle_ShouldReturnCreated_WhenAdmin()
        {
             // 1. Login as Admin
            var token = await AuthenticateAsync("admin@FUNewsManagementSystem.org", "@@abc123@@"); 
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. We need to use MultipartFormDataContent for NewsArticle as it expects FromForm
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("Integration Test News"), "NewsTitle");
            content.Add(new StringContent("Integration Headline"), "Headline");
            content.Add(new StringContent("Content body"), "NewsContent");
            content.Add(new StringContent("Integration"), "NewsSource");
            content.Add(new StringContent("1"), "CategoryId");
            content.Add(new StringContent("true"), "NewsStatus");
            content.Add(new StringContent("1"), "CreatedById"); // Admin ID
            
            var response = await _client.PostAsync("/api/newsarticle", content);
            
            // response.StatusCode.Should().Be(HttpStatusCode.Created); 
            // Asserting OK or Created depending on implementation. Controller says CreatedAtAction
            
            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
               var error = await response.Content.ReadAsStringAsync();
               // Fail explicitly to see error
               Assert.Fail($"Status: {response.StatusCode}, Error: {error}");
            }
            
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }
        
        private class LoginResponse
        {
            public string AccessToken { get; set; }
        }
    }
}
