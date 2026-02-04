using FUNewsManagementSystem.Client.Infrastructure;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/Presentation/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation/Views/Shared/{0}.cshtml");
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddSession();

// Function to define retry policy
var retryPolicy = Policy.Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.Unauthorized && r.StatusCode != System.Net.HttpStatusCode.BadRequest)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

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
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable Session

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
