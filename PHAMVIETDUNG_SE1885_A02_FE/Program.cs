using FUNewsManagementSystem.Client.Infrastructure;
using PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Hubs;
using Polly;
using WebOptimizer;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Filters.OfflineExceptionHandlerAttribute>();
    })
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/Presentation/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation/Views/Shared/{0}.cshtml");
    });
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddSession();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<PHAMVIETDUNG_SE1885_A02_FE.Infrastructure.Services.CacheRefreshWorker>();

// Register Services
builder.Services.AddWebOptimizer();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Function to define retry policy
var retryPolicy = Policy.Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.Unauthorized && r.StatusCode != System.Net.HttpStatusCode.BadRequest)
    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt)); // 1s + 2s = 3s delay

builder.Services.AddHttpClient("CoreClient", client => { client.BaseAddress = new Uri("http://localhost:5000"); })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("AnalyticsClient", client => { client.BaseAddress = new Uri("http://localhost:5100"); })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("AiClient", client => { client.BaseAddress = new Uri("http://localhost:5200"); })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddPolicyHandler(retryPolicy);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseResponseCompression();
app.UseWebOptimizer();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();

app.UseSession(); // Enable Session

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DashboardHub>("/hubs/admindashboard");

app.Run();
