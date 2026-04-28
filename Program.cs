using UserManagementAPI.Extensions;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// Service Registration
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// Register UserService as a singleton so the in-memory store persists for the
// lifetime of the application (swap with AddScoped + a real DbContext in production).
builder.Services.AddSingleton<IUserService, UserService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title       = "UserManagementAPI",
        Version     = "v1",
        Description = "TechHive Solutions — User Management REST API"
    });

    // Document the Bearer token requirement in Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "Token",
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your Bearer token. Example: demo-secret-token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// Logging (built-in console + debug)
// ─────────────────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// Middleware Pipeline  (ORDER MATTERS)
//
//  1. ErrorHandlingMiddleware  — outermost, catches everything below
//  2. AuthenticationMiddleware — validates token before any logic runs
//  3. LoggingMiddleware        — logs requests/responses after auth check
//  4. Built-in ASP.NET routing & controllers
// ─────────────────────────────────────────────────────────────────────────────

app.UseErrorHandling();       // 1 — must be first
app.UseTokenAuthentication(); // 2 — auth check
app.UseRequestLogging();      // 3 — logging

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagementAPI v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

// Friendly root response
app.MapGet("/", () => Results.Json(new
{
    api         = "UserManagementAPI",
    version     = "1.0",
    docs        = "/swagger",
    status      = "running",
    description = "TechHive Solutions User Management API"
}));

app.Run();