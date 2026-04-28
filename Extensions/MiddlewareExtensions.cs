using UserManagementAPI.Middleware;

namespace UserManagementAPI.Extensions
{
    /// <summary>
    /// Extension methods to register custom middleware cleanly in Program.cs.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>Registers global error-handling middleware (must be first).</summary>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ErrorHandlingMiddleware>();

        /// <summary>Registers token-based authentication middleware.</summary>
        public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder app)
            => app.UseMiddleware<AuthenticationMiddleware>();

        /// <summary>Registers request/response logging middleware.</summary>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<LoggingMiddleware>();
    }
}
