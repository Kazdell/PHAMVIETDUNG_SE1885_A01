namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
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
            _logger.LogInformation("Cache Refresh Worker starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cache Refresh Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var cacheService = scope.ServiceProvider.GetRequiredService<IAiCacheService>();
                        cacheService.RefreshTrendingCache();
                        _logger.LogInformation("Trending Cache Refreshed.");
                    }
                }
                catch (Exception ex)
                {
                   _logger.LogError(ex, "Error refreshing cache.");
                }

                // Wait 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }

            _logger.LogInformation("Cache Refresh Worker stopping.");
        }
    }
}
