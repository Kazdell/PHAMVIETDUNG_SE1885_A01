using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Services
{
    public class CacheRefreshWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheRefreshWorker> _logger;

        public CacheRefreshWorker(IServiceProvider serviceProvider, ILogger<CacheRefreshWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Frontend Cache Refresh Worker starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cache Refresh Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                        var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                        
                        // Pre-fetch and cache trending news
                        var client = httpClientFactory.CreateClient("AnalyticsClient");
                        
                        var response = await client.GetAsync("/api/analytics/Trending", stoppingToken);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            // Store in MemoryCache for 6 hours
                            cache.Set("CachedTrendingNews", content, TimeSpan.FromHours(6));
                            _logger.LogInformation("Trending news cached successfully in Frontend.");
                        }
                    }
                }
                catch (Exception ex)
                {
                   _logger.LogError(ex, "Error refreshing frontend cache.");
                }

                // Wait 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }

            _logger.LogInformation("Cache Refresh Worker stopping.");
        }
    }
}
