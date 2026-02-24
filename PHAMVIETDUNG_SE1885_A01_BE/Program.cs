using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;

using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FUNewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IGenericRepository<NewsTag>, GenericRepository<NewsTag>>();
builder.Services.AddScoped<IGenericRepository<NewsView>, GenericRepository<NewsView>>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddMemoryCache();
builder.Services.AddLazyCache(); // Application-level caching for Categories/Tags

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Register HttpClients for external APIs
builder.Services.AddHttpClient("AiClient", client => { client.BaseAddress = new Uri("http://localhost:5200"); });
builder.Services.AddHttpClient("AnalyticsClient", client => { client.BaseAddress = new Uri("http://localhost:5100"); });

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<TokenService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .AddOData(opt => opt.Select().Filter().Count().OrderBy().Expand().SetMaxTop(100).AddRouteComponents("odata", GetEdmModel()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:5260", "http://localhost:44021", "http://localhost:5000", "http://localhost:5100", "http://localhost:5200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); 

app.UseMiddleware<PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Middleware.GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Middleware.RequestLoggingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication(); // Added
app.UseAuthorization();

app.MapControllers();
app.MapHub<PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Hubs.AdminDashboardHub>("/hubs/admindashboard");

app.Run();

static IEdmModel GetEdmModel()
{
    var odataBuilder = new ODataConventionModelBuilder();
    odataBuilder.EntitySet<SystemAccount>("SystemAccounts");
    odataBuilder.EntitySet<Category>("Categories");
    odataBuilder.EntitySet<NewsArticle>("NewsArticles");
    odataBuilder.EntitySet<Tag>("Tags");
    odataBuilder.EntitySet<NewsTag>("NewsTags");
    return odataBuilder.GetEdmModel();
}

public partial class Program { }

public partial class Program { }
