namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Simple token-based authentication middleware.
    /// Validates a Bearer token from the Authorization header.
    /// Returns 401 Unauthorized for missing or invalid tokens.
    ///
    /// For testing, add the header:  Authorization: Bearer demo-secret-token
    /// In production, replace the token store with JWT validation or OAuth.
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IConfiguration _config;

        // Endpoints that do NOT require authentication (whitelist)
        private static readonly HashSet<string> _publicPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/",
            "/swagger",
            "/swagger/index.html",
            "/favicon.ico"
        };

        public AuthenticationMiddleware(
            RequestDelegate next,
            ILogger<AuthenticationMiddleware> logger,
            IConfiguration config)
        {
            _next   = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Allow Swagger UI and root path without a token
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            // Extract token from Authorization header
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader)
                || !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[AUTH] Missing or malformed Authorization header for {Path}", path);
                await WriteUnauthorizedAsync(context, "Authorization header is missing or malformed. Expected: Bearer <token>");
                return;
            }

            var token = authHeader.ToString()["Bearer ".Length..].Trim();
            var validToken = _config["Auth:Token"] ?? "demo-secret-token";

            if (token != validToken)
            {
                _logger.LogWarning("[AUTH] Invalid token received for {Path}", path);
                await WriteUnauthorizedAsync(context, "Invalid token. Access denied.");
                return;
            }

            _logger.LogInformation("[AUTH] Token validated successfully for {Path}", path);
            await _next(context);
        }

        private static bool IsPublicPath(string path)
            => path == "/"
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/docs", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase);

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                $"{{\"success\":false,\"error\":\"{message}\"}}");
        }
    }
}