using System.Diagnostics;

namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware that logs every incoming HTTP request and outgoing response.
    /// Captures: HTTP method, path, status code, and elapsed time.
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var method      = context.Request.Method;
            var path        = context.Request.Path;
            var queryString = context.Request.QueryString;

            _logger.LogInformation(
                "[REQUEST]  {Method} {Path}{Query} — {Timestamp}",
                method, path, queryString, DateTime.UtcNow.ToString("o"));

            // Continue down the pipeline
            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "[RESPONSE] {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
