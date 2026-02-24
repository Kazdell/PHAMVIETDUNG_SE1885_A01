using System.Diagnostics;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Middleware
{
  public class RequestLoggingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
      var stopwatch = Stopwatch.StartNew();

      // Log Request
      _logger.LogInformation("Incoming Request: {Method} {Path}", context.Request.Method, context.Request.Path);

      await _next(context);

      stopwatch.Stop();

      // Log Response
      _logger.LogInformation("Outgoing Response: {StatusCode} for {Method} {Path} - Taken {Elapsed}ms",
          context.Response.StatusCode,
          context.Request.Method,
          context.Request.Path,
          stopwatch.ElapsedMilliseconds);
    }
  }
}
