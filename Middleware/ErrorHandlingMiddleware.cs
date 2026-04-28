using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Global error-handling middleware.
    /// Catches any unhandled exception and returns a consistent JSON error response.
    /// Must be registered FIRST in the pipeline so it wraps all other middleware.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                KeyNotFoundException     => (HttpStatusCode.NotFound,            "The requested resource was not found."),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,     "Unauthorized access."),
                ArgumentException        => (HttpStatusCode.BadRequest,          exception.Message),
                InvalidOperationException => (HttpStatusCode.Conflict,           exception.Message),
                _                        => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                error   = message,
                // Only expose stack trace in Development mode
                detail  = _env.IsDevelopment() ? exception.ToString() : null
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
